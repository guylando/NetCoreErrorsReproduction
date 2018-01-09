using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;
using NetCoreErrorsReproduction;
using System.Diagnostics;

namespace NetCoreReproduction.Logging
{
    public class CustomLogger : ILogger
    {
        private readonly string mName;
        private readonly Func<string, LogLevel, bool> mFilter;
        private readonly IServiceProvider mServiceProvider;
        private readonly IHttpContextAccessor mContextAccessor;
        private readonly IdentityOptions mIdentityOptions;
        private static object mFileLoggingLock = new Object();
        public CustomLogger(string name, Func<string, LogLevel, bool> filter, IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            this.mName = string.IsNullOrEmpty(name) ? nameof(CustomLogger) : name;
            this.mFilter = filter;
            this.mServiceProvider = serviceProvider;
            this.mContextAccessor = contextAccessor;
            if (serviceProvider != null)
            {
                IOptions<IdentityOptions> identityOptions = serviceProvider.GetService<IOptions<IdentityOptions>>();
                if (identityOptions != null)
                {
                    this.mIdentityOptions = identityOptions.Value;
                }
            }
        }
        public IDisposable BeginScopeImpl(object state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // If the filter is null, everything is enabled
            return (this.mFilter == null || this.mFilter(this.mName, logLevel));
        }

        public bool GetCurrentUserInfo(out IPAddress userip, out int userid, out string username)
        {
            userip = null;
            userid = ConstantsGL.UninitializedNumericMLProperty;
            username = ConstantsGL.UninitializedStringMLProperty;
            if (this.mContextAccessor == null)
            {
                return false;
            }
            else
            { 
                if (this.mContextAccessor.HttpContext != null)
                {
                    try
                    {
                        // This might throw exception when connection disposed so put it in a try catch block
                        userip = this.mContextAccessor.HttpContext.Connection.RemoteIpAddress;
                    }
                    catch
                    {
                    }
                    string userId = this.mContextAccessor.HttpContext.User.GetUserId(this.mIdentityOptions);
                    if (userId != null)
                    {
                        userid = int.Parse(userId);
                    }
                    username = this.mContextAccessor.HttpContext.User.GetUserName(this.mIdentityOptions);
                    if (username == null)
                    {
                        username = ConstantsGL.UninitializedStringMLProperty;
                    }
                }

                return true;
            }
        }

        public void ConsolePrintCurrentUserInfoIfLoggedOn()
        {
            try
            {
                bool success = GetCurrentUserInfo(out IPAddress userip, out int currUserId, out string username);
                if (success)
                {
                    if (userip != null)
                    {
                        Console.WriteLine(ConstantsGL.LogsValuesPrefix + "User IP: " + userip);
                    }
                    if (currUserId != ConstantsGL.UninitializedNumericMLProperty)
                    {
                        Console.WriteLine(ConstantsGL.LogsValuesPrefix + "Current User Id: " + currUserId);
                    }
                    if (!string.Equals(username, ConstantsGL.UninitializedStringMLProperty))
                    {
                        Console.WriteLine(ConstantsGL.LogsValuesPrefix + "Current User Name: " + username);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ConstantsGL.LogsValuesPrefix + "Failed console printing current user info because of: " + e.ToString());
            }
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = "Error in postgresql logger";

            try
            {
                if (this.IsEnabled(logLevel))
                {
                    if (formatter != null)
                    {
                        message = formatter(state, exception);
                    }
                    else
                    {
                        if (state != null)
                        {
                            message += state;
                        }
                        if (exception != null)
                        {
                            message += Environment.NewLine + exception;
                        }
                    }

                    // Log only messages from our code and don't log messages from the different libraries and framework
                    // Our log messages contain the log values prefix
                    if (!message.Contains(ConstantsGL.LogsValuesPrefix))
                    {
                        return;
                    }

                    string userName = this.mContextAccessor.HttpContext.User.GetUserName(this.mIdentityOptions);
                    if (!message.Contains("Client user name: ") && userName != null)
                    {
                        message = ConstantsGL.LogsValuesPrefix + "Client user name: " + userName + "\r\n" + message;
                    }

                    // Log to console and debug
                    Debug.WriteLine(message);
                    Console.WriteLine(message);
                }
            }
            catch
            {
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
