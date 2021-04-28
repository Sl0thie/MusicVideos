# MusicVideos
A .NET Core web application that provides a html player for a smart tv and a html remote for a smart phone to make a video jukebox. It uses SignalR to communicate between the pages and the server.

![musicvideos](https://user-images.githubusercontent.com/28429345/116331378-fe0bdc80-a812-11eb-973e-7592a99743ea.png)

The remote provides the ability to filter the list of videos as well as control the playback. Volume can also be adjusted, this is currently being extended to include a video by video volume offset to provide a manual adjustment to level the volume between videos.

There is also a html page for neighbours to access to notifiy their disgust at how loud my teenager's are playing their music. Its still in progress though.

#Player
This cshtml razor page provides a simple video player for output of the video. This can be a smart tv, monitor or tablet. As each video plays the artist, title and local time are displayed for several seconds then fade away.

#Remote
The remote razor page provides control of the Player via a SignalR hub.

#File Importer
File Importer is a console application that imports video files to a base directory for using in the web application. It stores the files in a folder for the artist and then a folder for the video title. If then creates a thumbnail for the video as well as two waveform files using ffmepg.
