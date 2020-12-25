using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Json;
using FunBot.Storage;

namespace FunBot.Communication
{
    public sealed class SqLiteStates : Conversation.Collection
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

        public override Conversation Get(long chatId)
        {
            var content = connection.Read(@"
                SELECT content FROM states
                WHERE chat_id = :chat_id",
                row => row.String("content"),
                ("chat_id", chatId)
            ).SingleOrDefault();

            if (content == null)
            {
                return Factory(chatId).Greeting();
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

        public override void Update(long chatId, Conversation conversation)
        {
            connection.Execute(@"
                REPLACE INTO states (chat_id, content, expires_at)
                VALUES (:chat_id, :content, :expires_at)",
                ("chat_id", chatId),
                ("content", conversation.Serialize().AsString()),
                ("expires_at", conversation.AskAt)
            );
        }

        public override IReadOnlyCollection<(long ChatId, Conversation State)> Questions()
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

        private Conversation State(long chatId, string content)
        {
            var factory = Factory(chatId);
            var @object = content.AsJsonObject();
            return new StoredConversation(
                chatId,
                talks,
                clock,
                connection,
                random,
                factory,
                @object
            );
        }
    }
}