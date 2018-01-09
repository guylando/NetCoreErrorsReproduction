using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
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
    public abstract class BaseHub : Hub
    {
        #region Fields

        #region Instance Fields

        private readonly Lazy<ConcurrentDictionary<string, Tuple<Task, CancellationTokenSource>>> mHubTasks;
        private readonly string mQueryKeyParameterName;
        private readonly IdentityOptions mIdentityOptions;
        protected abstract Func<string, string, IHubCallerConnectionContext<dynamic>, CancellationToken, string, IPAddress, Action> mHubActionGenerator { get; set; }
        protected abstract ILogger mLogger { get; set; }

        #endregion

        #endregion

        #region Constructor

        public BaseHub(Lazy<ConcurrentDictionary<string, Tuple<Task, CancellationTokenSource>>> hubTasks,
            string queryKeyParameterName, IOptions<IdentityOptions> identityOptions, ILoggerFactory loggerFactory)
        {
            this.mHubTasks = hubTasks;
            this.mQueryKeyParameterName = queryKeyParameterName;
            this.mIdentityOptions = identityOptions.Value;
        }

        #endregion

        #region Private Methods

        #region AddTask

        private void AddTask(string keyVal)
        {
            try
            {
                // Delete previous task
                RemoveTask();

                // Create new task
                // NOTE THAT PASSING CONTEXT HERE TO THE TASK FROM SOME REASON GIVES EMPTY VALUES, PROOBABLY AFTER ITS DISPOSED SO PASS
                // ONLY DESIRED LITERAL VALUES SO THAT THEY WILL NOT GET DISPOSED (QueryString for example)
                // We don't check here that query string is not empty because it is checked by the caller of this method
                CancellationTokenSource taskCancellationTokenSource = new CancellationTokenSource();
                Task newTask = Task.Run(this.mHubActionGenerator(Context.GetUserId(this.mIdentityOptions), keyVal, Clients,
                    taskCancellationTokenSource.Token, Context.Request.QueryString.Value, Context.Request.HttpContext.Connection.RemoteIpAddress), taskCancellationTokenSource.Token);

                // Add new task to dictionary
                this.mHubTasks.Value.TryAdd(Context.ConnectionId,
                    new Tuple<Task, CancellationTokenSource>(newTask, taskCancellationTokenSource));
            }
            // If exception happened then adding the new task will just fail and be logged, user will get no notifications
            // TODO: (development) Consider maybe giving the user some indication that hub new task creation failed
            catch (Exception e)
            {
                mLogger.LogError(e.ToString());
            }
        }

        #endregion

        #region RemoveTask

        private void RemoveTask()
        {
            try
            {
                Tuple<Task, CancellationTokenSource> currentTaskInfo;
                if (this.mHubTasks.Value.TryGetValue(Context.ConnectionId, out currentTaskInfo))
                {
                    try
                    {
                        currentTaskInfo.Item2.Cancel();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        this.mHubTasks.Value.TryRemove(Context.ConnectionId, out currentTaskInfo);

                    }
                }
            }
            // If exception happened then removing the task will just fail and be logged, user will get no notifications
            // TODO: (development) Consider maybe giving the user some indication that hub task removing failed, however removing happens on disconnect so maybe no need to notify user on failure
            catch (Exception e)
            {
                mLogger.LogError(e.ToString());
            }
        }

        #endregion

        #endregion

        #region Hub Events

        #region OnConnected

        public override Task OnConnected()
        {
            try
            {
                if (Context.QueryString[this.mQueryKeyParameterName].Count > 0)
                {
                    AddTask(Context.QueryString[this.mQueryKeyParameterName].ToString());
                }
                else
                {
                    this.mLogger.LogError("SECURITY: Hub received no query key parameter or bad value for the parameter. Name: " + this.mQueryKeyParameterName + " Value: null" + " " + Environment.StackTrace);
                }

                return base.OnConnected();
            }
            catch (Exception e)
            {
                mLogger.LogError(e.ToString());
            }

            return Task.WhenAll();
        }

        #endregion

        #region OnReconnected

        public override Task OnReconnected()
        {
            try
            {
                if (Context.QueryString[this.mQueryKeyParameterName].Count > 0)
                {
                    AddTask(Context.QueryString[this.mQueryKeyParameterName].ToString());
                }
                else
                {
                    this.mLogger.LogError("SECURITY: Hub received no query key parameter or bad value for the parameter. Name: " + this.mQueryKeyParameterName + " Value: null" + " " + Environment.StackTrace);
                }

                return base.OnReconnected();
            }
            catch (Exception e)
            {
                mLogger.LogError(e.ToString());
            }

            return Task.WhenAll();
        }

        #endregion

        #region OnDisconnected

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                RemoveTask();

                return base.OnDisconnected(stopCalled);
            }
            catch (Exception e)
            {
                mLogger.LogError(e.ToString());
            }

            return Task.WhenAll();
        }

        #endregion

        #endregion
    }
}
