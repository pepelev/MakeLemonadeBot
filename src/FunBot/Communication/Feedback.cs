using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FunBot.Communication
{
    public sealed class Feedback : Conversation
    {
        private readonly ILogger log;
        private readonly Talk talk;
        private readonly Conversation back;
        private readonly string stopPhrase;

        public Feedback(ILogger log, Talk talk, Conversation back, string stopPhrase)
        {
            this.log = log;
            this.talk = talk;
            this.back = back;
            this.stopPhrase = stopPhrase;
        }

        public override async Task<Conversation> AnswerAsync(string query)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals(query, stopPhrase))
            {
                await talk.SayAsync("Ничего, напиши когда надумаешь");
                return back;
            }

            log.Information("Feedback: {Text}", query);
            await talk.SayAsync("Спасибо!");
            return back;
        }

        public override Task<Conversation> AskAsync() => Task.FromResult<Conversation>(this);
        public override DateTime AskAt => Ask.Never;

        public override JObject Serialize() => new JObject(
            new JProperty("type", "feedback"),
            new JProperty("from", back.Serialize()),
            new JProperty("stopPhrase", stopPhrase)
        );
    }
}