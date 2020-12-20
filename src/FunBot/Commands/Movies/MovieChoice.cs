using System;
using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Commands.Movies
{
    public sealed class MovieChoice : Command<Movie?>
    {
        private readonly SQLiteConnection connection;
        private readonly long chatId;
        private readonly Random random;
        private readonly Clock clock;

        public MovieChoice(
            SQLiteConnection connection,
            long chatId,
            Random random,
            Clock clock)
        {
            this.connection = connection;
            this.chatId = chatId;
            this.random = random;
            this.clock = clock;
        }

        public override Movie? Run()
        {
            using var transaction = connection.BeginTransaction();
            var toBeShown = TryGetPreviousChoice(transaction);
            if (toBeShown != null)
                return toBeShown;

            var notShownIds = transaction.Read(@"
                SELECT id FROM movies
                WHERE id NOT IN (
                    SELECT movie_id FROM shown_movies
                    WHERE chat_id = :chat_id
                )",
                row => row.String("id"),
                ("chat_id", chatId)
            );
            if (notShownIds.Count == 0)
                return null;

            var id = random.Pick(notShownIds);
            transaction.Execute(@"
                INSERT INTO show_movies (chat_id, movie_id, state, at)
                VALUES (:chat_id, :movie_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("movie_id", id),
                ("at", clock.Now)
            );
            var result = transaction.Read(@"
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

            return result;
        }

        private Movie? TryGetPreviousChoice(SQLiteTransaction transaction) => transaction.Read(@"
            SELECT
                movies.id AS id,
                movies.name AS name,
                movies.original_name AS original_name,
                movies.year AS year
            FROM movies
            INNER JOIN shown_movies ON movies.id = shown_movies.movie_id
            WHERE
                shown_movies.chat_id = :chat_id
                AND
                shown_movies.state = 'to-be-shown'",
            row => new Movie(
                row.String("id"),
                row.String("name"),
                row.MaybeString("original_name"),
                row.MaybeInt("year")
            ),
            ("chat_id", chatId)
        ).SingleOrDefault();
    }
}