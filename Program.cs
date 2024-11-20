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
        }

        // Метод для запуска расчета фиксированной ставки
        private static void RunFixedRateCalculator(decimal loanAmount, int loanYears, decimal interestRate)
        {
            Console.WriteLine("Running Fixed Rate Loan Calculator...");
            var loanCalculator = new FixedRateLoanCalculator();
            decimal totalPayment = loanCalculator.CalculateTotalPayment(loanAmount, loanYears, interestRate);
            Console.WriteLine($"Total payment for fixed rate: {totalPayment:F2} USD.");
        }

        // Метод для запуска расчета плавающей ставки
        private static void RunFloatingRateCalculator(decimal loanAmount, decimal firstRate, decimal secondRate)
        {
            Console.WriteLine("Running Floating Rate Loan Calculator...");
            var floatingRateCalculator = new FloatingRateLoanCalculator
            {
                LoanAmount = loanAmount,
                FirstRate = firstRate,
                SecondRate = secondRate
            };
            decimal totalPayment = floatingRateCalculator.CalculateTotalPayment();
            Console.WriteLine($"Total payment for floating rate: {totalPayment:F2} USD.");
        }
    }
}
