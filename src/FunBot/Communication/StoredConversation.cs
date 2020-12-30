using System;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.Threading.Tasks;
using MakeLemonadeBot.Configuration;
using MakeLemonadeBot.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MakeLemonadeBot.Communication
{
    public sealed class StoredConversation : Conversation
    {
        private readonly long chatId;
        private readonly ILogger feedbackLog;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Conversation conversation;
        private readonly JObject @object;
        private readonly Random random;
        private readonly Talk.Collection talks;
        private readonly Settings settings;

        public StoredConversation(
            long chatId,
            ILogger feedbackLog,
            Talk.Collection talks,
            Clock clock,
            SQLiteConnection connection,
            Random random,
            Settings settings,
            JObject @object)
        {
            this.chatId = chatId;
            this.talks = talks;
            this.clock = clock;
            this.connection = connection;
            this.random = random;
            this.@object = @object;
            this.settings = settings;
            this.feedbackLog = feedbackLog;
            conversation = new LazyConversation(Parse);
        }

        public override DateTime AskAt => conversation.AskAt;

        private Conversation Navigate(string propertyName) => new StoredConversation(
            chatId,
            feedbackLog,
            talks,
            clock,
            connection,
            random,
            settings,
            @object.Get<JObject>(propertyName)
        );

        private Conversation Parse()
        {
            var type = @object.Get<string>("type");
            return type switch
            {
                "greeting" => new Greeting(
                    talks.For(chatId, FullKeyboard),
                    settings.Phrases,
                    new LazyConversation(() => new ActiveFeedback.Bootstrap(FullSelection()))
                ),
                "fullSelection" => FullSelection(),
                "selection" => Selection(),
                "serialSelection" => SerialSelection(
                    Navigate("back"),
                    Navigate("next")
                ),
                "feedback" => Feedback(
                    Navigate("from")
                ),
                "activeFeedbackBootstrap" => Navigate("conversation"),
                _ => throw new Exception($"Could not parse {@object}")
            };
        }

        private Keyboard FullKeyboard => new TwoColumnKeyboard(
            "Кино",
            "Сериалы",
            "Мультфильмы",
            "Книги",
            "Кладовая",
            "Написать нам"
        );

        private Keyboard SerialKeyboard => new TwoColumnKeyboard(
            "Короткий",
            "Длинный"
        );

        private Keyboard FeedbackKeyboard => new TwoColumnKeyboard(
            "Я передумал"
        );

        private Conversation FullSelection() => new FullSelection(
            Interaction(settings.Users.Get(chatId).DailyBudget)
        );

        private Conversation Selection(int queriesLeft, DateTime timestamp) => new Selection(
            timestamp,
            queriesLeft,
            new LazyConversation(FullSelection),
            Interaction(queriesLeft),
            settings.Users.Get(chatId)
        );

        private Matching<string, Conversation> Interaction(int queriesLeft)
        {
            var talk = talks.For(chatId, FullKeyboard);
            var serialSelectionTalk = talks.For(chatId, SerialKeyboard);
            var timestamp = clock.Now;
            return new Matching<string, Conversation>(
                "Написать нам",
                StringComparer.CurrentCultureIgnoreCase,
                new WithoutArgument<string, Conversation>(
                    new FeedbackDialogue(
                        talks.For(chatId, FeedbackKeyboard),
                        new LazyConversation(
                            () => Feedback(Selection(queriesLeft, timestamp))
                        )
                    )
                ),
                new Matching<string, Conversation>(
                    "Сериалы",
                    StringComparer.CurrentCultureIgnoreCase,
                    new Limited<string>(
                        queriesLeft,
                        talk,
                        new LazyConversation(() => Selection(0, timestamp)),
                        new WithoutArgument<string, Conversation>(
                            new SerialSelectionDialogue(
                                serialSelectionTalk,
                                new LazyConversation(
                                    () => SerialSelection(
                                        new LazyConversation(() => Selection(queriesLeft, timestamp)),
                                        new LazyConversation(() => Selection(Math.Max(0, queriesLeft - 1), timestamp))
                                    )
                                )
                            )
                        )
                    ),
                    new Commands(
                        ImmutableStack.CreateRange(
                            new (string Command, Content.Collection Collection)[]
                            {
                                ("кино", new Movies(chatId, connection, random, clock)),
                                ("книги", new Books(chatId, connection, random, clock)),
                                ("мультфильмы", new Cartoons(chatId, connection, random, clock)),
                                ("кладовая", new Storeroom(chatId, connection, random, clock))
                            }
                        ),
                        this,
                        new LazyConversation(() => Selection(queriesLeft - 1, timestamp)),
                        new LazyConversation(() => Selection(0, timestamp)),
                        talk,
                        queriesLeft
                    )
                )
            );
        }

        private Conversation SerialSelection(Conversation back, Conversation next) => new SerialSelection(
            talks.For(chatId, FullKeyboard),
            new Serials(chatId, SerialDuration.Short, connection, random, clock),
            new Serials(chatId, SerialDuration.Long, connection, random, clock),
            back,
            next
        );

        private Conversation Feedback(Conversation from) => new Feedback(
            feedbackLog,
            talks.For(chatId, FullKeyboard),
            from,
            "я передумал"
        );

        private Conversation Selection()
        {
            var left = @object.Get<int>("queriesLeft");
            var timestamp = @object.Get<DateTime>("timestamp");
            return Selection(left, timestamp);
        }

        public override async Task<Conversation> AnswerAsync(string query) =>
            await conversation.AnswerAsync(query);

        public override async Task<Conversation> AskAsync() => await conversation.AskAsync();
        public override JObject Serialize() => @object;
    }
}