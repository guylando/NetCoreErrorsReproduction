using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace NetCoreReproduction.Logging
{
    /// <summary>
    /// Based on asp.net debug logger provider:
    /// https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Debug/DebugLoggerProvider.cs
    /// </summary>
    public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> mFilter;
        private readonly IServiceProvider mServiceProvider;
        private readonly IHttpContextAccessor mContextAccessor;
        public CustomLoggerProvider(Func<string, LogLevel, bool> filter, IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            this.mFilter = filter;
            this.mServiceProvider = serviceProvider;
            this.mContextAccessor = contextAccessor;
        }
        public ILogger CreateLogger(string name)
        {
            return new CustomLogger(name, this.mFilter, this.mServiceProvider, this.mContextAccessor);
        }

        public void Dispose()
        {
        }
    }
}
