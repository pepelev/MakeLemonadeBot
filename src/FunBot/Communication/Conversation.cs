using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public abstract class Conversation
    {
        public abstract DateTime AskAt { get; }
        public abstract Task<Conversation> AnswerAsync(string query);
        public abstract Task<Conversation> AskAsync();
        public abstract JObject Serialize();

        public abstract class Collection
        {
            public abstract Conversation Get(long chatId);
            public abstract void Update(long chatId, Conversation conversation);
            public abstract IReadOnlyCollection<(long ChatId, Conversation State)> Questions();
        }
    }
}