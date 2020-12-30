using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using MakeLemonadeBot.Storage;

namespace MakeLemonadeBot.Communication
{
    public sealed class Storeroom : Content.Collection
    {
        private readonly SQLiteConnection connection;
        private readonly long chatId;
        private readonly Random random;
        private readonly Clock clock;

        public Storeroom(long chatId, SQLiteConnection connection, Random random, Clock clock)
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
            SELECT id FROM storeroom
            WHERE id NOT IN (
                SELECT thing_id FROM shown_things
                WHERE chat_id = :chat_id
            )",
            row => row.String("id"),
            ("chat_id", chatId)
        );

        public override Content Pick()
        {
            using var transaction = connection.BeginTransaction();
            var id = PickId(transaction);
            var thing = connection.Read(@"
                SELECT id, content, description, category
                FROM storeroom
                WHERE id = :id",
                row => new Thing(
                    row.String("id"),
                    row.String("content"),
                    row.MaybeString("description"),
                    row.MaybeString("category")
                ),
                ("id", id)
            ).Single();
            transaction.Commit();
            return new Item(chatId, thing, connection);
        }



        private string PickId(SQLiteTransaction transaction)
        {
            var toBeShownId = transaction.Read(@"
                SELECT thing_id FROM shown_things
                WHERE state = 'to-be-shown'",
                row => row.String("thing_id")
            ).SingleOrDefault();

            if (toBeShownId != null)
            {
                return toBeShownId;
            }

            var id = random.Pick(NotShownIds(transaction));
            transaction.Execute(@"
                INSERT INTO shown_things (chat_id, thing_id, state, at)
                VALUES (:chat_id, :thing_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("thing_id", id),
                ("at", clock.Now)
            );
            return id;
        }

        private sealed class Item : Content
        {
            private readonly Thing thing;
            private readonly long chatId;
            private readonly SQLiteConnection connection;

            public Item(long chatId, Thing thing, SQLiteConnection connection)
            {
                this.thing = thing;
                this.connection = connection;
                this.chatId = chatId;
            }

            public override string Print() => thing.Print();

            public override void MarkShown()
            {
                connection.Execute(@"
                    UPDATE shown_things
                    SET state = 'shown'
                    WHERE (chat_id, thing_id) = (:chat_id, :thing_id)",
                    ("chat_id", chatId),
                    ("thing_id", thing.Id)
                );
            }
        }
    }
}