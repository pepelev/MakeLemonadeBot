using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Conversation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FunBot
{
    public sealed class EntryPoint
    {
        static async Task Main()
        {
            var json = await File.ReadAllTextAsync("telegram-credentials.json", Encoding.UTF8);
            var telegramCredentials = JsonConvert.DeserializeObject<TelegramCredentials>(json);

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                Secrets(),
                new[] { SheetsService.Scope.SpreadsheetsReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)
            );

            using var service = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FunBot"
                }
            );


            var client = new TelegramBotClient(telegramCredentials.Token);
            var index = new Dictionary<long, State>();
            var offset = 0;
            while (true)
            {
                var updates = await client.GetUpdatesAsync(
                    allowedUpdates: new[] {UpdateType.Message},
                    offset: offset,
                    timeout: 45
                );

                foreach (var update in updates)
                {
                    var message = update.Message;
                    var chatId = message.Chat.Id;
                    var state = Chat(chatId);
                    var newState = await state.RespondAsync(message.Text);
                    index[chatId] = newState;
                    offset = Math.Max(offset, update.Id + 1);
                }
            }

            State Chat(long chatId) => index.TryGetValue(chatId, out var state)
                ? state
                : new Hello(client, chatId);
        }

        private static ClientSecrets Secrets()
        {
            using var file = File.OpenRead("google-credentials.json");
            return GoogleClientSecrets.Load(file).Secrets;
        }
    }
}