using System;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.Threading.Tasks;
using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class StoredConversation : Conversation
    {
        private readonly long chatId;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Conversation conversation;
        private readonly JObject @object;
        private readonly Random random;
        private readonly Talk.Collection talks;

        public StoredConversation(
            long chatId,
            Talk.Collection talks,
            Clock clock,
            SQLiteConnection connection,
            Random random,
            JObject @object)
        {
            this.chatId = chatId;
            this.talks = talks;
            this.clock = clock;
            this.connection = connection;
            this.random = random;
            this.@object = @object;
            conversation = new LazyConversation(Parse);
        }

        public override DateTime AskAt => conversation.AskAt;

        private Conversation Navigate(string propertyName) => new StoredConversation(
            chatId,
            talks,
            clock,
            connection,
            random,
            @object.Get<JObject>(propertyName)
        );

        private Conversation Parse()
        {
            var type = @object.Get<string>("type");
            return type switch
            {
                "greeting" => new Greeting(
                    talks.For(chatId, FullKeyboard),
                    new LazyConversation(() => Selection(5))
                ),
                "selection" => Selection(),
                "serialSelection" => SerialSelection(),
                "feedback" => Feedback(
                    Navigate("from")
                ),
                _ => throw new Exception($"Could not parse {@object}")
            };
        }

        private Keyboard FullKeyboard => new TwoColumnKeyboard(
            "Кино",
            "Сериалы",
            //"Мультфильмы",
            "Книги",
            //"Кладовая",
            "Написать нам"
        );

        private Keyboard SerialKeyboard => new TwoColumnKeyboard(
            "Короткий",
            "Длинный"
        );

        private Keyboard FeedbackKeyboard => new TwoColumnKeyboard(
            "Я передумал"
        );

        private Conversation Selection(int queriesLeft)
        {
            var talk = talks.For(chatId, FullKeyboard);
            var serialSelectionTalk = talks.For(chatId, SerialKeyboard);
            return new Selection(
                clock.Now,
                queriesLeft,
                new LazyConversation(() => Selection(5)),
                new Matching<string, Conversation>(
                    "Написать нам",
                    StringComparer.CurrentCultureIgnoreCase,
                    new WithoutArgument<string, Conversation>(
                        new FeedbackDialogue(
                            talks.For(chatId, FeedbackKeyboard),
                            Feedback(new LazyConversation(() => Selection(queriesLeft)))
                        )
                    ),
                    new Limited<string>(
                        queriesLeft,
                        talk,
                        new LazyConversation(() => Selection(0)),
                        new Matching<string, Conversation>(
                            "Сериалы",
                            StringComparer.CurrentCultureIgnoreCase,
                            new WithoutArgument<string, Conversation>(
                                new SerialSelectionDialogue(serialSelectionTalk, new LazyConversation(() => SerialSelection(queriesLeft)))
                            ),
                            new Commands(
                                ImmutableStack.CreateRange(
                                    new (string Command, Content.Collection Collection)[]
                                    {
                                        ("кино", new Movies(chatId, connection, random, clock)),
                                        ("книги", new Books(chatId, connection, random, clock))
                                    }
                                ),
                                conversation,
                                new LazyConversation(() => Selection(queriesLeft - 1)),
                                new LazyConversation(() => Selection(0)),
                                talk,
                                queriesLeft
                            )
                        )
                    )
                )
            );
        }

        private Conversation SerialSelection(int queriesLeft) => new SerialSelection(
            talks.For(chatId, FullKeyboard),
            new Serials(chatId, SerialDuration.Short, connection, random, clock),
            new Serials(chatId, SerialDuration.Long, connection, random, clock),
            new LazyConversation(() => Selection(queriesLeft)),
            new LazyConversation(() => Selection(Math.Max(0, queriesLeft - 1))),
            queriesLeft
        );

        private Conversation Feedback(Conversation from) => new Feedback(
            talks.For(chatId, FullKeyboard),
            from
        );

        private Conversation Selection()
        {
            var left = @object.Get<int>("queriesLeft");
            return Selection(left);
        }

        private Conversation SerialSelection()
        {
            var left = @object.Get<int>("queriesLeft");
            return SerialSelection(left);
        }

        public override async Task<Conversation> AnswerAsync(string query) =>
            await conversation.AnswerAsync(query);

        public override async Task<Conversation> AskAsync() => await conversation.AskAsync();
        public override JObject Serialize() => @object;
    }
}