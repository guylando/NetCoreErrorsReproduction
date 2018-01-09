var a = 0;
setupAndStartSignalRUpdating("test", "upd", function () {
    $("#aaa").text(new Date());
    a++;
    if (a % 5 === 0) {
        $.post("/Home/ContactPost", function (data) {
        });
    }
}, "t", 1);
function setupAndStartSignalRUpdating(hubName, updateFuncName, updateFun, keysParam, keysParamValue, startCallback) {
    /* Initialize signalr */
    var newSignalRConnection = $.hubConnection("/ff", { useDefaultPath: false });
    newSignalRConnection.logging = true;
    newSignalRConnection.dontReconnectSignalR = false;
    var theHub = newSignalRConnection.createHubProxy(hubName);
    theHub.on(updateFuncName, updateFun);
    theHub.on("stopClient", function () {
        newSignalRConnection.dontReconnectSignalR = true;
        newSignalRConnection.stop();
    });
    newSignalRConnection.signalrHub = theHub;

    /* Add keys param to query string */
    newSignalRConnection.qs = {};
    if (typeof keysParam === 'object') {
        $.extend(newSignalRConnection.qs, keysParam);
    }
    else {
        newSignalRConnection.qs[keysParam] = keysParamValue;
    }

    /* Start signalr information updating */
    if (startCallback) {
        newSignalRConnection.start().done(startCallback);
    }
    else {
        newSignalRConnection.start();
    }

    /* Return the newly created connection */
    return newSignalRConnection;
}