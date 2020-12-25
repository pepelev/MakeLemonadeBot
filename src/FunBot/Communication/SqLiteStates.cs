using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Json;
using FunBot.Storage;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class SqLiteStates : State.Collection
    {
        private readonly SQLiteConnection connection;
        private readonly Talk.Collection talks;
        private readonly Clock clock;
        private readonly Random random = new Random();

        public SqLiteStates(SQLiteConnection connection, Talk.Collection talks, Clock clock)
        {
            this.connection = connection;
            this.talks = talks;
            this.clock = clock;
        }

        public override State Get(long chatId)
        {
            var content = connection.Read(@"
                SELECT content FROM states
                WHERE chat_id = :chat_id",
                row => row.String("content"),
                ("chat_id", chatId)
            ).SingleOrDefault();

            if (content == null)
            {
                return Factory(chatId).Welcome();
            }

            return State(chatId, content);
        }

        private Factory Factory(long chatId) => new Factory(
            chatId,
            talks,
            connection,
            random,
            clock
        );

        public override void Update(long chatId, State state)
        {
            connection.Execute(@"
                REPLACE INTO states (chat_id, content, expires_at)
                VALUES (:chat_id, :content, :expires_at)",
                ("chat_id", chatId),
                ("content", state.Serialize().AsString()),
                ("expires_at", state.ExpiresAt)
            );
        }

        public override IReadOnlyCollection<(long ChatId, State State)> Expired()
        {
            var expired = connection.Read(@"
                SELECT chat_id, content FROM states
                WHERE expires_at <= :now
                ORDER BY expires_at ASC",
                row => (ChatId: row.Long("chat_id"), Content: row.String("content")),
                ("now", clock.Now)
            );
            return expired.Select(
                state =>
                {
                    var (chatId, content) = state;
                    return (chatId, State(chatId, content));
                }
            ).ToList();
        }

        private State State(long chatId, string content)
        {
            var factory = Factory(chatId);
            var @object = content.AsJsonObject();
            return Parse(@object);

            State Parse(JObject json)
            {
                var type = json.Get<string>("type");
                return type switch
                {
                    "welcome" => factory.Welcome(),
                    "selection" => Selection(json),
                    "serialSelection" => SerialSelection(json),
                    "feedback" => factory.Feedback(
                        Parse(json.Get<JObject>("from"))
                    )
                };
            }

            State Selection(JObject json)
            {
                var left = json.Get<int>("queriesLeft");
                return factory.Selection(left);
            }

            State SerialSelection(JObject json)
            {
                var left = json.Get<int>("queriesLeft");
                return factory.SerialSelection(left);
            }
        }
    }
}