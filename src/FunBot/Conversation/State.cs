using System;
using System.Threading.Tasks;

namespace FunBot.Conversation
{
    public abstract class State
    {
        public abstract Task<State> RespondAsync(string query);
        public abstract Task<State> ExpireAsync();
        public abstract DateTime ExpiresAt { get; }
        public abstract byte[] Serialize();
    }
}