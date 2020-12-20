using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Conversation
{
    public abstract class State
    {
        public abstract Task<State> RespondAsync(string query);
        public abstract Task<State> ExpireAsync();
        public abstract DateTime ExpiresAt { get; }
        public abstract JObject Serialize();

        public abstract class Factory
        {
            public abstract State Welcome();
            public abstract State Selection(int queriesLeft);
            public abstract State SerialSelection(int queriesLeft);
            public abstract State Feedback(State from);
        }

        public abstract class Collection
        {
            public abstract State Get(long chatId);
            public abstract void Update(long chatId, State state);
            public abstract IReadOnlyCollection<(long ChatId, State State)> Expired();
        }
    }
}