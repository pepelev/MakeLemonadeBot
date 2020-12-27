using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Communication;
using FunBot.Configuration;
using FunBot.Jobs;
using FunBot.Sheets;
using FunBot.Storage;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Telegram;
using Telegram.Bot;
using Parallel = FunBot.Jobs.Parallel;

namespace FunBot
{
    public sealed class EntryPoint
    {
        private static async Task Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Console.CancelKeyPress += (sender, args) =>
            {
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    cancellationTokenSource.Cancel();
                    args.Cancel = true;
                }
            };

            var text = await File.ReadAllTextAsync("configuration.json", Encoding.UTF8, token);
            var json = JObject.Parse(text);
            var settings = new JsonSettings(json);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                Secrets(),
                new[] {SheetsService.Scope.SpreadsheetsReadonly},
                "user",
                token,
                new FileDataStore("token.json", true)
            );

            using var sheets = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FunBot"
                }
            );


            var configuration = new LoggerConfiguration();
            const string template = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message} {Properties}{NewLine}{Exception}";
            using var log = configuration
                .WriteTo.File("FunBot.log", outputTemplate: template)
                .WriteTo.Console(outputTemplate: template)
                .WriteTo.Telegram(
                    settings.Telegram.Log.Token,
                    settings.Telegram.Log.ChatId,
                    Render,
                    LogEventLevel.Warning
                )
                .Enrich.FromLogContext()
                .CreateLogger();

            using var feedbackLog = new LoggerConfiguration()
                .WriteTo.Telegram(
                    settings.Telegram.Feedback.Token,
                    settings.Telegram.Feedback.ChatId,
                    Render
                )
                .WriteTo.Logger(log)
                .CreateLogger();

            using var connection = new SqLiteConnectionFactory("Data Source=s.db;Version=3;New=True;").Create();

            var telegramToken = settings.Telegram.Listening.Token;
            var client = new TelegramBotClient(telegramToken);
            var talks = new TelegramTalks(client);
            var clock = new Utc();
            Conversation.Collection states = new SqLiteStates(feedbackLog, connection, talks, clock, settings.Users);
            Offset offset = new SqLiteOffset(telegramToken, connection);

            using var @lock = new SemaphoreSlim(1, 1);
            var listening = new Cycle(
                new Catching(
                    new Locking(
                        token,
                        @lock,
                        new Logging(
                            nameof(Listening),
                            log,
                            clock,
                            new Listening(
                                states,
                                token,
                                new Telegram2(
                                    telegramToken,
                                    client,
                                    connection,
                                    offset,
                                    states
                                )
                            )
                        )
                    )
                ),
                token
            );
            var sources = settings.Sources;
            var updates = new Cycle(
                new Catching(
                    new Logging(
                        "Scheduled updates",
                        log,
                        clock,
                        new Scheduled(
                            clock.Now.Date,
                            sources.UpdatePeriod,
                            token,
                            clock,
                            new Sequential(
                                new UpdateJobs(
                                    token,
                                    @lock,
                                    log,
                                    clock,
                                    connection,
                                    new GoogleSheet.Collection(
                                        sheets,
                                        sources
                                    )
                                ).ToArray()
                            )
                        )
                    )
                ),
                token
            );
            var job = new Parallel(listening, updates);
            await job.RunAsync();
            log.Information("Finishing");
        }

        private static ClientSecrets Secrets()
        {
            using var file = File.OpenRead("google-credentials.json");
            return GoogleClientSecrets.Load(file).Secrets;
        }

        private static Serilog.Sinks.Telegram.TelegramMessage Render(LogEvent @event)
        {
            var header = $"{@event.Level}: {@event.MessageTemplate.Text}";
            var properties = string.Join(
                "\n",
                @event.Properties.Select(
                    pair =>
                    {
                        var value = pair.Value.ToString(null, CultureInfo.InvariantCulture);
                        return $"{pair.Key} = {value}";
                    }
                )
            );
            var exception = @event.Exception;
            if (exception != null)
            {
                var exceptionText = string.Join(
                    "\n",
                    exception.Message,
                    exception.StackTrace
                );

                return new Serilog.Sinks.Telegram.TelegramMessage(
                    string.Join("\n\n", header, properties, exceptionText)
                );
            }

            return new Serilog.Sinks.Telegram.TelegramMessage(
                string.Join("\n\n", header, properties)
            );
        }
    }
}