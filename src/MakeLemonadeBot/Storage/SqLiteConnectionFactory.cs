using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace MakeLemonadeBot.Storage
{
    public class SqLiteConnectionFactory
    {
        private readonly string connectionString;

        private readonly ILookup<Schema, string> migrations = Lookup.Create(
            (
                Schema.Zero,
                @"CREATE TABLE movies (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    original_name TEXT,
                    year INT
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE shown_movies (
                    chat_id INT NOT NULL,
                    movie_id INT NOT NULL
                    REFERENCES movies (id)
                    ON UPDATE CASCADE
                    ON DELETE CASCADE,
                    state TEXT NOT NULL,
                    at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX shown_movies_ids
                ON shown_movies (chat_id, movie_id)"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX to_be_shown_movies
                ON shown_movies (state)
                WHERE state = 'to-be-shown'"
            ),

            (
                Schema.Zero,
                @"CREATE TABLE books (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    author TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE shown_books (
                    chat_id INT NOT NULL,
                    book_id INT NOT NULL
                    REFERENCES books (id)
                    ON UPDATE CASCADE
                    ON DELETE CASCADE,
                    state TEXT NOT NULL,
                    at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX shown_books_ids
                ON shown_books (chat_id, book_id)"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX to_be_shown_books
                ON shown_books (state)
                WHERE state = 'to-be-shown'"
            ),

            (
                Schema.Zero,
                @"CREATE TABLE serials (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    original_name TEXT NOT NULL,
                    year INT NOT NULL,
                    duration TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE shown_serials (
                    chat_id INT NOT NULL,
                    serial_id INT NOT NULL
                    REFERENCES serials (id)
                    ON UPDATE CASCADE
                    ON DELETE CASCADE,
                    state TEXT NOT NULL,
                    at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX shown_serials_ids
                ON shown_serials (chat_id, serial_id)"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX to_be_shown_serials
                ON shown_serials (state)
                WHERE state = 'to-be-shown'"
            ),

            (
                Schema.Zero,
                @"CREATE TABLE cartoons (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    original_name TEXT NOT NULL,
                    year INT NOT NULL,
                    note TEXT
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE shown_cartoons (
                    chat_id INT NOT NULL,
                    cartoon_id INT NOT NULL
                    REFERENCES cartoons (id)
                    ON UPDATE CASCADE
                    ON DELETE CASCADE,
                    state TEXT NOT NULL,
                    at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX shown_cartoons_ids
                ON shown_cartoons (chat_id, cartoon_id)"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX to_be_shown_cartoons
                ON shown_cartoons (state)
                WHERE state = 'to-be-shown'"
            ),

            (
                Schema.Zero,
                @"CREATE TABLE storeroom (
                    id TEXT NOT NULL PRIMARY KEY,
                    content TEXT NOT NULL,
                    description TEXT,
                    category TEXT
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE shown_things (
                    chat_id INT NOT NULL,
                    thing_id INT NOT NULL
                    REFERENCES storeroom (id)
                    ON UPDATE CASCADE
                    ON DELETE CASCADE,
                    state TEXT NOT NULL,
                    at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX shown_things_ids
                ON shown_things (chat_id, thing_id)"
            ),
            (
                Schema.Zero,
                @"CREATE UNIQUE INDEX to_be_shown_things
                ON shown_things (state)
                WHERE state = 'to-be-shown'"
            ),

            (
                Schema.Zero,
                @"CREATE TABLE conversations (
                    chat_id INT PRIMARY KEY,
                    content TEXT NOT NULL,
                    expires_at TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE offsets (
                    token TEXT PRIMARY KEY,
                    value INT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"CREATE TABLE messages (
                    id INT NOT NULL,
                    chat_id INT NOT NULL,
                    date TEXT NOT NULL,
                    text TEXT NOT NULL,
                    author TEXT NOT NULL
                )"
            ),
            (
                Schema.Zero,
                @"REPLACE INTO schema (subject, version)
                VALUES ('root', 'Initial')"
            )
        );

        public SqLiteConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SQLiteConnection Create()
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            CreateSchema(connection);
            return connection;
        }

        private void CreateSchema(SQLiteConnection connection)
        {
            using var transaction = connection.BeginTransaction();
            var visitedSchemas = new HashSet<Schema>();
            while (true)
            {
                var version = SchemaVersion(transaction);
                if (!visitedSchemas.Add(version))
                    throw new Exception($"There is a bug in schema migration from {version}");

                if (!migrations.Contains(version))
                    break;

                foreach (var query in migrations[version])
                {
                    using var command = new SQLiteCommand(query, connection, transaction);
                    command.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        private static Schema SchemaVersion(SQLiteTransaction transaction)
        {
            transaction.Execute(@"
                CREATE TABLE IF NOT EXISTS schema (
                    subject TEXT PRIMARY KEY,
                    version TEXT NON NULL
                )"
            );
            transaction.Execute(@"
                INSERT OR IGNORE INTO schema (subject, version)
                VALUES ('root', 'Zero')"
            );
            var version = transaction.Read(
                "SELECT version FROM schema",
                row => row.String("version")
            ).Single();
            return Enum.Parse<Schema>(version, true);
        }

        private enum Schema
        {
            Zero,
            Initial
        }
    }

    public static class Lookup
    {
        public static ILookup<TKey, TElement> Create<TKey, TElement>(params (TKey Key, TElement Element)[] content)
            => content.ToLookup(item => item.Key, item => item.Element);
    }
}