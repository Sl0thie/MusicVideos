//#region Variables
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
let hubId = '';

// Element objects.
let player = document.getElementById('player');
let overlay = document.getElementById('overlay');
let songartist = document.getElementsByClassName('songartist');
let songtitle = document.getElementsByClassName('songtitle');
let clock = document.getElementsByClassName('clock');
let nextPlayer = 0;
let playerplaying = false;
let playerVideoId = 0;

//#endregion
//#region Initialization
window.addEventListener('load', function () {
    // Add event handlers
    player.addEventListener('mousedown', screenClick, true);
    overlay.addEventListener('mousedown', screenClick, true);
    player.addEventListener('error', function (evt) {logError(evt.target.error);});

    // Create the SignalR object.
    connection = new signalR.HubConnectionBuilder().withUrl('/dataHub').build();

    // Calls playVideo when received.
    connection.on('clientPlayVideo', function (video) {
        playVideo(video);
    });

    // Sets the volume. Settings filter is unused.
    connection.on('setOutVolumeAsync', function (volume) {
        player.volume = volume / 100;
    })

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
    //connection.invoke('Play');
}
//#endregion

function logError(e) {
    console.log('ERROR: ' + e.name + ' ' + e.message);
}

function playVideo(video) {

    console.log('playVideo');

    try {
        player.src = video.virtualPath.replace("''","'");
        playerVideoId = video.id.toString();
        player.play();

        var current = new Date();
        var hour = current.getHours();
        var minute = current.getMinutes();
        if (hour > 12) {
            hour = hour - 12;
        }
        if (minute < 10) {
            minute = "0" + minute;
        }
        let cTime = hour + ":" + minute;

        // Update display elements.
        for (const element of songartist) { element.innerHTML = video.artist; }
        for (const element of songtitle) { element.innerHTML = video.title; }
        for (const element of clock) { element.innerHTML = cTime; }

        // hide the mouse cursor again. Some browsers don't seem to keep the cursor hidden (Samsung)
        player.style.cursor = 'none';
        overlay.style.cursor = 'none';
        overlayShadow.style.cursor = 'none';

        // Start the display fadeout process.
        fadeoutStart();
    }
    catch (error) {
        logError(error);
        connection.invoke('ServerPlayerError', parseInt(playerVideoId));
    }

    console.log('Id: ' + video.id);
    console.log('Artist: ' + video.artist);
    console.log('Title: ' + video.title);
}

function screenClick() {
    console.log('function screenClick ' + playerVideoId);
    connection.invoke('ServerScreenClick', parseInt(playerVideoId));
}

function playerplay() {
    playerplaying = true;
}

function playerended() {
    playerplaying = false;
    connection.invoke('ServerPlayerEnded', parseInt(playerVideoId));
}

function playerready() {
    connection.invoke('ServerUpdateVideoProprties', playerVideoId, player.duration.toString(), player.videoWidth.toString(), player.videoHeight.toString());   
}

function playererror() {
    connection.invoke('ServerPlayerError', parseInt(playerVideoId));
}

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