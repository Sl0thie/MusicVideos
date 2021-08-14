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
let player0 = document.getElementById('player0');
let player1 = document.getElementById('player1');
let overlay = document.getElementById('overlay');
let songartist = document.getElementsByClassName('songartist');
let songtitle = document.getElementsByClassName('songtitle');
let clock = document.getElementsByClassName('clock');
let nextPlayer = 0;
let player0playing = false;
let player1playing = false;
let player0VideoId;
let player1VideoId;

//#endregion
//#region Initialization
window.addEventListener('load', function () {
    // Add event handlers
    player0.addEventListener('mousedown', screenClick, true);
    player1.addEventListener('mousedown', screenClick, true);
    overlay.addEventListener('mousedown', screenClick, true);
    player0.addEventListener('error', function (evt) {logError(evt.target.error);});
    player1.addEventListener('error', function (evt) {logError(evt.target.error);});

    // Create the SignalR object.
    connection = new signalR.HubConnectionBuilder().withUrl('/videoHub').build();

    // Define the methods that will be received from the hub.
    connection.on('setOutRegistrationAsync', function (id) {
        hubId = id;
        console.log('Hub Id: ' + hubId);
    });

    // Calls loadVideo when received.
    connection.on('loadVideo', function (video) {
        loadVideo(video);
    });

    // Calls playVideo when received.
    connection.on('playVideo', function (video, timeStr) {
        playVideo(video, timeStr);
    });

    // Sets the volume. Settings filter is unused.
    connection.on('SetOutVolumeAsync', function (volume) {
        player0.volume = volume / 100;
        player1.volume = volume / 100;
    })

    // Start the SignalR connection to the server hub.
    connection.start().then(function () {
        setTimeout(connectionStarted, startupPause);
    });

    // Hide the mouse cursor.
    player0.style.cursor = 'none';
    player1.style.cursor = 'none';
    overlay.style.cursor = 'none';
    overlayShadow.style.cursor = 'none';
});
// Once connected to hub get the first video to play.
function connectionStarted() {
    window.addEventListener('error', logError);
    connection.invoke('RegisterPlayerAsync','123456');
}
//#endregion
//#region Log.Info
function logError(e) {
    console.log('ERROR: ' + e.message);
    connection.invoke('SetInJavascriptErrorAsync', document.title, e.message, e.filename, e.lineno, e.colno);
}
function log(message) {
    connection.invoke('LogMessageAsync', document.URL, message);
}
//#endregion

function loadVideo(video) {

    console.log('loadVideo');

    try {
        var videoObj = JSON.parse(video);

        console.log('Id: ' + videoObj.Id);
        console.log('Artist: ' + videoObj.Artist);
        console.log('Title: ' + videoObj.Title);
        console.log('VirtualPath: ' + videoObj.VirtualPath);

        if (nextPlayer == 0) {
            player0.src = videoObj.VirtualPath;
        }
        else {
            player1.src = videoObj.VirtualPath;
        }
    }
    catch (error) {
        logError(error);
    }
}

function playVideo(video, timeStr) {

    console.log('playVideo');

    try {
        var obj = JSON.parse(video);
        var timeObj = JSON.parse(timeStr);
        timeObj = new Date(timeObj);
        var date2_ms = timeObj.getTime();
        var date3_ms = new Date().getTime();
        var diff2 = date2_ms - date3_ms;

        console.log('Diff2: ' + diff2);

        if (nextPlayer == 0) {
            if (player1playing == true) {
                player1.pause();
            }
            player1.style.display = 'none';
            player0.style.display = 'initial';
            player0VideoId = obj.Id.toString();
            player0.play();
            nextPlayer = 1;
        }
        else {
            if (player0playing == true) {
                player0.pause();
            }
            player0.style.display = 'none';
            player1.style.display = 'initial';
            player1VideoId = obj.Id.toString();
            player1.play();
            nextPlayer = 0;
        }

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
        for (const element of songartist) { element.innerHTML = obj.Artist; }
        for (const element of songtitle) { element.innerHTML = obj.Title; }
        for (const element of clock) { element.innerHTML = cTime; }

        // hide the mouse cursor again. Some browsers don't seem to keep the cursor hidden (Samsung)
        player0.style.cursor = 'none';
        player1.style.cursor = 'none';
        overlay.style.cursor = 'none';
        overlayShadow.style.cursor = 'none';
        // Start the display fadeout process.
        fadeoutStart();

    }
    catch (error) {
        logError(error);
    }

    console.log('Id: ' + obj.Id);
    console.log('Artist: ' + obj.Artist);
    //console.log('SearchArtist: ' + obj.SearchArtist);
    console.log('Title: ' + obj.Title);
    //console.log('Album: ' + obj.Album);
    //console.log('Path: ' + obj.Path);
    //console.log('Genres: ' + obj.Genres);
    //console.log('Extension: ' + obj.Extension);
    //console.log('Duration: ' + obj.Duration);
    //console.log('VideoBitRate: ' + obj.VideoBitRate);
    //console.log('VideoWidth: ' + obj.VideoWidth);
    //console.log('VideoHeight: ' + obj.VideoHeight);
    //console.log('VideoFPS: ' + obj.VideoFPS);
    //console.log('PlayCount: ' + obj.PlayCount);
    //console.log('QueuedCount: ' + obj.QueuedCount);
    //console.log('PlayTime: ' + obj.PlayTime);
    //console.log('Rating: ' + obj.Rating);
    //console.log('LastPlayed: ' + obj.LastPlayed);
    //console.log('LastQueued: ' + obj.LastQueued);
    //console.log('Released: ' + obj.Released);
    //console.log('Added: ' + obj.Added);
    //console.log('Errors: ' + obj.Errors);
    //console.log('PhysicalPath: ' + obj.PhysicalPath);
    //console.log('VirtualPath: ' + obj.VirtualPath);

    //const timeObj = JSON.parse(time);
    //console.log('TimeObj: ' + timeObj);
    //console.log('Time: ' + time);
}

function screenClick() {
    connection.invoke('ScreenClickAsync', hubId);
}

function player0play() {
    player0playing = true;
}

function player0ended() {
    player0playing = false;
}

function player1play() {
    player1playing = true;
}

function player1ended() {
    player1playing = false;
}

function player0ready() {
    connection.invoke('UpdateVideoPropertiesAsync', hubId, player0VideoId, player0.duration.toString(), player0.videoWidth.toString(), player0.videoHeight.toString());
}

function player0error() {

}

function player1ready() {
    connection.invoke('UpdateVideoPropertiesAsync', hubId, player1VideoId, player1.duration.toString(), player1.videoWidth.toString(), player1.videoHeight.toString());
}

function player1error() {

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