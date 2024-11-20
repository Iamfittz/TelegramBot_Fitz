using Microsoft.Extensions.Configuration;
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
            // Настройка конфигурации для чтения из appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();


            // Получение токена из конфигурации
            string botToken = configuration["BotSettings:BotToken"];

            Console.WriteLine("Bot is running...");
            // Запуск бота
            var botService = new BotService(botToken);  
            botService.Start();  // Запуск получения обновлений бота

            
            Console.ReadLine();
            Console.WriteLine("Welcome to the loan calculator");

           var loanCalculator = new LoanCalculator();
            loanCalculator.Run();

        }
    }
}
