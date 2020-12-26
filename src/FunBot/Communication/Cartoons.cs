using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Communication
{
    public sealed class Cartoons : Content.Collection
    {
        private readonly SQLiteConnection connection;
        private readonly long chatId;
        private readonly Random random;
        private readonly Clock clock;

        public Cartoons(long chatId, SQLiteConnection connection, Random random, Clock clock)
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
            SELECT id FROM cartoons
            WHERE id NOT IN (
                SELECT cartoon_id FROM shown_cartoons
                WHERE chat_id = :chat_id
            )",
            row => row.String("id"),
            ("chat_id", chatId)
        );

        public override Content Pick()
        {
            using var transaction = connection.BeginTransaction();
            var id = PickId(transaction);
            var cartoon = connection.Read(@"
                SELECT id, name, original_name, year, note
                FROM `cartoons`
                WHERE id = :id",
                row => new Cartoon(
                    row.String("id"),
                    row.String("name"),
                    row.String("original_name"),
                    row.Int("year"),
                    row.MaybeString("note")
                ),
                ("id", id)
            ).Single();
            transaction.Commit();
            return new Item(chatId, cartoon, connection);
        }



        private string PickId(SQLiteTransaction transaction)
        {
            var toBeShownId = transaction.Read(@"
                SELECT cartoon_id FROM shown_cartoons
                WHERE state = 'to-be-shown'",
                row => row.String("cartoon_id")
            ).SingleOrDefault();

            if (toBeShownId != null)
            {
                return toBeShownId;
            }

            var id = random.Pick(NotShownIds(transaction));
            transaction.Execute(@"
                INSERT INTO shown_cartoons (chat_id, cartoon_id, state, at)
                VALUES (:chat_id, :cartoon_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("cartoon_id", id),
                ("at", clock.Now)
            );
            return id;
        }

        private sealed class Item : Content
        {
            private readonly Cartoon cartoon;
            private readonly long chatId;
            private readonly SQLiteConnection connection;

            public Item(long chatId, Cartoon cartoon, SQLiteConnection connection)
            {
                this.cartoon = cartoon;
                this.connection = connection;
                this.chatId = chatId;
            }

            public override string Print() => cartoon.Print();

            public override void MarkShown()
            {
                connection.Execute(@"
                    UPDATE shown_cartoons
                    SET state = 'shown'
                    WHERE (chat_id, cartoon_id) = (:chat_id, :cartoon_id)",
                    ("chat_id", chatId),
                    ("cartoon_id", cartoon.Id)
                );
            }
        }
    }
}