using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Communication;
using FunBot.Jobs;

namespace FunBot.Tests.Complex
{
    public sealed class Environment
    {
        private readonly Talks talks;
        private readonly SQLiteConnection connection;
        private readonly Conversation.Collection states;
        private string BotToken => "someBotToken";
        private readonly Source source;
        private readonly Job sut;
        private readonly TestClock clock;

        public Environment(
            DateTime start,
            SQLiteConnection connection)
        {
            const int chatId = 42;
            this.connection = connection;
            talks = new Talks(chatId);
            clock = new TestClock(start);
            states = new SqLiteStates(connection, talks, clock);
            source = new Source(this, chatId);
            sut = new Listening(
                states,
                CancellationToken.None,
                source
            );
        }

        public async Task<IEnumerable<string>> FeedAsync(params string[] queries)
        {
            foreach (var query in queries)
            {
                source.Feed(query);
            }
            await sut.RunAsync().ConfigureAwait(false);
            return talks.Extract();
        }

        public async Task<IEnumerable<string>> FeedAsync(int count, string text)
        {
            for (var i = 0; i < count; i++)
            {
                source.Feed(text);
            }

            await sut.RunAsync().ConfigureAwait(false);
            return talks.Extract();
        }

        private sealed class Talks : Talk.Collection
        {
            private ImmutableQueue<string> replies = ImmutableQueue<string>.Empty;
            private readonly long expectedChatId;
            private readonly Talk talk;

            public Talks(long expectedChatId)
            {
                this.expectedChatId = expectedChatId;
                talk = new Talk(this);
            }

            public override Communication.Talk For(long chatId, Keyboard keyboard)
            {
                if (chatId != expectedChatId)
                {
                    throw new InvalidOperationException("Wrong chat");
                }

                return talk;
            }

            public IEnumerable<string> Extract()
            {
                var result = replies;
                replies = ImmutableQueue<string>.Empty;
                return result;
            }

            private sealed class Talk : Communication.Talk
            {
                private readonly Talks talks;

                public Talk(Talks talks)
                {
                    this.talks = talks;
                }

                public override Task SayAsync(string phrase)
                {
                    talks.replies = talks.replies.Enqueue(phrase);
                    return Task.CompletedTask;
                }
            }
        }

        private sealed class Source : UpdateSource
        {
            private readonly Environment context;
            private int messageId = 3;
            private ImmutableQueue<string> queries = ImmutableQueue<string>.Empty;
            private readonly long chatId;

            public Source(Environment context, long chatId)
            {
                this.context = context;
                this.chatId = chatId;
            }

            public override Task<IReadOnlyList<Update>> UpdatesAsync(CancellationToken token)
            {
                var result = queries.Select(
                    (query, index) => new TelegramUpdate(
                        new Message(this, messageId + index, query),
                        context.connection,
                        context.states,
                        context.BotToken
                    )
                ).ToList() as IReadOnlyList<Update>;

                messageId += queries.Count();
                queries = ImmutableQueue<string>.Empty;
                return Task.FromResult(result);
            }

            public void Feed(string query)
            {
                queries = queries.Enqueue(query);
            }

            private sealed class Message : FunBot.Message
            {
                private readonly Source source;

                public Message(Source source, int id, string text)
                {
                    this.source = source;
                    Id = id;
                    Text = text;
                }

                public override int Id { get; }
                public override long ChatId => source.chatId;
                public override string Text { get; }
            }
        }

        public async Task<IEnumerable<string>> WaitAsync(DateTime until)
        {
            clock.Pass(until);
            await sut.RunAsync().ConfigureAwait(false);
            return talks.Extract();
        }
    }
}