using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using FunBot.Conversation;
using FunBot.Json;
using FunBot.Storage;
using Newtonsoft.Json.Linq;

namespace FunBot
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
        private readonly State.Collection states;
        private readonly Message message;
        private readonly string token;

        public TelegramUpdate(
            Message message,
            SQLiteConnection connection,
            State.Collection states,
            string token)
        {
            this.message = message;
            this.connection = connection;
            this.states = states;
            this.token = token;
        }

        public override string Text => message.Text;
        public override State Subject => new StoringState(this, states.Get(message.ChatId));

        private void Store(State state)
        {
            using var transaction = connection.BeginTransaction();
            transaction.Execute(@"
                REPLACE INTO states (chat_id, content, expires_at)
                VALUES (:chat_id, :content, :expires_at)",
                ("chat_id", message.ChatId),
                ("content", state.Serialize().AsString()),
                ("expires_at", state.ExpiresAt)
            );
            transaction.Execute(@"
                REPLACE INTO offsets (token, value)
                VALUES (:token, :value)",
                ("token", token),
                ("value", message.Id + 1)
            );
            transaction.Commit();
        }

        private sealed class StoringState : State
        {
            private readonly TelegramUpdate update;
            private readonly State state;

            public StoringState(TelegramUpdate update, State state)
            {
                this.update = update;
                this.state = state;
            }

            public override async Task<State> RespondAsync(string query)
            {
                var result = await state.RespondAsync(query);
                update.Store(result);
                return result;
            }

            public override async Task<State> ExpireAsync()
            {
                var result = await state.ExpireAsync();
                update.Store(result);
                return result;
            }

            public override DateTime ExpiresAt => state.ExpiresAt;
            public override JObject Serialize() => state.Serialize();
        }
    }
}