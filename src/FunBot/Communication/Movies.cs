using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Communication
{
    public sealed class Movies : Content.Collection
    {
        private readonly long chatId;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Random random;

        public Movies(long chatId, SQLiteConnection connection, Random random, Clock clock)
        {
            this.chatId = chatId;
            this.connection = connection;
            this.random = random;
            this.clock = clock;
        }

        public override bool Empty
        {
            get
            {
                using var transaction = connection.BeginTransaction();
                return NotShownIds(transaction).Count == 0;
            }
        }

        private IReadOnlyList<string> NotShownIds(SQLiteTransaction transaction) => transaction.Read(@"
            SELECT id FROM movies
            WHERE id NOT IN (
                SELECT movie_id FROM shown_movies
                WHERE chat_id = :chat_id
            )",
            row => row.String("id"),
            ("chat_id", chatId)
        );

        public override Content Pick()
        {
            using var transaction = connection.BeginTransaction();
            var id = PickId(transaction);
            var movie = transaction.Read(@"
                SELECT id, name, original_name, year
                FROM movies
                WHERE id = :id",
                row => new Movie(
                    row.String("id"),
                    row.String("name"),
                    row.MaybeString("original_name"),
                    row.MaybeInt("year")
                ),
                ("id", id)
            ).Single();
            transaction.Commit();
            return new Item(chatId, movie, connection);
        }

        private string PickId(SQLiteTransaction transaction)
        {
            var toBeShownId = transaction.Read(@"
                SELECT movie_id FROM shown_movies
                WHERE state = 'to-be-shown'",
                row => row.String("movie_id")
            ).SingleOrDefault();

            if (toBeShownId != null)
            {
                return toBeShownId;
            }

            var id = random.Pick(NotShownIds(transaction));
            transaction.Execute(@"
                INSERT INTO shown_movies (chat_id, movie_id, state, at)
                VALUES (:chat_id, :movie_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("movie_id", id),
                ("at", clock.Now)
            );
            return id;
        }

        private sealed class Item : Content
        {
            private readonly long chatId;
            private readonly SQLiteConnection connection;
            private readonly Movie movie;

            public Item(long chatId, Movie movie, SQLiteConnection connection)
            {
                this.movie = movie;
                this.connection = connection;
                this.chatId = chatId;
            }

            public override string Print() => movie.Print();

            public override void MarkShown()
            {
                connection.Execute(@"
                    UPDATE shown_movies
                    SET state = 'shown'
                    WHERE (chat_id, movie_id) = (:chat_id, :movie_id)",
                    ("chat_id", chatId),
                    ("movie_id", movie.Id)
                );
            }
        }
    }
}