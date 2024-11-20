using System;
using Telegram.Bot;
using TelegramBot_Fitz.Bot;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the loan calculator");

            var loanCalculator = new LoanCalculator();
            loanCalculator.Run();

            // Запуск бота
            string botToken = "7215305698:AAHAqk-owYiegTWajlofjb5Ny2EYaI4yM7I"; // Токен бота
            var botService = new BotService(botToken);  // Используем класс BotService, который мы написали
            botService.Start();  // Запуск получения обновлений бота

            Console.WriteLine("Bot is running...");
            Console.ReadLine(); // Ждем завершения работы
        }
    }
}
