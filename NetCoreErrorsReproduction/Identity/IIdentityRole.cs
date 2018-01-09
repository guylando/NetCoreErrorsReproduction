using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction.Identity
{
    public interface IIdentityRole
    {
        int Id { get; set; }
        string Name { get; set; }
        string NormalizedName { get; set; }
    }
}
