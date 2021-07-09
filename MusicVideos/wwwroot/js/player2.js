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

//#endregion
//#region Initialization
window.addEventListener('load', function () {
    player0.addEventListener('mousedown', screenClick, true);
    player1.addEventListener('mousedown', screenClick, true);
    overlay.addEventListener('mousedown', screenClick, true);

    // Create the SignalR object.
    connection = new signalR.HubConnectionBuilder().withUrl('/videoHub').build();

    // Define the methods that will be received from the hub.
    connection.on('setRegistration', function (id) {
        hubId = id;
        console.log('Hub Id: ' + hubId);
    });

    // Define the methods that will be received from the hub.
    connection.on('loadVideo', function (video) {
        loadVideo(video);
    });

    connection.on('playVideo', function (video, time) {
        playVideo(video, time);
    });

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
//#region Debug
function logError(e) {
    console.log('ERROR: ' + e.message);
    connection.invoke('LogErrorAsync', document.title, e.message, e.filename, e.lineno.toString(), e.colno.toString());
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

function playVideo(video, time) {

    console.log('playVideo');

    try {
        const obj = JSON.parse(video);

        if (nextPlayer == 0) {
            if (player1playing == true) {
                player1.pause();
            }
            player1.style.display = 'none';
            player0.style.display = 'initial';
            player0.play();
            nextPlayer = 1;
        }
        else {
            if (player0playing == true) {
                player0.pause();
            }
            player0.style.display = 'none';
            player1.style.display = 'initial';
            player1.play();
            nextPlayer = 0;
        }
    }
    catch (error) {
        logError(error);
    }

    //console.log('Id: ' + obj.Id);
    //console.log('Artist: ' + obj.Artist);
    //console.log('SearchArtist: ' + obj.SearchArtist);
    //console.log('Title: ' + obj.Title);
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

}

function player0error() {

}

function player1ready() {

}

function player1error() {

}