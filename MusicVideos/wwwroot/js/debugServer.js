//#region Variables
'use strict';
let debugServer;
let debugOutput;
//#endregion
//#region Initialisation
window.addEventListener('load', function () {
    debugOutput = document.getElementById('debugOutput');
    debugServer = new signalR.HubConnectionBuilder().withUrl('/debugHub').build();
    debugServer.on('PrintError', function (name, message) {
        let newItem = document.createElement('li');
        newItem.innerHTML = 'Error ' + name + ' ' + message;
        debugOutput.appendChild(newItem);
    });
    debugServer.on('PrintMessage', function ( message) {
        console.log(message);
        let newItem = document.createElement('li');
        newItem.innerHTML = 'Message ' + name + ' ' + message;
        debugOutput.appendChild(newItem);
    });
    debugServer.start().then(function () {
        setTimeout(debugStarted, 1000);
    });
});
//#endregion