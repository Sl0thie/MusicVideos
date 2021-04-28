//#region Variables
"use strict";
let connection;
let clockcounter;
let countId;
let clock = document.getElementById("clock"); 
let buttonVolume = document.getElementById("buttonVolume"); 

//#endregion
//#region Initialisation
window.onload = initialise();

function initialise() {
    connection = new signalR.HubConnectionBuilder().withUrl("/messageHub").build();

    connection.start().then(function () {
        setTimeout(connectionStarted, 1000);
    });
}

//#endregion
//#region Count

function lowerVolume() {
    connection.invoke("LowerVolume");
    startCount();
}

function startCount() {
    clearInterval(countId);
    clockcounter = 60;
    clock.style.display = "block";
    buttonVolume.disabled = true;
    countId = setInterval(count, 1000);
}

function count() {
    clockcounter--;
    clock.innerText = clockcounter;
    if (clockcounter === 0) {
        endCount();
    }
}

function endCount() {
    clock.style.display = "none";
    buttonVolume.disabled = false;
    clearInterval(countId);
}

//#endregion