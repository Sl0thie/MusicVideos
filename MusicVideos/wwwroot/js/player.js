﻿//#region Variables
'use strict';
const maxVolume = 100;         // The volume is based between 0 and 1 but 100 is used for the sliders.
const startupPause = 1000;     // Delay between the start of SignalR and sending the first message.
const minOpacity = 0;          // Minimum opacity for the elements.
const maxOpacity = 1;          // Maximum opacity for the elements.
const incrementOpacity = 0.01; // Amount to decrease the opacity each loop.
const intervalDisplay = 10000; // Time interval to display the artist, title and time.
const intervalFade = 50;       // 50 milliseconds is 3 frames. 33 is 2 frames. 17 is 1 frame.
let fadeoutStartId;            // Fadeout interval Id.
let fadeoutId;                 // Fade interval Id.
let opacity;                   // Overlay opacity value.
let connection;                // SignalR connection object.
let volume = 1;                // Volume for the video element.
let currentIndex;              // Index of the current Video.

// Element objects.
let player = document.getElementById('player'); 
let overlay = document.getElementById('overlay');
let songartist = document.getElementsByClassName('songartist');
let songtitle = document.getElementsByClassName('songtitle');
let clock = document.getElementsByClassName('clock');
//#endregion
//#region Initialization
window.addEventListener('load', function () {
    player.addEventListener('mousedown', nextVideo, true);
    overlay.addEventListener('mousedown', nextVideo, true);
    // Create the SignalR object.
    connection = new signalR.HubConnectionBuilder().withUrl('/messageHub').build();
    // Define the methods that will be received from the hub.
    connection.on('SetVideo', function (index, songPath, songArtist, songTitle, clockTime) {
        setVideo(index, songPath, songArtist, songTitle, clockTime);
    });
    connection.on('MediaPlay', function () {
        player.play();
    });
    connection.on('MediaPause', function () {
        player.pause();
    });
    connection.on('SetVolume', function (value) {
        console.log('SetVolume() ' + value)
        player.volume = value / maxVolume;
    });
    // Start the SignalR connection to the server hub.
    connection.start().then(function () {
        setTimeout(connectionStarted, startupPause);
    });
    // Hide the mouse cursor.
    player.style.cursor = 'none';
    overlay.style.cursor = 'none';
    overlayShadow.style.cursor = 'none';
});
// Once connected to hub get the first video to play.
function connectionStarted() {
    window.addEventListener('error', logError);
    connection.invoke('GetNextVideoAsync');
}
//#endregion
//#region Log.Info
function logError(e) {
    connection.invoke('LogErrorAsync', document.title, e.message, e.filename, e.lineno.toString(), e.colno.toString());
}
function log(message) {
    connection.invoke('LogMessageAsync', document.URL, message);
}
//#endregion
//#region Media
// Get the next video from the hub.
function nextVideo() {
    connection.invoke('GetNextVideoAsync');
}

// When the video has ended get another video to play from the hub.
function playerended() {
    connection.invoke('GetNextVideoAsync');
}
// If there is an error with a video then get the next video.
// TODO Add logging to this function to log the video that has raised the error on the hub.
function playerError() {
    connection.invoke('GetNextVideoAsync');
}
// When the player is ready auto start the video.
function playerReady() {
    player.play();
    connection.invoke('UpdateVideoProperties', currentIndex.toString(), player.duration.toString(), player.videoWidth.toString(), player.videoHeight.toString());
}
// Play the next video.
function setVideo(index, songPath, songArtist, songTitle, clockTime) {
    currentIndex = index;
    // Update display elements.
    for (const element of songartist) { element.innerHTML = songArtist; }
    for (const element of songtitle) { element.innerHTML = songTitle; }
    for (const element of clock) { element.innerHTML = clockTime; }
    // Update the player's source. (video element auto plays)
    player.src = songPath;
    // hide the mouse cursor again. Some browsers don't seem to keep the cursor hidden (Samsung)
    player.style.cursor = 'none';
    overlay.style.cursor = 'none';
    overlayShadow.style.cursor = 'none';
    // Start the display fadeout process.
    fadeoutStart();
}

//#endregion
//#region Overlay
// Start title display process by making the elements visible and starting the timer.
function fadeoutStart() {
    overlay.style.opacity = maxOpacity;
    overlayShadow.style.opacity = maxOpacity;
    clearTimeout(fadeoutStartId);
    clearInterval(fadeoutId);
    fadeoutStartId = setTimeout(fadeout, intervalDisplay);
}
// Begin the fading loop to slowly fade the display.
function fadeout() {
    clearInterval(fadeoutStartId);
    clearInterval(fadeoutId);
    fadeoutId = setInterval(hide, intervalFade);
}
// Reduce the visibility of the display and check if it is completely faded. Stop the loop if true.
function hide() {
    opacity = Number(window.getComputedStyle(overlay).getPropertyValue("opacity"))
    if (opacity > minOpacity) {
        opacity = opacity - incrementOpacity;
        overlay.style.opacity = opacity
        overlayShadow.style.opacity = opacity
    }
    else {
        clearInterval(fadeoutId);
    }
}
//#endregion
// Rating Histogram
function showHistogram() {
     
}
function hideHistogram() {

}
//#endregion