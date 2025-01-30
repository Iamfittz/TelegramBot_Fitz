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
            // Setting up configuration to read from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string botToken = configuration["BotSettings:BotToken"];

            Console.WriteLine("Bot is running...");

            var botService = new BotService(botToken);
            botService.Start();  

            Console.ReadLine();
        }
    }
}
