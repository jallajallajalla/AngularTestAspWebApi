using Dapper;
using Microsoft.AspNet.Identity;
using MyDoc.Models;
using MyDoc.Repositories.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyDoc.Repositories
{

    public interface IUserRepository<TUser> 
    {
        string GetUserName(string userId);
        string GetUserId(string userName);
        List<TUser> GetUserById(string userId);
        List<TUser> GetUserByName(string userName);
        List<TUser> GetUserByEmail(string email);
        string GetPasswordHash(string userId);
        int SetPasswordHash(string userId, string passwordHash);
        string GetSecurityStamp(string userId);
        int Insert(TUser user);
        int InsertOrUpdate(TUser user);
        int Delete(TUser user);
        int Update(TUser user);
        TUser GetCurrentUser();
        int DeleteLogin(TUser user, UserLoginInfo login = null);
        int DeleteLogin(string userId);
        int AddLogin(TUser user, UserLoginInfo login);
        List<UserLoginInfo> FindLoginsByUserId(string userId);
        string FindLoginUserIdByLogin(UserLoginInfo userLogin);
        ClaimsIdentity FindClaimsByUserId(string userId);
        int DeleteClaim(string userId);
        int DeleteClaim(TUser user, Claim claim);
        int AddClaim(Claim userClaim, string userId);
        int AddSession(string userId, string IPAddress);
    }


    public class UserRepository<TUser>: IUserRepository<TUser>
        where TUser: IdentityUser
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IConnectionStringProvider _connectionStringProvider;

        public UserRepository(ICacheProvider cacheProvider, IConnectionStringProvider connectionStringProvider)
        {
            _cacheProvider = cacheProvider;
            _connectionStringProvider = connectionStringProvider;
        }

        public TUser GetCurrentUser()
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();

            if (userId == null)
                return null;

            return GetUserById(userId).Single();
        }

        public string GetUserName(string userId)
        {

            string commandText = "SELECT [Name] FROM dbo.[Users] WHERE [Id] = @id";
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                var ret = conn.Query<string>(commandText, new { Id = userId });
                return ret == null ? null : ret.ToString();
            }
        }

        public string GetUserId(string userName)
        {
            string commandText = "SELECT [Id] FROM dbo.[Users] WHERE [UserName] = @name";
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                var ret = conn.Query<string>(commandText, new { UserName = userName });
                return ret == null ? null : ret.ToString();
            }
        }

        public string GetUserSocialSecurityNumber(string SocialSecurityNumber)
        {
            string commandText = "SELECT [Id] FROM dbo.[Users] WHERE [SocialSecurityNumber] = @SocialSecurityNumber";
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                var ret = conn.Query<string>(commandText, new { SocialSecurityNumber = SocialSecurityNumber });
                return ret == null ? null : ret.ToString();
            }
        }

        public List<TUser> GetUserById(string userId)
        {
            return _cacheProvider.GetOrAdd("GetUserById_" + userId, () =>
            {
                return GetUser(userId, null);
            }, 30);
        }

        public List<TUser> GetUserByName(string userName)
        {
            var res = GetUser(null, userName);
            return res;
        }

        private List<TUser> GetUser(string userid, string userName, string email = null)
        {
            using (var con = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {

                var queryText = String.Format(@"SELECT [Id]            
                                            ,[FirstName]
                                            ,[LastName]
                                            ,[Email]
                                            ,[EmailConfirmed]
                                            ,[PasswordHash]
                                            ,[SecurityStamp]
                                            ,[PhoneNumber]
                                            ,[PhoneNumberConfirmed]
                                            ,[TwoFactorEnabled]
                                            ,[LockoutEndDateUtc]
                                            ,[LockoutEnabled]
                                            ,[AccessFailedCount]
                                            ,[UserName]
                                            ,[Culture]
                                            ,[ReminderIntervall]
                                            ,[City]
                                            ,[Address]
                                            ,[Zipcode]
                                            ,[SocialSecurityNumber]
                                        FROM [dbo].[Users]
                                        WHERE 1 = 1 ");

                    if (!string.IsNullOrEmpty(userid)) {
                        queryText = queryText + " AND [Id] = @id";
                        return con.Query<TUser>(queryText, new { Id = userid }).ToList();
                    }

                    if (!string.IsNullOrEmpty(userName)) {
                        queryText = queryText + " AND [UserName] = @UserName";
                        var res = con.Query<TUser>(queryText, new { UserName = userName }).ToList();
                        return res;
                    }

                    if (!string.IsNullOrEmpty(email))
                    {
                        queryText = queryText + " AND [Email] = @email";
                        return con.Query<TUser>(queryText, new { Email = email }).ToList();
                    }

                return null;
                }
         }

        public List<TUser> GetUserByEmail(string email)
        {
            return GetUser(null, null, email);
        }

        public string GetPasswordHash(string userId)
        {
            string commandText = "SELECT [PasswordHash] FROM dbo.[Users] WHERE [Id] = @id";
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                var ret = conn.Query<string>(commandText, new { Id = userId }).FirstOrDefault();
                return string.IsNullOrEmpty(ret) ? null : ret;
            }
        }

        public int SetPasswordHash(string userId, string passwordHash)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string Sql = @"UPDATE dbo.[Users] SET [PasswordHash] = @pwdHash WHERE Id = @id";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql;
                    cmd.Parameters.AddWithValue("@pwdHash", passwordHash);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();

                    return 0;
                }
            }
        }

        public string GetSecurityStamp(string userId)
        {
            string commandText = "SELECT [SecurityStamp] FROM dbo.[Users] WHERE Id = @id";
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                var ret = conn.Query<string>(commandText, new { Id = userId }).FirstOrDefault();
                return string.IsNullOrEmpty(ret) ? null : ret.ToString();
            }
        }

        public int Insert(TUser user)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string commandText = @"INSERT INTO dbo.[Users] 
                                                    ([UserName],[Id],[PasswordHash],[SecurityStamp],[Email],[EmailConfirmed],[PhoneNumber],[PhoneNumberConfirmed],[AccessFailedCount],[LockoutEnabled],[LockoutEndDateUtc],[TwoFactorEnabled],[Culture],[City],[ZipCode],[Address],[FirstName],[LastName],[SocialSecurityNumber])
                                             VALUES (@userName, @id, @PasswordHash, @SecurityStamp, @email, @emailconfirmed, @phonenumber, @phonenumberconfirmed, @AccessFailedCount, @lockoutenabled, @lockoutenddate,    @twofactorenabled, @Culture, @City, @ZipCode, @Address, @FirstName, @LastName, @SocialSecurityNumber)
                                            ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;

                    cmd.Parameters.AddWithValue("@userName", user.UserName);
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp);
                    cmd.Parameters.AddWithValue("@email", GetStringOrDBNull(user.Email));
                    cmd.Parameters.AddWithValue("@emailconfirmed", user.EmailConfirmed);
                    cmd.Parameters.AddWithValue("@phonenumber", GetStringOrDBNull(user.PhoneNumber));
                    cmd.Parameters.AddWithValue("@phonenumberconfirmed", user.PhoneNumberConfirmed);
                    cmd.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);
                    cmd.Parameters.AddWithValue("@lockoutenabled", user.LockoutEnabled);
                    cmd.Parameters.AddWithValue("@lockoutenddate", GetStringOrDBNull(user.LockoutEndDateUtc));
                    cmd.Parameters.AddWithValue("@twofactorenabled", user.TwoFactorEnabled);
                    cmd.Parameters.AddWithValue("@Culture", GetStringOrDBNull(user.Culture));
                    cmd.Parameters.AddWithValue("@City", GetStringOrDBNull(user.City));
                    cmd.Parameters.AddWithValue("@ZipCode", GetStringOrDBNull(user.ZipCode));
                    cmd.Parameters.AddWithValue("@Address", GetStringOrDBNull(user.Address));
                    cmd.Parameters.AddWithValue("@FirstName", GetStringOrDBNull(user.FirstName));
                    cmd.Parameters.AddWithValue("@LastName", GetStringOrDBNull(user.LastName));
                    cmd.Parameters.AddWithValue("@SocialSecurityNumber", user.SocialSecurityNumber);
                    var modified = cmd.ExecuteScalar();
                    return 1;
                }
            }
        }

        private  object GetStringOrDBNull(object obj)
        {
            return obj == null ? DBNull.Value : (object)obj;
        }

        public int InsertOrUpdate(TUser user)
        {
            if (GetUserById(user.Id).Count == 0)
                return Insert(user);
            else
                return Update(user);
        }

        private int Delete(string userId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string commandText = @"DELETE FROM dbo.[Users] WHERE Id = @userId";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;
                    cmd.Parameters.AddWithValue("@userId", userId);

                    _cacheProvider.Remove("GetUserById_" + userId);

                    var modified = cmd.ExecuteScalar();
                    return int.Parse(modified.ToString());
                }
            }
        }

        public int Delete(TUser user)
        {
            return Delete(user.Id);
        }

        public int Update(TUser user)
        {
            if (GetUserById(user.Id) == null)
                throw new Exception("Can not update unexisting user!");

            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string commandText = @"UPDATE dbo.[Users] SET
                                    [UserName] = @userName, 
                                    [Email] = @email, 
                                    [EmailConfirmed] = @emailconfirmed, 
                                    [PhoneNumber] = @phonenumber, 
                                    [PhoneNumberConfirmed] = @phonenumberconfirmed,
                                    [AccessFailedCount] = @AccessFailedCount, 
                                    [LockoutEnabled] = @lockoutenabled, 
                                    [LockoutEndDateUtc] = @lockoutenddate, 
                                    [TwoFactorEnabled] = @twofactorenabled,  
                                    [Culture] = @Culture,  
                                    [City] = @City,  
                                    [ZipCode] = @ZipCode,  
                                    [Address] = @Address,  
                                    [FirstName] = @FirstName,  
                                    [LastName] = @LastName,
                                    [SocialSecurityNumber] = @SocialSecurityNumber
                                 WHERE Id = @id";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;

                    cmd.Parameters.AddWithValue("@userName", user.UserName);
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@email", GetStringOrDBNull(user.Email));
                    cmd.Parameters.AddWithValue("@emailconfirmed", user.EmailConfirmed);
                    cmd.Parameters.AddWithValue("@phonenumber", GetStringOrDBNull(user.PhoneNumber));
                    cmd.Parameters.AddWithValue("@phonenumberconfirmed", user.PhoneNumberConfirmed);
                    cmd.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);
                    cmd.Parameters.AddWithValue("@lockoutenabled", user.LockoutEnabled);
                    cmd.Parameters.AddWithValue("@lockoutenddate", GetStringOrDBNull(user.LockoutEndDateUtc));
                    cmd.Parameters.AddWithValue("@twofactorenabled", user.TwoFactorEnabled);
                    cmd.Parameters.AddWithValue("@Culture", GetStringOrDBNull(user.Culture));
                    cmd.Parameters.AddWithValue("@City", GetStringOrDBNull(user.City));
                    cmd.Parameters.AddWithValue("@ZipCode", GetStringOrDBNull(user.ZipCode));
                    cmd.Parameters.AddWithValue("@Address", GetStringOrDBNull(user.Address));
                    cmd.Parameters.AddWithValue("@FirstName", GetStringOrDBNull(user.FirstName));
                    cmd.Parameters.AddWithValue("@LastName", GetStringOrDBNull(user.LastName));
                    cmd.Parameters.AddWithValue("@SocialSecurityNumber", user.SocialSecurityNumber);
                    var modified = cmd.ExecuteScalar();
                    _cacheProvider.Remove("GetUserById_" + user.Id);
                    return 1;
                }
            }
        }

        public int DeleteLogin(TUser user, UserLoginInfo login = null)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string commandText = @"DELETE FROM dbo.[UserProviderLogins] WHERE [UserId] = @userId";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@userId", user.Id);
                    if (login != null) {
                        commandText = commandText + " AND [LoginProvider] = @loginProvider and ProviderKey = @providerKey";
                        cmd.Parameters.AddWithValue("@loginProvider", login.LoginProvider);
                        cmd.Parameters.AddWithValue("@providerKey", login.ProviderKey);
                    }
                    cmd.CommandText = commandText;

                    var modified = cmd.ExecuteScalar();
                    return int.Parse(modified.ToString());
                }
            }
        }

        public int DeleteLogin(string userId)
        {
            var user = GetUserById(userId).FirstOrDefault();
            if (user != null)
                return DeleteLogin(user, null);
            else
                return -1;
        }

        public int AddLogin(TUser user, UserLoginInfo login)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var commandText = @"INSERT INTO dbo.[UserLogins] ([LoginProvider], [ProviderKey], [UserId]) VALUES (@loginProvider, @providerKey, @userId)";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;

                    cmd.Parameters.AddWithValue("loginProvider", login.LoginProvider);
                    cmd.Parameters.AddWithValue("providerKey", login.ProviderKey);
                    cmd.Parameters.AddWithValue("userId", user.Id);
                    cmd.ExecuteScalar();
                    return 1;
                }
            }
        }

        public List<UserLoginInfo> FindLoginsByUserId(string userId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT  [LoginProvider]
                                  ,[ProviderKey]
                                  ,[UserId]
                              FROM dbo.[UserLogins] WHERE [UserId] = @userId";

                return conn.Query<UserLoginInfo>(commandText, new { UserId = userId }).ToList();
            }
        }

        public string FindLoginUserIdByLogin(UserLoginInfo userLogin)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT [UserId]
                              FROM dbo.[UserLogins] WHERE [LoginProvider] = @loginProvider AND [ProviderKey] = @providerKey";

                var ret = conn.Query<string>(commandText, new {LoginProvider = userLogin.LoginProvider, ProviderKey = userLogin.ProviderKey}).FirstOrDefault();
                return string.IsNullOrEmpty(ret) ? null : ret.ToString();
            }
        }

        public ClaimsIdentity FindClaimsByUserId(string userId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string commandText = @"SELECT [Id]
                                      ,[ClaimType] as Type
                                      ,[ClaimValue] as Value
                                  FROM [dbo].[UserClaims] 
                                    WHERE [UserId] = @UserId";


                ClaimsIdentity claims = new ClaimsIdentity();
                foreach (var c in conn.Query(commandText, new { userId = userId }))
                    claims.AddClaim(new Claim(c.ClaimType, c.ClaimValue));

                return claims;
            }

        }

        public int DeleteClaim(string userId)
        {
            var user = GetUserById(userId).FirstOrDefault();
            if (user != null)
                return DeleteClaim(user, null);
            else
                return -1;
        }

        public int DeleteClaim(TUser user, Claim claim)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {

                    cmd.CommandType = CommandType.Text;
                    string commandText = @"DELETE FROM dbo.[UserClaims] WHERE [UserId] = @userId";
                    cmd.Parameters.AddWithValue("@userId", user.Id);

                    if (claim != null)
                    {
                        commandText = commandText + " AND [ClaimValue] = @ClaimValue AND [ClaimType] = @ClaimType";
                        cmd.Parameters.AddWithValue("@ClaimValue", claim.Value);
                        cmd.Parameters.AddWithValue("@ClaimType", claim.Type);
                    }
                    cmd.CommandText = commandText;

                    var modified = cmd.ExecuteScalar();
                    return int.Parse(modified.ToString());
                }
            }
        }

        public int AddClaim(Claim userClaim, string userId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var commandText = @"INSERT INTO dbo.[UserClaims] ([ClaimValue], [ClaimType], [UserId]) 
                                        VALUES (@ClaimValue, @ClaimType, @userId)";
                                        //SELECT SCOPE_IDENTITY()";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;

                    cmd.Parameters.AddWithValue("ClaimValue", userClaim.Value);
                    cmd.Parameters.AddWithValue("ClaimType", userClaim.Type);
                    cmd.Parameters.AddWithValue("userId", userId);

                    cmd.ExecuteScalar();
                    return 1;
                }
            }
        }

        public int AddSession(string userId, string IPAddress)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var commandText = @"INSERT INTO dbo.[UserSessions] ([UserId], [LoginTime], [IPAddress]) 
                                        VALUES (@UserId, @LoginTime, @IPAddress)";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = commandText;

                    cmd.Parameters.AddWithValue("UserId", userId);
                    cmd.Parameters.AddWithValue("IPAddress", IPAddress);
                    cmd.Parameters.AddWithValue("LoginTime", System.DateTime.Now);

                    cmd.ExecuteScalar();
                    return 1;
                }
            }
        }

        //public List<TUser> GetUserByEmail(string email)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
