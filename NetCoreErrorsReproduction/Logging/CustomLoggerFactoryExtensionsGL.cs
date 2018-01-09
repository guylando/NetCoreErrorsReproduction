using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetCoreReproduction.Logging;
using System;

/// <summary>
/// Put it in the Microsoft.Extensions.Logging namespace so that it will be visible when using Microsoft.Extensions.Logging
/// </summary>
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Based on asp.net debug logger extensions:
    /// https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Debug/DebugLoggerFactoryExtensions.cs
    /// </summary>
    public static class CustomLoggerFactoryExtensionsGL
    {
        public static ILoggerFactory AddPostgresql(this ILoggerFactory factory, IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            return AddPostgresql(factory, LogLevel.Information, serviceProvider, contextAccessor);
        }
        public static ILoggerFactory AddPostgresql(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            factory.AddProvider(new CustomLoggerProvider(filter, serviceProvider, contextAccessor));
            return factory;
        }
        public static ILoggerFactory AddPostgresql(this ILoggerFactory factory, LogLevel minLevel, IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            // The underscore is a convention used to name a variable which is required but not used
            return AddPostgresql(factory, (_, logLevel) => logLevel >= minLevel, serviceProvider, contextAccessor);
        }
    }
}
