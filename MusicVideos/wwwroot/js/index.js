﻿
//#region Variables
'use strict';
let connection;
let debugOutput;
//#endregion
//#region Initialisation
window.addEventListener('load', function () {
    debugOutput = document.getElementById('debugOutput');
    connection = new signalR.HubConnectionBuilder().withUrl('/messageHub').build();
    connection.on('PrintError', function (docTitle, message, filename, lineNo, colNo) {
        let line1 = document.createElement('li');
        line1.innerHTML = 'ERROR ' + docTitle;
        let line2 = document.createElement('li');
        line2.innerHTML = 'Message ' + message;
        let line3 = document.createElement('li');
        line3.innerHTML = 'Filename ' + filename;
        let line4 = document.createElement('li');
        line4.innerHTML = 'LineNumber ' + lineNo;
        let line5 = document.createElement('li');
        line5.innerHTML = 'Column Number ' + colNo;
        debugOutput.appendChild(line1);
        debugOutput.appendChild(line2);
        debugOutput.appendChild(line3);
        debugOutput.appendChild(line4);
        debugOutput.appendChild(line5);
    });
    connection.on('PrintMessage', function (docTitle,message) {
        console.log(message);
        let newItem = document.createElement('li');
        newItem.innerHTML = docTitle + ' Message : ' + message;
        debugOutput.appendChild(newItem);
    });
    connection.start();
});
//#endregion