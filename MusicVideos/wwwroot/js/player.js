//#region Variables
'use strict';
let fadeoutStartId;                                             // Fadeout interval Id.
let fadeoutId;                                                  // Fade interval Id.
let opacity;                                                    // Overlay opacity value.
let connection;                                                 // SignalR connection object.
let volume = 1;                                                 // Volume for the video element.
let player = document.getElementById('player');                 // Player object.
let overlay = document.getElementById('overlay');               // Text overlay object.
let songartist = document.getElementsByClassName('songartist'); // Artist span object.
let songtitle = document.getElementsByClassName('songtitle');   // Title span object.
let clock = document.getElementsByClassName('clock');           // Clock span object.
//#endregion
//#region Initialisation
window.addEventListener('load', function () {
    player.addEventListener('mousedown', nextSong, true);
    overlay.addEventListener('mousedown', nextSong, true);

    connection = new signalR.HubConnectionBuilder().withUrl('/messageHub').build();
    connection.on('SetVideo', function (index, songPath, songArtist, songTitle, clockTime) {
        setSong(index, songPath, songArtist, songTitle, clockTime);
    });
    connection.on('MediaPlay', function () {
        player.play();
    });
    connection.on('MediaPause', function () {
        player.pause();
    });
    connection.on('SetVolume', function (value) {
        console.log('SetVolume() ' + value)
        player.volume = value / 100;
    });
    connection.start().then(function () {
        setTimeout(connectionStarted, 1000);
    });
});
//#endregion
//#region Media
function nextSong() {
    connection.invoke('GetNextSongAsync');
}

function connectionStarted() {
    connection.invoke('GetNextSongAsync');
}

function playerended() {
    connection.invoke('GetNextSongAsync');
}

function playerError() {
    connection.invoke('GetNextSongAsync');
}

function playerReady() {
    player.play();
}

function setSong(index, songPath, songArtist, songTitle, clockTime) {
    try {
        for (const element of songartist) { element.innerHTML = songArtist; }
        for (const element of songtitle) { element.innerHTML = songTitle; }
        for (const element of clock) { element.innerHTML = clockTime; }
        player.src = songPath;
        player.style.cursor = 'none';
        overlay.style.cursor = 'none';
        overlayShadow.style.cursor = 'none';

        fadeoutStart();
    }
    catch (err) {
        console.log(err.message);
    } 
}

//#endregion
//#region Overlay
function fadeoutStart() {
    try {
        overlay.style.opacity = 1
        overlayShadow.style.opacity = 1
        clearTimeout(fadeoutStartId);
        clearInterval(fadeoutId);
        fadeoutStartId = setTimeout(fadeout, 10000);
    }
    catch (err) {
        console.log(err.message);
    } 
}

function fadeout() {
    try {
        clearInterval(fadeoutStartId);
        clearInterval(fadeoutId);
        fadeoutId = setInterval(hide, 50);
    }
    catch (err) {
        console.log(err.message);
    }
}

function hide() {
    opacity = Number(window.getComputedStyle(overlay).getPropertyValue('opacity'))
    if (opacity > 0) {
        opacity = opacity - 0.01;
        overlay.style.opacity = opacity
        overlayShadow.style.opacity = opacity
    }
    else {
        clearInterval(fadeoutId);
    }
}

//#endregion