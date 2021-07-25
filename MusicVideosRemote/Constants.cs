namespace MusicVideosRemote
{
    using System;
    using System.IO;

    /// <summary>
    /// Constants class.
    /// </summary>
    [Obsolete("move to data store.")]
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
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Gets the path to the database. Varies by product.
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