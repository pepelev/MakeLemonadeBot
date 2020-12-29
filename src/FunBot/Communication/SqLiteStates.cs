using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FunBot.Configuration;
using FunBot.Json;
using FunBot.Storage;
using Serilog;

namespace FunBot.Communication
{
    public sealed class SqLiteStates : Conversation.Collection
    {
        private readonly ILogger feedbackLog;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Random random = new Random();
        private readonly Talk.Collection talks;
        private readonly Settings settings;

        public SqLiteStates(
            ILogger feedbackLog,
            SQLiteConnection connection,
            Talk.Collection talks,
            Clock clock,
            Settings settings)
        {
            this.feedbackLog = feedbackLog;
            this.connection = connection;
            this.talks = talks;
            this.clock = clock;
            this.settings = settings;
        }

        public override Conversation Get(long chatId)
        {
            var content = connection.Read(@"
                SELECT content FROM conversations
                WHERE chat_id = :chat_id",
                row => row.String("content"),
                ("chat_id", chatId)
            ).SingleOrDefault();

            return State(chatId, content ?? "{\"type\": \"greeting\"}");
        }

        public override void Update(long chatId, Conversation conversation)
        {
            connection.Execute(@"
                REPLACE INTO conversations (chat_id, content, expires_at)
                VALUES (:chat_id, :content, :expires_at)",
                ("chat_id", chatId),
                ("content", conversation.Serialize().AsString()),
                ("expires_at", conversation.AskAt)
            );
        }

        public override IReadOnlyCollection<(long ChatId, Conversation State)> Questions()
        {
            var expired = connection.Read(@"
                SELECT chat_id, content FROM conversations
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
            var @object = content.AsJsonObject();
            return new StoredConversation(
                chatId,
                feedbackLog.ForContext("ChatId", chatId),
                talks,
                clock,
                connection,
                random,
                settings,
                @object
            );
        }
    }
}