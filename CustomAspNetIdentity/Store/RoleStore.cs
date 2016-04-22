using CustomAspNetIdentity.Models;
using CustomAspNetIdentity.Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDoc.Repositories.Store
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>
         where TRole : IdentityRole
    {
        private RoleRepository _roleRepository;
        private ICacheProvider _cacheProvider;
        private IConnectionStringProvider _connectionStringProvider;


        public IQueryable<TRole> Roles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public RoleStore()
        {
            _cacheProvider = new CacheProvider();
            _connectionStringProvider = new ConnectionStringProvider();

            _roleRepository = new RoleRepository(_cacheProvider, _connectionStringProvider);
        }

        public Task CreateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            _roleRepository.Insert(role);

            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            _roleRepository.Delete(role.Id);

            return Task.FromResult<Object>(null);
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            TRole result = _roleRepository.GetRoleById(roleId) as TRole;

            return Task.FromResult<TRole>(result);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            TRole result = _roleRepository.GetRoleByName(roleName) as TRole;

            return Task.FromResult<TRole>(result);
        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            _roleRepository.Update(role);

            return Task.FromResult<Object>(null);
        }

        public void Dispose()
        {
            return;
        }

    }
}
