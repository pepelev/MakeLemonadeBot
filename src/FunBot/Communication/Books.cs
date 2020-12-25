using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Communication
{
    public sealed class Books : Content.Collection
    {
        private readonly SQLiteConnection connection;
        private readonly long chatId;
        private readonly Random random;
        private readonly Clock clock;

        public Books(long chatId, SQLiteConnection connection, Random random, Clock clock)
        {
            this.connection = connection;
            this.chatId = chatId;
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
            SELECT id FROM books
            WHERE id NOT IN (
                SELECT book_id FROM shown_books
                WHERE chat_id = :chat_id
            )",
            row => row.String("id"),
            ("chat_id", chatId)
        );

        public override Content Pick()
        {
            using var transaction = connection.BeginTransaction();
            var id = PickId(transaction);
            var book = connection.Read(@"
                SELECT id, name, author
                FROM books
                WHERE id = :id",
                row => new Book(
                    row.String("id"),
                    row.String("name"),
                    row.String("author")
                ),
                ("id", id)
            ).Single();
            transaction.Commit();
            return new Item(chatId, book, connection);
        }



        private string PickId(SQLiteTransaction transaction)
        {
            var toBeShownId = transaction.Read(@"
                SELECT book_id FROM shown_books
                WHERE state = 'to-be-shown'",
                row => row.String("book_id")
            ).SingleOrDefault();

            if (toBeShownId != null)
            {
                return toBeShownId;
            }

            var id = random.Pick(NotShownIds(transaction));
            transaction.Execute(@"
                INSERT INTO shown_books (chat_id, book_id, state, at)
                VALUES (:chat_id, :book_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("book_id", id),
                ("at", clock.Now)
            );
            return id;
        }

        private sealed class Item : Content
        {
            private readonly Book book;
            private readonly long chatId;
            private readonly SQLiteConnection connection;

            public Item(long chatId, Book book, SQLiteConnection connection)
            {
                this.book = book;
                this.connection = connection;
                this.chatId = chatId;
            }

            public override string Print() => book.Print();

            public override void MarkShown()
            {
                connection.Execute(@"
                    UPDATE shown_books
                    SET state = 'shown'
                    WHERE (chat_id, book_id) = (:chat_id, :book_id)",
                    ("chat_id", chatId),
                    ("book_id", book.Id)
                );
            }
        }
    }
}