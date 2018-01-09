using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction.Middleware
{
    public class HideExceptionsMiddleware
    {
        #region Fields

        private readonly RequestDelegate mNext;
        private readonly ILogger mLogger;
        private readonly IAntiforgery mAntiforgery;

        #endregion

        #region Constructor

        public HideExceptionsMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IAntiforgery antiforgery)
        {
            this.mNext = next;
            this.mLogger = loggerFactory.CreateLogger<HideExceptionsMiddleware>();
            this.mAntiforgery = antiforgery;
        }

        #endregion Constructor

        public async Task Invoke(HttpContext context)
        {
            await this.mNext.Invoke(context);

            if (context.Response.StatusCode == 400 &&
                context.Request.Headers.ContainsKey("X-Requested-With") &&
                string.Equals(context.Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest") &&
                !context.Response.HasStarted &&
                !context.Response.ContentLength.HasValue)
            {
                this.mLogger.LogError("ERROR: Empty body 400 error, handling it now in HideExceptionsMiddleware.");
                string traceEnv = Environment.GetEnvironmentVariable("EXOPTION_TRACE");
                if (!await this.mAntiforgery.IsRequestValidAsync(context))
                {
                    // Update logger context to be current context because otherwise it has the wrong context
                    this.mLogger.LogError("ERROR: HideExceptionsMiddleware user: " + (context.User.Identity.Name ?? "anonymous"));
                }
                else
                {
                    this.mLogger.LogError("ERROR: Server is returning an 400 response with empty body while the csrf token was valid, this is an unexpected situation." + " " + Environment.StackTrace);
                }
                await context.Response.WriteAsync("An error occurred. Please try again or report the error to the website administrator.");
            }
        }
    }
}
