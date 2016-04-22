using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAspNetIdentity.Models
{
    public class IdentityUserRole
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string Username { get; set; }

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUserId { get; set; }
        public string CreatedUserFullName { get; set; }

    }
}
