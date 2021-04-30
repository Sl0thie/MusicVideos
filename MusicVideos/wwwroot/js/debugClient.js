//#region Variables
'use strict';
let debugClient;
//#endregion
//#region Initialisation
window.addEventListener('load', function () {
    debugClient = new signalR.HubConnectionBuilder().withUrl('/debugHub').build();
    debugClient.start().then(function () {
        setTimeout(debugStarted, 1000);
    });
});
//#endregion

function debugStarted() {
    window.addEventListener('error', sendError);
    debugClient.invoke('SendMessageAsync', 'Debug Client Startup : ' + window.document.URL);
}

function sendError(name, message) {
    console.log('Error ' + name + ' ' + message);
    debugClient.invoke('SendErrorAsync', name, message);
}

function sendMessage(message) {
    debugClient.invoke('SendMessageAsync', message);
}