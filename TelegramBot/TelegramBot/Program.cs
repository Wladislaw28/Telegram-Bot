using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient Bot;
        static ApiAi apiAi;
        static void Main(string[] args)
        {
            //c52371d371a2455e8e894b0ac3f5b80f
            Bot = new TelegramBotClient("586806558:AAHDsqjo9CRCvusntiPMH_sdgN9Vfg-BYIA");
            AIConfiguration config = new AIConfiguration("c52371d371a2455e8e894b0ac3f5b80f", SupportedLanguage.Russian);
            apiAi = new ApiAi(config);

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} Нажал кнопку {buttonText}");

            if(buttonText == "Картинка")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.sunhome.ru/i/wallpapers/200/planeta-zemlya-kartinka.960x540.jpg");
            }
            else if(buttonText == "Видео")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.youtube.com/watch?v=sZgXUK5L3Ss");
            }

            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Вы нажали кнопку {buttonText}");

        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.TextMessage)
                return;


            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"{name} отправил сообщение:  '{message.Text}'");

            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start - запуск БОТА
/inline - вывод меню
/keyboard - вывод клавиатуры";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;
                case "/inline":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("VK","https://vk.com/id68799893"),
                            InlineKeyboardButton.WithUrl("Telegram","https://t.me/Mihasev")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Картинка"),
                            InlineKeyboardButton.WithCallbackData("Видео"),
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню", 
                        replyMarkup: inlineKeyboard);
                    break;
                case "/keyboard":
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("Привет"),
                            new KeyboardButton("Как твои дела?")
                        },
                        new[]
                        {
                            new KeyboardButton("Контакт") {RequestContact = true },
                            new KeyboardButton("Геолокация") {RequestLocation = true}
                        }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Сообщение",
                        replyMarkup: replyKeyboard);
                    break;
                default:
                    var response = apiAi.TextRequest(message.Text);
                    string answer = response.Result.Fulfillment.Speech;
                    if (answer == "")
                        answer = "Прости, я не понимаю";
                    await Bot.SendTextMessageAsync(message.From.Id, answer);
                    break;
            }
        }
    }
}
