using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNet.Identity.Dapper;
using TestAPI.Models;
using TestAPI.Repository.Providers;
using System.Data.SqlClient;
using Dapper;
using System.Web;
using Microsoft.AspNet.Identity.Owin;


namespace TestAPI.Repository
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterUser(UserModel userModel);
        Task<AppMember> FindUser(string userName, string password);
        Client FindClient(string clientId);
        Task<bool> AddRefreshToken(RefreshToken token);
        Task<bool> RemoveRefreshToken(string refreshTokenId);
        Task<bool> RemoveRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> FindRefreshToken(string refreshTokenId);
        Task<AppMember> FindAsync(UserLoginInfo loginInfo);
        Task<IdentityResult> CreateAsync(AppMember user);
        Task<IdentityResult> AddLoginAsync(int userId, UserLoginInfo login);
        List<RefreshToken> GetAllRefreshTokens();
        Task<AppMember> FindUserByUsername(string userName);
    }

    public class AuthRepository :  IAuthRepository
    {
        //private AuthContext _ctx;

        private ApplicationUserManager _userManager;

        private readonly ICacheProvider _cacheProvider;
        private readonly IConnectionStringProvider _connectionStringProvider;

        public AuthRepository(ICacheProvider cacheProvider, IConnectionStringProvider connectionStringProvider, ApplicationUserManager userManager = null)
        {
            _cacheProvider = cacheProvider;
            _connectionStringProvider = connectionStringProvider;

            _userManager = userManager == null ? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>() : userManager;
        }


        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            AppMember user = new AppMember
            {
                UserName = userModel.UserName
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);

            return result;
        }

        public async Task<AppMember> FindUser(string userName, string password)
        {
            AppMember user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public async Task<AppMember> FindUserByUsername(string userName)
        {
            AppMember user = await _userManager.FindByNameAsync(userName);

            return user;
        }

        public Client FindClient(string clientId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT [Id]
                                          ,[Secret]
                                          ,[Name]
                                          ,[ApplicationType]
                                          ,[Active]
                                          ,[RefreshTokenLifeTime]
                                          ,[AllowedOrigin]
                                      FROM [dbo].[Client] WHERE [Id] = @ClientId";

                return conn.Query<Client>(commandText, new { ClientId = clientId }).ToList().SingleOrDefault();
            }
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            var existingToken = FindRefreshTokenByFilter("",token.Subject,token.ClientId);

            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken);
            }

            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                return await conn.ExecuteAsync(@"Insert into [RefreshTokens] ([Id],[Subject],[ClientId],[IssuedUtc],[ExpiresUtc],[ProtectedTicket],[MemberId]) 
                                                                      values (@Id, @Subject, @ClientId, @IssuedUtc, @ExpiresUtc, @ProtectedTicket, @MemberId)",
                        new
                        {
                            Id = token.Id,
                            Subject = token.Subject,
                            ClientId = token.ClientId,
                            IssuedUtc = token.IssuedUtc,
                            ExpiresUtc = token.ExpiresUtc,
                            ProtectedTicket = token.ProtectedTicket,
                            MemberId = token.MemberId
                        }) > 0;
            }
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await FindRefreshToken(refreshTokenId);

            if (refreshToken != null)
            {
                return await RemoveRefreshToken(refreshToken);
            }

            return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                return await conn.ExecuteAsync(@"DELETE FROM [dbo].[RefreshTokens] WHERE Id = @RefreshTokensId", new { RefreshTokensId = refreshToken.Id }) > 0 ;
            }
        }

        public  Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {

            var tokens = FindRefreshTokenByFilter(refreshTokenId);

            if (tokens != null)
                return Task.FromResult<RefreshToken>(tokens);
            else
                return Task.FromResult<RefreshToken>(null);
        }

        private RefreshToken FindRefreshTokenByFilter(string refreshTokenId = "", string subject = "", string clientId = "")
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT [Id]
                                          ,[Subject]
                                          ,[ClientId]
                                          ,[IssuedUtc]
                                          ,[ExpiresUtc]
                                          ,[ProtectedTicket]
                                      FROM [dbo].[RefreshTokens] WHERE 1 = 1";

                IEnumerable<RefreshToken> tokens = null;

                if (!string.IsNullOrEmpty(refreshTokenId)) {
                    commandText = commandText + " AND Id = @RefreshTokensId";
                    tokens = conn.Query<RefreshToken>(commandText, new { RefreshTokensId = refreshTokenId }).ToList();
                }

                if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(clientId))
                {
                    commandText = commandText + " AND [Subject] = @Subject AND [Subject] = @ClientId";
                    tokens = conn.Query<RefreshToken>(commandText, new { Subject = subject, ClientId = clientId }).ToList();
                }

                return tokens.FirstOrDefault();
            }
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT [Id]
                                          ,[Subject]
                                          ,[ClientId]
                                          ,[IssuedUtc]
                                          ,[ExpiresUtc]
                                          ,[ProtectedTicket]
                                      FROM [dbo].[RefreshTokens]";

                return conn.Query<RefreshToken>(commandText, new { }).ToList();
            }
        }
    

        public async Task<AppMember> FindAsync(UserLoginInfo loginInfo)
        {
            AppMember user = await _userManager.FindAsync(loginInfo);

            return user;
        }

        public async Task<IdentityResult> CreateAsync(AppMember user)
        {
            var result = await _userManager.CreateAsync(user);

            return result;
        }

        public async Task<IdentityResult> AddLoginAsync(int userId, UserLoginInfo login)
        {
            var result = await _userManager.AddLoginAsync(userId, login);

            return result;
        }


    }
}
