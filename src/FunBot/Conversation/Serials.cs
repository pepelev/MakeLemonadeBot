using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Conversation
{
    public sealed class Serials : Content.Collection
    {
        private readonly long chatId;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly SerialDuration duration;
        private readonly Random random;

        public Serials(
            long chatId,
            SerialDuration duration,
            SQLiteConnection connection,
            Random random,
            Clock clock)
        {
            this.chatId = chatId;
            this.connection = connection;
            this.random = random;
            this.clock = clock;
            this.duration = duration;
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
            SELECT id FROM serials
            WHERE id NOT IN (
                SELECT serial_id FROM shown_serials
                WHERE chat_id = :chat_id
            ) AND duration = :duration",
            row => row.String("id"),
            ("chat_id", chatId),
            ("duration", duration.ToString("G"))
        );

        public override Content Pick()
        {
            using var transaction = connection.BeginTransaction();
            var id = PickId(transaction);
            var movie = transaction.Read(@"
                SELECT id, name, original_name, year, duration
                FROM serials
                WHERE id = :id",
                row => new Serial(
                    row.String("id"),
                    row.String("name"),
                    row.String("original_name"),
                    row.Int("year"),
                    Enum.Parse<SerialDuration>(row.String("duration"))
                ),
                ("id", id)
            ).Single();
            transaction.Commit();
            return new Item(chatId, movie, connection);
        }

        private string PickId(SQLiteTransaction transaction)
        {
            var toBeShownId = transaction.Read(@"
                SELECT serial_id FROM shown_serials
                WHERE state = 'to-be-shown'",
                row => row.String("serial_id")
            ).SingleOrDefault();

            if (toBeShownId != null)
            {
                return toBeShownId;
            }

            var id = random.Pick(NotShownIds(transaction));
            transaction.Execute(@"
                INSERT INTO shown_serials (chat_id, serial_id, state, at)
                VALUES (:chat_id, :serial_id, 'to-be-shown', :at)",
                ("chat_id", chatId),
                ("serial_id", id),
                ("at", clock.Now)
            );
            return id;
        }

        private sealed class Item : Content
        {
            private readonly long chatId;
            private readonly SQLiteConnection connection;
            private readonly Serial serial;

            public Item(long chatId, Serial serial, SQLiteConnection connection)
            {
                this.serial = serial;
                this.connection = connection;
                this.chatId = chatId;
            }

            public override string Print() => serial.Print();

            public override void MarkShown()
            {
                connection.Execute(@"
                    UPDATE shown_serials
                    SET state = 'shown'
                    WHERE (chat_id, serial_id) = (:chat_id, :serial_id)",
                    ("chat_id", chatId),
                    ("serial_id", serial.Id)
                );
            }
        }
    }
}