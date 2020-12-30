using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using MakeLemonadeBot.Communication;
using MakeLemonadeBot.Json;
using MakeLemonadeBot.Storage;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot
{
    public abstract class Message
    {
        public abstract int Id { get; }
        public abstract long ChatId { get; }
        public abstract string Text { get; }
    }

    public sealed class TelegramMessage : Message
    {
        private readonly Telegram.Bot.Types.Update update;

        public TelegramMessage(Telegram.Bot.Types.Update update)
        {
            this.update = update;
        }

        public override int Id => update.Id;
        public override long ChatId => update.Message.Chat.Id;
        public override string Text => update.Message.Text;
    }

    public sealed class TelegramUpdate : Update
    {
        private readonly SQLiteConnection connection;
        private readonly Conversation.Collection states;
        private readonly Message message;
        private readonly string token;

        public TelegramUpdate(
            Message message,
            SQLiteConnection connection,
            Conversation.Collection states,
            string token)
        {
            this.message = message;
            this.connection = connection;
            this.states = states;
            this.token = token;
        }

        public override string Text => message.Text;
        public override Conversation Subject => new StoringConversation(this, states.Get(message.ChatId));

        private void Store(Conversation conversation)
        {
            using var transaction = connection.BeginTransaction();
            transaction.Execute(@"
                REPLACE INTO conversations (chat_id, content, expires_at)
                VALUES (:chat_id, :content, :expires_at)",
                ("chat_id", message.ChatId),
                ("content", conversation.Serialize().AsString()),
                ("expires_at", conversation.AskAt)
            );
            transaction.Execute(@"
                REPLACE INTO offsets (token, value)
                VALUES (:token, :value)",
                ("token", token),
                ("value", message.Id + 1)
            );
            transaction.Commit();
        }

        private sealed class StoringConversation : Conversation
        {
            private readonly TelegramUpdate update;
            private readonly Conversation conversation;

            public StoringConversation(TelegramUpdate update, Conversation conversation)
            {
                this.update = update;
                this.conversation = conversation;
            }

            public override async Task<Conversation> AnswerAsync(string query)
            {
                var result = await conversation.AnswerAsync(query);
                update.Store(result);
                return result;
            }

            public override async Task<Conversation> AskAsync()
            {
                var result = await conversation.AskAsync();
                update.Store(result);
                return result;
            }

            public override DateTime AskAt => conversation.AskAt;
            public override JObject Serialize() => conversation.Serialize();
        }
    }
}