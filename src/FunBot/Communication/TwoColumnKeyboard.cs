using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Communication
{
    public sealed class TwoColumnKeyboard : Keyboard
    {
        private readonly string[] buttons;

        public TwoColumnKeyboard(params string[] buttons)
        {
            this.buttons = buttons;
        }

        public override IReplyMarkup Markup
        {
            get
            {
                if (buttons.Length == 0)
                    return new ReplyKeyboardMarkup();

                return new ReplyKeyboardMarkup(
                    Rows().Select(row => row.Select(cell => new KeyboardButton(cell)))
                );

                IEnumerable<List<string>> Rows()
                {
                    var row = new List<string>();
                    foreach (var button in buttons)
                    {
                        row.Add(button);
                        if (row.Count < 2)
                            continue;

                        yield return row;
                        row = new List<string>();
                    }

                    if (row.Count > 0)
                        yield return row;
                }
            }
        }
    }
}