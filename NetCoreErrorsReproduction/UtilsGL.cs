using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction
{
    public static class UtilsGL
    {
        #region GetUserName

        public static string GetUserName(this ClaimsPrincipal user, IdentityOptions options)
        {
            if (user == null)
            {
                return null;
            }
            else
            {
                return user.FindFirstValue(options.ClaimsIdentity.UserNameClaimType);
            }
        }

        #endregion

        #region GetUserId

        public static string GetUserId(this ClaimsPrincipal user, IdentityOptions options)
        {
            if (user == null)
            {
                return null;
            }
            else
            {
                return user.FindFirstValue(options.ClaimsIdentity.UserIdClaimType);
            }
        }
        public static string GetUserId(this HubCallerContext context, IdentityOptions options)
        {
            if (context.User == null)
            {
                return null;
            }
            else
            {
                return ((ClaimsPrincipal)context.User).FindFirstValue(options.ClaimsIdentity.UserIdClaimType);
            }
        }

        #endregion
    }
}
