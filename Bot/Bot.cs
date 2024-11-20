using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using System.Threading;

namespace TelegramBot_Fitz.Bot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
        }

        public void Start()
        {
            // Используем правильные делегаты для StartReceiving
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        }

        // Метод для обработки обновлений
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            if (message?.Text != null)
            {
                // Заменили SendTextMessageAsync на SendMessage
                await botClient.SendMessage(message.Chat.Id, "Hello, this is your loan calculator bot!");
            }
        }

        // Метод для обработки ошибок
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
