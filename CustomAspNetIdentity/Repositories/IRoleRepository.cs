using CustomAspNetIdentity.Models;
using Dapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAspNetIdentity.Repositories
{
    public interface IRoleRepository
    {
        int Delete(string roleId);
        IEnumerable<IdentityRole> GetRoles();
        int Insert(IdentityRole role);
        string GetRoleName(string roleId);
        string GetRoleId(string roleName);
        int Update(IdentityRole role);
        IdentityRole GetRoleById(string roleId);
        IdentityRole GetRoleByName(string roleName);
        List<IdentityUserRole> FindRolesByUserId(string userId);
        List<IdentityUserRole> FindUsersByRoleId(string roleId);
        int DeleteUserRole(string userId, string roleId = null);
        int AddUserRole(string userId, string roleId, string CreatedBy = "");

    }

    public class RoleRepository : IRoleRepository
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IConnectionStringProvider _connectionStringProvider;

        public RoleRepository(ICacheProvider cacheProvider, IConnectionStringProvider connectionStringProvider)
        {
            _cacheProvider = cacheProvider;
            _connectionStringProvider = connectionStringProvider;
        }

        public int Delete(string roleId)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string Sql = "DELETE FROM [dbo].[Role] WHERE Id = @id";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql;
                    cmd.Parameters.AddWithValue("@id", roleId);

                    var modified = cmd.ExecuteScalar();

                    _cacheProvider.Remove("IdentityRoles");
                    GetRoles();

                    return 0;
                }
            }
        }

        public IEnumerable<IdentityRole> GetRoles()
        {
            return _cacheProvider.GetOrAdd("IdentityRoles", () =>
            {
                var queryText = String.Format(@"SELECT [Id]
                                  ,[Name]
                                  ,[Description]
                                  ,[CreatedDate]
                                  ,[LastUpdatedDate]
                              FROM [dbo].[Role] as IdentityRole
                                ORDER BY Name DESC
                    ");
                using (var con = new SqlConnection(_connectionStringProvider.DefaultConnection))
                {
                    var dbResult = con.Query<IdentityRole>(queryText, new { }).ToList();
                    return dbResult;
                }

            }, 30);
        }

        public int Insert(IdentityRole role)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {

                    string Sql = "INSERT INTO dbo.[Role] " +
                                    " ([Id], [Name], [Description], [CreatedDate], [LastUpdatedDate], [Inactive]) VALUES " +
                                    " (@id,@Name,@Description,@CreatedDate, @LastUpdatedDate, @Inactive)" +
                                    " SELECT SCOPE_IDENTITY()";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql;
                    cmd.Parameters.AddWithValue("@id", role.Id);
                    cmd.Parameters.AddWithValue("@Name", role.Name);
                    cmd.Parameters.AddWithValue("@Description", role.Description);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@LastUpdatedDate", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@Inactive", role.Inactive);

                    var modified = cmd.ExecuteScalar();

                    _cacheProvider.Remove("IdentityRoles");
                    GetRoles();

                    return int.Parse(modified.ToString());
                }
            }
        }

        public string GetRoleName(string roleId)
        {
            var role = GetRoles().FirstOrDefault(r => r.Name.ToLower() == roleId);
            return role == null ? "" : role.Name;
        }

        public string GetRoleId(string roleName)
        {
            var role = GetRoles().FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
            return role == null ? "" : role.Id;
        }

        public IdentityRole GetRoleById(string roleId)
        {
            return GetRoles().FirstOrDefault(r => r.Id == roleId);
        }

        public IdentityRole GetRoleByName(string roleName)
        {
            return GetRoles().FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
        }

        public int Update(IdentityRole role)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {

                    string Sql = @"UPDATE dbo.[Role] SET
                                        [Name] = @Name, 
                                        [Description] = @Description,
                                        [LastUpdatedDate] = @LastUpdatedDate
                                      WHERE Id=@Id";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql;
                    cmd.Parameters.AddWithValue("@id", role.Id);
                    cmd.Parameters.AddWithValue("@Name", role.Name);
                    cmd.Parameters.AddWithValue("@Description", role.Description);
                    cmd.Parameters.AddWithValue("@LastUpdatedDate", DateTime.UtcNow);

                    var modified = cmd.ExecuteScalar();

                    _cacheProvider.Remove("IdentityRoles");
                    GetRoles();

                    return int.Parse(modified.ToString());
                }
            }
        }

        private List<IdentityUserRole> FindRolesByFilter(string userId = null, string roleId = null)
        {
            using (var con = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                string queryText = @"SELECT	u.Id as UserId,
		                                u.[UserName] as UserName,
		                                u.[Firstname] + ' ' + u.[Lastname] as UserFullName,
                                        u.[Email] as UserEmail,
                                        u.[Username] as Username,
		                                r.[Name] as RoleName, 
                                        r.[Id] as RoleId, 
		                                ur.CreatedDate,
		                                cu.[Id] as CreatedUserId,
		                                cu.[Firstname] + ' ' + cu.[Lastname] as CreatedUserFullName
                                        FROM UserRoles ur
                                            INNER JOIN dbo.[Role] r ON r.[Id] =  ur.[RoleId]
                                            INNER JOIN dbo.[Users] u ON u.[Id] =  ur.[UserId]
                                            INNER JOIN dbo.[Users] cu ON cu.[Id] =  ur.[CreatedBy]
                                        WHERE 1 = 1";

                if (!string.IsNullOrEmpty(userId))
                {
                    queryText = queryText + " AND u.[Id] = @Id";
                    return con.Query<IdentityUserRole>(queryText, new { Id = userId }).ToList();
                }

                if (!string.IsNullOrEmpty(roleId))
                {
                    queryText = queryText + " AND r.RoleId = @roleId";
                    return con.Query<IdentityUserRole>(queryText, new { roleId = roleId }).ToList();
                }

                return null;
            }
        }

        public List<IdentityUserRole> FindRolesByUserId(string userId)
        {
            return FindRolesByFilter(userId, null);
        }

        public List<IdentityUserRole> FindUsersByRoleId(string roleId)
        {
            return FindRolesByFilter(null, roleId);
        }

        public int DeleteUserRole(string userId, string roleId = null)
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    string Sql = @"DELETE FROM [dbo].[UserRoles] 
                                    WHERE UserId = @userId";
                    cmd.Parameters.AddWithValue("@id", userId);

                    if (!string.IsNullOrEmpty(roleId))
                    {
                        Sql = Sql + " RoleId = @roleId";
                        cmd.Parameters.AddWithValue("@roleId", roleId);
                    }

                    cmd.CommandText = Sql;

                    var modified = cmd.ExecuteScalar();
                    return 0;
                }
            }
        }

        public int AddUserRole(string userId, string roleId, string CreatedBy = "")
        {
            using (var conn = new SqlConnection(_connectionStringProvider.DefaultConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {

                    string Sql = "INSERT INTO dbo.[UserRoles] " +
                                    " ([UserId], [RoleId], [CreatedDate],[CreatedBy]) VALUES " +
                                    " (@userId, @roleId, @CreatedDate, @CreatedBy)" +
                                    " SELECT SCOPE_IDENTITY()";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@roleId", roleId);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);

                    var modified = cmd.ExecuteScalar();
                    return int.Parse(modified.ToString());
                }
            }
        }

    }
}
