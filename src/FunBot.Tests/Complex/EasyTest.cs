using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using FunBot.Jobs;
using FunBot.Sheets;
using FunBot.Storage;
using FunBot.Updates;
using NUnit.Framework;
using Serilog.Core;

namespace FunBot.Tests.Complex
{
    public class EasyTest
    {
        private Environment context;

        [SetUp]
        public async Task SetUpAsync()
        {
            var factory = new SqLiteConnectionFactory("Data Source=:memory:;Version=3;New=True;");
            var connection = factory.Create();
            var updates = new Job[]
            {
                new BooksUpdate(
                    connection,
                    Logger.None,
                    new ConstSheet(
                        new Row("Идентификатор", "Название", "Автор"),
                        new Row("12", "Стоя под радугой", "Фэнни Флэгг"),
                        new Row("13", "Дживс и Вустер", "П. Г. Вудхауз")
                    ),
                    CancellationToken.None
                ),
                new MoviesUpdate(
                    connection,
                    Logger.None,
                    new ConstSheet(
                        new Row("Идентификатор", "Название", "Оригинальное название", "Год"),
                        new Row("1", "Красотка", "Pretty Woman, 1990"),
                        new Row("2", "Блондинка в законе", "Legally Blonde", "2001"),
                        new Row("3", "Мой учитель-осьминог", "My Octopus Teacher", "2020"),
                        new Row("4", "Выход через сувенирную лавку", "Exit Through the Gift Shop", "2010"),
                        new Row("5", "Огни большого города", "City Lights", "1931"),
                        new Row("6", "Семь психопатов", "Seven Psychopaths", "2012")
                    ),
                    CancellationToken.None
                ),
                new SerialsUpdate(
                    connection,
                    Logger.None,
                    new ConstSheet(
                        new Row("Идентификатор", "Название", "Оригинальное название", "Год", "Продолжительность"),
                        new Row("1", "Тед Лассо", "Ted Lasso", "2020", "короткий"),
                        new Row("7", "Шиттс Крик", "Schitt's Creek", "2015", "длинный")
                    ),
                    CancellationToken.None
                ),
            };

            foreach (var update in updates)
            {
                await update.RunAsync();
            }

            context = new Environment(25.November(2020).AsUtc(), connection);
        }

        [Test]
        public async Task Say_Hello()
        {
            var replies = await context.FeedAsync("/hello");

            replies.Should().BeEquivalentTo("Привет, это отличный бот");
        }

        [Test]
        public async Task Offer_Book()
        {
            await context.FeedAsync("/hello");

            var replies = await context.FeedAsync("Книги");

            replies.Single().Should().BeOneOf(
                "Фэнни Флэгг - Стоя под радугой",
                "П. Г. Вудхауз - Дживс и Вустер"
            );
        }

        [Test]
        public async Task Offer_Two_Books()
        {
            await context.FeedAsync("/hello");

            var replies1 = await context.FeedAsync("Книги");
            var replies2 = await context.FeedAsync("Книги");

            replies1.Concat(replies2).Should().BeEquivalentTo(
                "Фэнни Флэгг - Стоя под радугой",
                "П. Г. Вудхауз - Дживс и Вустер"
            );
        }

        [Test]
        public async Task Out_Of_Books()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(2, "Книги");

            var replies = await context.FeedAsync("Книги");

            replies.Should().BeEquivalentTo("В этой категории больше ничего не осталось");
        }

        [Test]
        public async Task Out_Of_Daily_Norm()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(5, "Кино");

            var replies = await context.FeedAsync("Кино");

            replies.Should().BeEquivalentTo("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Refresh_Limit_On_New_Day()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(5, "Кино");
            await context.WaitAsync(26.November(2020).At(01, 30).AsUtc());

            var replies = await context.FeedAsync("Кино");

            replies.Single().Should().NotBe("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Refresh_Limit_On_New_Day_Even_If_Query_Comes_At_Day_Bound()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(5, "Кино");

            var replies = await context.WaitAsync(26.November(2020).At(01, 30).AsUtc(), "кино");

            replies.Single().Should().NotBe("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Offer_Short_Serial()
        {
            await context.FeedAsync("/hello");

            var replies = await context.FeedAsync("Сериалы", "Короткий");

            replies.Should().Equal(
                "Какой, длинный или короткий?",
                "Тед Лассо (Ted Lasso), 2020"
            );
        }

        [Test]
        public async Task Offer_Long_Serial()
        {
            await context.FeedAsync("/hello");

            var replies = await context.FeedAsync("Сериалы", "Длинный");

            replies.Should().Equal(
                "Какой, длинный или короткий?",
                "Шиттс Крик (Schitt's Creek), 2015"
            );
        }

        [Test]
        public async Task Unknown_Serial()
        {
            await context.FeedAsync("/hello");

            var replies = await context.FeedAsync("Сериалы", "Назад");

            replies.Should().Equal(
                "Какой, длинный или короткий?",
                "Я не понял тебя"
            );
        }

        [Test]
        public async Task SerialSelection_Spend_Query()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(3, "Кино");

            await context.FeedAsync("Сериалы", "Короткий");

            var lastPositiveReply = await context.FeedAsync("Кино");
            var negativeReply = await context.FeedAsync("Кино");

            lastPositiveReply.Single().Should().NotBe("На сегодня это все, приходи завтра");
            negativeReply.Should().BeEquivalentTo("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Failed_Serial_Selection_Not_Spends_Queries()
        {
            await context.FeedAsync("/hello");

            for (var i = 0; i < 5; i++)
            {
                await context.FeedAsync("Сериалы", "Короткий");
            }

            var replies = await context.FeedAsync("Кино");
            replies.Single().Should().NotBe("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Give_Feedback()
        {
            await context.FeedAsync("/hello");

            var replies = await context.FeedAsync("Написать нам", "Мне нравится этот бот");

            replies.Should().BeEquivalentTo(
                "Расскажи мне, что ты обо мне думаешь",
                "Спасибо!"
            );
        }

        [Test]
        public async Task Feedback_First()
        {
            await context.FeedAsync("/hello", "Написать нам", "ОС");

            var replies = await context.FeedAsync("Кино");

            replies.Single().Should().NotBe("На сегодня это все, приходи завтра");
        }

        [Test]
        public async Task Misunderstanding_On_Unknown_Query_When_There_Is_No_Queries_Left()
        {
            await context.FeedAsync("/hello");
            await context.FeedAsync(5, "кино");

            var replies = await context.FeedAsync("123");

            replies.Should().BeEquivalentTo("Я не понял тебя");
        }
    }
}