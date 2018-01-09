using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction.Identity
{
    public class IdentityUser : IIdentityUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PhonePrefix { get; set; }
        public string Phone { get; set; }
        public string SecurityStamp { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public DateTime LockoutEndTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public short AccessFailedCount { get; set; }
        public short AccountStatus { get; set; }
    }
}
