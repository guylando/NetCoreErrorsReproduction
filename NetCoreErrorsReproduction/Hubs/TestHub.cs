using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreErrorsReproduction.Hubs
{
    [HubName("test")]
    public class TestHub : BaseHub
    {
        #region Static Fields

        // Concurrent Dictionary to make it thread safe for use
        // Lazy initialization to make it thread safe: https://msdn.microsoft.com/en-us/library/dd997286.aspx
        private static Lazy<ConcurrentDictionary<string, Tuple<Task, CancellationTokenSource>>> hubUpdatingTasksInfo =
            new Lazy<ConcurrentDictionary<string, Tuple<Task, CancellationTokenSource>>>();

        #endregion

        #region Instance Fields

        protected override ILogger mLogger { get; set; }
        protected override Func<string, string, IHubCallerConnectionContext<dynamic>, CancellationToken, string, IPAddress, Action> mHubActionGenerator { get; set; }

        #endregion

        #region Constructor

        public TestHub(IOptions<IdentityOptions> identityOptions, ILoggerFactory loggerFactory)
            : base(hubUpdatingTasksInfo, "t", identityOptions, loggerFactory)
        {
            this.mLogger = loggerFactory.CreateLogger<TestHub>();
            this.mHubActionGenerator = new Func<string, string, IHubCallerConnectionContext<dynamic>, CancellationToken, string, IPAddress, Action>((currUserId, assetId, clients, cancellationToken, creationRequestQuery, clientIP) => {
                return (Action)(async () =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            clients.Caller.upd();
                            loggerFactory.CreateLogger<TestHub>().LogError("signalr hub error");
                            await Task.Delay(TimeSpan.FromMilliseconds(2000));
                        }
                    }
                    // Just abort hub in case of this exception
                    catch (Exception e) when ((e is OperationCanceledException) || (e is ThreadAbortException) || (e is AggregateException))
                    {

                    }
                    catch (Exception e)
                    {
                        loggerFactory.CreateLogger<TestHub>().LogError("SignalR AssetsTickerHub client IP: " + clientIP.ToString() + "\r\n" + e.ToString());
                    }

                    try
                    {
                        // Stop the client connection when task is done
                        // Don't stop client if cancellation token was called which could happen on reconnect and stopping client will terminate the connection in this case
                        // If cancellation called on disconnect then no need to stop client anyway, let him try to continue reconnecting
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            clients.Caller.stopClient();
                        }
                    }
                    catch (Exception e) when ((e is OperationCanceledException) || (e is ThreadAbortException) || (e is AggregateException))
                    {

                    }
                    catch (Exception e)
                    {
                        loggerFactory.CreateLogger<TestHub>().LogError("SignalR AssetsTickerHub client IP: " + clientIP.ToString() + "\r\n" + e.ToString());
                    }
                });
            });
        }

        #endregion
    }
}
