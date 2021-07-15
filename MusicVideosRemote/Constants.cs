﻿namespace MusicVideosRemote
{
    using System;
    using System.IO;

    public static class Constants
    {
        /// <summary>
        /// The name of the database file.
        /// </summary>
        public const string DatabaseFilename = "SQLite.db3";

        /// <summary>
        /// Flags used by Sqlite.
        /// </summary>
        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// The path to the database. Varies by product.
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }
}