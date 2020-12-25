using System;
using System.Collections.Immutable;
using System.Data.SQLite;

namespace FunBot.Communication
{
    public sealed class Factory : Conversation.Factory
    {
        private readonly long chatId;
        private readonly Talk.Collection talks;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Random random;

        public Factory(
            long chatId,
            Talk.Collection talks,
            SQLiteConnection connection,
            Random random,
            Clock clock)
        {
            this.chatId = chatId;
            this.talks = talks;
            this.connection = connection;
            this.random = random;
            this.clock = clock;
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

        public override Conversation Welcome() => new Welcome(
            talks.For(chatId, FullKeyboard),
            this
        );

        public override Conversation Selection(int queriesLeft)
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
                        this,
                        new Matching<string, Conversation>(
                            "Сериалы",
                            StringComparer.CurrentCultureIgnoreCase,
                            new WithoutArgument<string, Conversation>(
                                new SerialSelectionDialogue(this, serialSelectionTalk, queriesLeft)
                            ),
                            new Commands(
                                ImmutableStack.CreateRange(
                                    new (string Command, Content.Collection Collection)[]
                                    {
                                        ("кино", new Movies(chatId, connection, random, clock)),
                                        ("книги", new Books(chatId, connection, random, clock))
                                    }
                                ),
                                this,
                                talk,
                                queriesLeft
                            )
                        )
                    )
                )
            );
        }

        public override Conversation SerialSelection(int queriesLeft)
        {
            return new SerialSelection(
                talks.For(chatId, FullKeyboard),
                new Serials(chatId, SerialDuration.Short, connection, random, clock),
                new Serials(chatId, SerialDuration.Long, connection, random, clock),
                new LazyConversation(() => Selection(queriesLeft)),
                new LazyConversation(() => Selection(Math.Max(0, queriesLeft - 1))),
                queriesLeft
            );
        }

        public override Conversation Feedback(Conversation from) => new Feedback(
            talks.For(chatId, FullKeyboard),
            @from
        );
    }
}