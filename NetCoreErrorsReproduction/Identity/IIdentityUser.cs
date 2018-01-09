using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction.Identity
{
    public interface IIdentityUser
    {
        string Id { get; set; }
        string UserName { get; set; }
        string PhonePrefix { get; set; }
        string Phone { get; set; }
        string SecurityStamp { get; set; }
        string Password { get; set; }
        string Email { get; set; }
        string NormalizedEmail { get; set; }
        DateTime LockoutEndTime { get; set; }
        DateTime LastLoginTime { get; set; }
        short AccessFailedCount { get; set; }
        short AccountStatus { get; set; }
    }
}
