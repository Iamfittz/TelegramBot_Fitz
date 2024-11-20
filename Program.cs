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
            Console.WriteLine("Bot is running...");
            // Запуск бота
            string botToken = "7215305698:AAHAqk-owYiegTWajlofjb5Ny2EYaI4yM7I"; 
            var botService = new BotService(botToken);  
            botService.Start();  // Запуск получения обновлений бота

            
            Console.ReadLine();
            Console.WriteLine("Welcome to the loan calculator");

           var loanCalculator = new LoanCalculator();
            loanCalculator.Run();

        }
    }
}
