//#region Variables
'use strict';
const maxVolume = 100;               // The volume is based between 0 and 1 but 100 is used for the sliders.
const startupPause = 1000;           // Delay between the start of SignalR and sending the first message.
const indexMinimum = 0;              // The minimum index value.
const millisecondsPerDay = 86400000; // Number of Milliseconds in a day.
const maximumDays = 365;             // Maximum days to set a cookie.
const overlayZIndex = 2;             // Z-Index for the overlay elements.
let connection;                      // SignalR connection.

// Element objects.
let Id = document.getElementById('Id');
let headArtist = document.getElementById('headArtist');
let headTitle = document.getElementById('headTitle');
let songlist = document.getElementById('songlist');
let queuelist = document.getElementById('queuelist');
let popupVolume = document.getElementById('popupVolume');
let formVolume = document.getElementById('formVolume');
let volumeValue = document.getElementById('volumeValue');
let volumeImage = document.getElementById('volumeImage');
let popupSettings = document.getElementById('popupSettings');
let formSettings = document.getElementById('formSettings');
let popupDetails = document.getElementById('popupDetails');
let formDetails = document.getElementById('formDetails');
let detailsRating = document.getElementById('detailsRating');
let popupFilter = document.getElementById('popupFilter');
let formFilter = document.getElementById('formFilter');
let toggleShowAll = document.getElementById('toggleShowAll');
let toggleShowUnrated = document.getElementById('toggleShowUnrated');
let sliderFilterRating = document.getElementById('sliderFilterRating');
let valueFilterRating = document.getElementById('valueFilterRating');

//#endregion
//#region Initialization
window.addEventListener('load', function () {
    popupVolume.addEventListener('mousedown', closeVolume);
    formVolume.addEventListener('mousedown', cancelEvent);
    popupSettings.addEventListener('mousedown', closeSettings);
    formSettings.addEventListener('mousedown', cancelEvent);
    popupDetails.addEventListener('mousedown', closeDetails);
    formDetails.addEventListener('mousedown', cancelEvent);
    popupFilter.addEventListener('mousedown', closeFilter);
    formFilter.addEventListener('mousedown', cancelEvent);
    connection = new signalR.HubConnectionBuilder().withUrl('/messageHub').build();
    connection.on('SetVideo', function (path, artist, title, timeStr) {
        setVideo(path, artist, title, timeStr);
    });
    connection.on('ClearPlaylist', function () {
        clearPlaylist();
    });
    connection.on('SetPlaylistItem', function (id, artist, title, rating) {
        addPlaylistItem(id, artist, title, rating);
    });
    connection.on('ClearQueuelist', function () {
        clearQueuelist();
    });
    connection.on('SetQueuelistItem', function (id, artist, title) {
        addQueuelistItem(id, artist, title);
    });
    connection.on('SetVideoDetails', function (duration, extension, genreslist, lastplayed, rating, released) {
        setVideoDetails(duration, extension, genreslist, lastplayed, rating, released);
    });
    connection.start().then(function () {
        setTimeout(connectionStarted, startupPause);
    });
});

function connectionStarted() {
    // Set volume from previous session via cookie.
    let cookieVolume = getCookie('volume');
    if (cookieVolume === '') {
        cookieVolume = maxVolume;
    }
    let sliderVolume = document.getElementById('sliderVolume');
    sliderVolume.value = cookieVolume;
    updateVolume(cookieVolume);
    connection.invoke('GetPlaylistAsync');
}

//#endregion
//#region Playlist
function setVideo(index, path, artist, title, timeStr) {
    Id.value = index;
    headArtist.innerText = artist;
    headTitle.innerText = title;
    connection.invoke('GetVideoDetails', index.toString());
}

function clearPlaylist() {
    songlist.innerHTML = '';
}

function addPlaylistItem(id, artist, title, rating) {
    let newOuter = document.createElement('div');
    let newArtist = document.createElement('div');
    let newTitle = document.createElement('div');
    newArtist.innerHTML = artist;
    newArtist.className = 'itemartist';
    newTitle.innerHTML = title;
    newTitle.className = 'itemtitle';
    newOuter.appendChild(newArtist);
    newOuter.appendChild(newTitle);
    newOuter.onmousedown = function () { mouseDownItem(id); };
    newOuter.onmouseup = function () { mouseUpItem(id); };
    newOuter.id = id;
    newOuter.className = 'item';
    songlist.appendChild(newOuter);
    doEvents();
}

function doEvents() {
    var promise = new Promise(
        function (resolve, reject) {
            requestAnimationFrame(function () {
                resolve();
            });
        });
    return promise;
}

function mouseDownItem(id) {
    addToQueue(id);
    let selectedItem = document.getElementById(id);
    let artist = selectedItem.getElementsByClassName('itemartist');
    let title = selectedItem.getElementsByClassName('itemtitle');
    artist[indexMinimum].style.color = '#AAAAFF';
    title[indexMinimum].style.color = '#AAAAFF';
}

function mouseUpItem(id) {
    let selectedItem = document.getElementById(id);
    let artist = selectedItem.getElementsByClassName('itemartist');
    let title = selectedItem.getElementsByClassName('itemtitle');
    artist[indexMinimum].style.color = '#2424FF';
    title[indexMinimum].style.color = '#2424FF';
}

function addToQueue(id) {
    connection.invoke('AddToQueueAsync', id.toString());
    connection.invoke("GetQueuelistAsync");
}

//#endregion
//#region Queuelist

function addQueuelistItem(id, artist, title) {
    let newOuter = document.createElement('div');
    let newArtist = document.createElement('div');
    let newTitle = document.createElement('div');
    newArtist.innerHTML = artist;
    newArtist.className = 'itemartist';
    newTitle.innerHTML = title;
    newTitle.className = 'itemtitle';
    newOuter.appendChild(newArtist);
    newOuter.appendChild(newTitle);
    newOuter.onmousedown = function () { mouseDownItem(id); };
    newOuter.onmouseup = function () { mouseUpItem(id); };
    newOuter.id = id;
    newOuter.className = 'item';
    queuelist.appendChild(newOuter);
    doEvents();
}

function clearQueuelist() {
    console.log('Clear Log');
    queuelist.innerHTML = '';
}

//#endregion
//#region Buttons
function buttonPrevious() {
    connection.invoke('ButtonPrevAsync');
}

function buttonPlay() {
    connection.invoke('ButtonPlayAsync');
}

function buttonPause() {
    connection.invoke('ButtonPauseAsync');
}

function buttonNext() {
    connection.invoke('ButtonNextAsync');
}

function buttonVolume() {
    popupVolume.style.display = 'block';
}

function buttonDetails() {
    popupDetails.style.display = 'block';
}

function buttonFilter() {
    popupFilter.style.display = 'block';
}

function buttonSettings() {
    popupSettings.style.display = 'block';
}

function buttonPlaylist() {
    songlist.style.zIndex = overlayZIndex;
    queuelist.style.zIndex = -overlayZIndex;
}

function buttonQueuelist() {
    songlist.style.zIndex = -overlayZIndex;
    queuelist.style.zIndex = overlayZIndex;
}

//#endregion
//#region Genres

function checkClick(id) {
    let check = document.getElementById('check' + id);
    let source = check.src;
    let i = source.lastIndexOf('/');
    source = source.substring(i);

    if (source === '/checkoff.png') {
        check.src = '/images/checkon.png';
        setGenres(id, 'add');
    }
    else {
        check.src = '/images/checkoff.png';
        setGenres(id, 'remove');
    }
}

function setGenres(genreId, state) {
    connection.invoke('SetGenres', Id.value, genreId.toString(), state);
}

//#endregion
//#region Volume
function updateVolume(value) {
    setCookie('volume', value, maximumDays);
    volumeValue.innerText = value;
    connection.invoke('SetVolumeAsync', value);

    if (value > 75) {
        volumeImage.src = '/images/volume75.png';
    }
    else if (value > 50) {
        volumeImage.src = '/images/volume50.png';
    }
    else if (value > 25) {
        volumeImage.src = '/images/volume25.png';
    }
    else if (value > 0) {
        volumeImage.src = '/images/volume0.png';
    }
    else {
        volumeImage.src = '/images/volumeMute.png';
    }
}

function closeVolume() {
    popupVolume.style.display = "none";
    event.stopPropagation();
}

//#endregion
//#region Rating
function updateRating(value) {
    detailsRating.innerText = value;
    connection.invoke('SetRating', Id.value, value);
}

//#endregion
//#region Details
function closeDetails() {
    popupDetails.style.display = "none";
    event.stopPropagation();
}

function cancelEvent() {
    event.stopPropagation();
}

function setVideoDetails(duration, extension, genreslist, lastplayed, rating, released) {
    let detailsArtist = document.getElementById('detailsArtist');
    let detailsTitle = document.getElementById('detailsTitle');
    let detailsDuration = document.getElementById('detailsDuration');
    let detailsExtension = document.getElementById('detailsExtension');
    let detailsLastplayed = document.getElementById('detailsLastplayed');
    let detailsRating = document.getElementById('detailsRating');
    let sliderRating = document.getElementById('sliderRating');
    let detailsReleased = document.getElementById('detailsReleased');
    detailsArtist.innerText = headArtist.innerText;
    detailsTitle.innerText = headTitle.innerText;
    detailsDuration.innerText = duration;
    detailsExtension.innerText = extension;
    detailsLastplayed.innerText = lastplayed;
    detailsRating.innerText = rating;
    sliderRating.value = rating;
    detailsReleased.innerText = released;

    let i;
    for (i = 1; i < 21; i++) {
        let check = document.getElementById('check' + i);
        check.src = '/images/checkoff.png';
    }
    var genres = JSON.parse(genreslist);
    genres.forEach(updateGenre);

    connection.invoke("GetQueuelistAsync");
}

function updateGenre(item, index) {
    let check = document.getElementById('check' + item);
    check.src = '/images/checkon.png';
}

//#endregion
//#region Settings
function closeSettings() {
    popupSettings.style.display = "none";
    event.stopPropagation();
}
//#endregion
//#region Filter
function closeFilter() {
    popupFilter.style.display = 'none';
    setFilter();
    connection.invoke("GetPlaylistAsync");
    event.stopPropagation();
}

function setFilterGenre(id) {
    let filter = document.getElementById('filter' + id);
    let source = filter.src;
    let i = source.lastIndexOf('/');
    source = source.substring(i);

    if (source === '/checkoff.png') {
        filter.src = '/images/checkon.png';
        toggleShowUnrated.src = '/images/checkoff.png';
        toggleShowAll.src = '/images/checkoff.png';
        //setFilter(id, 'add');
        connection.invoke('SetFilterGenre', id.toString(), 'add');
    }
    else {
        filter.src = '/images/checkoff.png';
        //setFilter(id, 'remove');
        connection.invoke('SetFilterGenre', id.toString(), 'remove');
    }
}

function switchShowAll() {
    let source = toggleShowAll.src;
    let i = source.lastIndexOf('/');
    source = source.substring(i);

    if (source === '/checkoff.png') {
        toggleShowAll.src = '/images/checkon.png';
        toggleShowUnrated.src = '/images/checkoff.png';

        for (let i = 1; i < 21; i++) {
            let filter = document.getElementById('filter' + i);
            filter.src = '/images/checkoff.png';
        }
    }
    else {
        toggleShowAll.src = '/images/checkoff.png';
    }
}

function switchShowNoGenre() {
    let source = toggleShowUnrated.src;
    let i = source.lastIndexOf('/');
    source = source.substring(i);

    if (source === '/checkoff.png') {
        toggleShowUnrated.src = '/images/checkon.png';
        toggleShowAll.src = '/images/checkoff.png';

        for (let i = 1; i < 21; i++) {
            let filter = document.getElementById('filter' + i);
            filter.src = '/images/checkoff.png';
        }
    }
    else {
        toggleShowUnrated.src = '/images/checkoff.png';
    }
}

function updateFilterRating() {
    valueFilterRating.innerText = sliderFilterRating.value;
}

function setFilter() {
    let showall;
    let showunrated;
    let filterrating;

    let source = toggleShowAll.src;
    let i = source.lastIndexOf('/');
    source = source.substring(i);
    if (source === '/checkoff.png') {
        showall = 'false';
    }
    else {
        showall = 'true';
    }

    source = toggleShowUnrated.src;
    i = source.lastIndexOf('/');
    source = source.substring(i);
    if (source === '/checkoff.png') {
        showunrated = 'false';
    }
    else {
        showunrated = 'true';
    }

    filterrating = valueFilterRating.innerText;
    connection.invoke('SetFilter', showall, showunrated, filterrating);
}

//#endregion
//#region Cookie
function clearcookie() {
    setCookie('volume', '', 0);
}

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * millisecondsPerDay));
    var expires = 'expires=' + d.toUTCString();
    document.cookie = cname + '=' + cvalue + ';' + expires + ';path=/';
}

function getCookie(cname) {
    var name = cname + '=';
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return '';
}

//#endregion