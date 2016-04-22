using Microsoft.AspNet.Identity;
using MyDoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using MyDoc.Repositories.Providers;


namespace MyDoc.Repositories.Store
{

    /// <summary>
    /// Class that implements the key ASP.NET Identity user store iterfaces
    /// </summary>
    public class UserStore<TUser> : IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserTwoFactorStore<TUser, string>,
        IUserLockoutStore<TUser, string>,
        IUserStore<TUser>
        where TUser : IdentityUser
    {

        private IRoleRepository _roleRepository;
        private IUserRepository<TUser> _userRepository;
        private ICacheProvider _cacheProvider;
        private IConnectionStringProvider _connectionStringProvider;


        public IQueryable<TUser> Users
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public UserStore(IUserRepository<TUser> userRepository, ICacheProvider cacheProvider, 
           IConnectionStringProvider connectionStringProvider, IRoleRepository roleRepository)
        {
            _cacheProvider = cacheProvider;
            _connectionStringProvider = connectionStringProvider;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public Task CreateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Insert(user);

            return Task.FromResult<object>(null);
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Null or empty argument: userId");

            TUser result = _userRepository.GetUserById(userId).FirstOrDefault();
            if (result != null)
                return Task.FromResult<TUser>(result);

            return Task.FromResult<TUser>(null);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Null or empty argument: userName");

            TUser result = _userRepository.GetUserByName(userName).FirstOrDefault();

            // Should I throw if > 1 user?
            if (result != null)
                return Task.FromResult<TUser>(result);

            return Task.FromResult<TUser>(null);
        }

        public Task UpdateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Update(user);

            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            return;
        }

        public Task AddClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (claim == null)
                throw new ArgumentNullException("user");

            _userRepository.AddClaim(claim, user.Id);

            return Task.FromResult<object>(null);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ClaimsIdentity identity = _userRepository.FindClaimsByUserId(user.Id);

            return Task.FromResult<IList<Claim>>(identity.Claims.ToList());
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (claim == null)
                throw new ArgumentNullException("claim");

            _userRepository.DeleteClaim(user, claim);

            return Task.FromResult<object>(null);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            _userRepository.AddLogin(user, login);

            return Task.FromResult<object>(null);
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            var userId = _userRepository.FindLoginUserIdByLogin(login);
            if (userId != null)
            {
                TUser user = _userRepository.GetUserById(userId).FirstOrDefault();
                if (user != null)
                {
                    return Task.FromResult<TUser>(user);
                }
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            List<UserLoginInfo> userLogins = new List<UserLoginInfo>();
            if (user == null)
                throw new ArgumentNullException("user");


            List<UserLoginInfo> logins = _userRepository.FindLoginsByUserId(user.Id);
            if (logins != null)
                return Task.FromResult<IList<UserLoginInfo>>(logins);


            return Task.FromResult<IList<UserLoginInfo>>(null);
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            _userRepository.DeleteLogin(user, login);

            return Task.FromResult<Object>(null);
        }


        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentException("Argument cannot be null or empty: roleName.");

            string roleId = _roleRepository.GetRoleId(roleName);
            if (!string.IsNullOrEmpty(roleId))
            {
                _roleRepository.AddUserRole(user.Id, roleId, "");
            }

            return Task.FromResult<object>(null);
        }

        public Task SessionAsync(string userId, string IPAdress)
        {

            _userRepository.AddSession(userId, IPAdress);

            return Task.FromResult<object>(null);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var roleObjects = _roleRepository.FindRolesByUserId(user.Id);
            List<string> stringRoles = new List<string>();
            foreach (var role in roleObjects)
                stringRoles.Add(role.RoleName);

            List<string> roles = stringRoles;
            {
                if (roles != null)
                {
                    return Task.FromResult<IList<string>>(roles);
                }
            }

            return Task.FromResult<IList<string>>(null);
        }

        /// <summary>
        /// Verifies if a user is in a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task<bool> IsInRoleAsync(TUser user, string role)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException("role");

            bool Exists = _roleRepository.FindRolesByUserId(user.Id).Any(x => x.RoleName.ToLower() == role.ToLower()) ;

            if (Exists)
                return Task.FromResult<bool>(true);

            return Task.FromResult<bool>(false);
        }


        public Task RemoveFromRoleAsync(TUser user, string role)
        { 
            if (user == null)
                throw new ArgumentNullException("user");

            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException("role");

            var r = _roleRepository.GetRoleByName(role);
            if (r == null)
                throw new ArgumentNullException("role");

            _roleRepository.DeleteUserRole(user.Id, r.Id);

            return Task.FromResult<Object>(null);
        }

        public Task DeleteAsync(TUser user)
        {
            if (user != null)
            {
                _userRepository.Delete(user);
            }

            return Task.FromResult<Object>(null);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            string passwordHash = _userRepository.GetPasswordHash(user.Id);

            return Task.FromResult<string>(passwordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            var hasPassword = !string.IsNullOrEmpty(_userRepository.GetPasswordHash(user.Id));

            return Task.FromResult<bool>(Boolean.Parse(hasPassword.ToString()));
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            return Task.FromResult<Object>(null);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            user.SecurityStamp = stamp;

            return Task.FromResult(0);

        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            user.Email = email;
            _userRepository.Update(user);

            return Task.FromResult(0);

        }

        public Task<string> GetEmailAsync(TUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }

        public Task<TUser> FindByEmailAsync(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            TUser result = _userRepository.GetUserByEmail(email).FirstOrDefault() as TUser;
            if (result != null)
            {
                return Task.FromResult<TUser>(result);
            }

            return Task.FromResult<TUser>(null);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }


        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }


        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }



        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }


        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount++;
            _userRepository.Update(user);

            return Task.FromResult(user.AccessFailedCount);
        }


        public Task ResetAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount = 0;
            _userRepository.Update(user);

            return Task.FromResult(0);
        }


        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }


        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }


        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            _userRepository.InsertOrUpdate(user);

            return Task.FromResult(0);
        }






    }

}
