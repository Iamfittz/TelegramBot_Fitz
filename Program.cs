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

        // Method for starting the calculation of a fixed rate
        private static void RunFixedRateCalculator(decimal loanAmount, int loanYears, decimal interestRate)
        {
            Console.WriteLine("Running Fixed Rate Loan Calculator...");
            var loanCalculator = new FixedRateLoanCalculator();
            decimal totalPayment = loanCalculator.CalculateTotalPayment(loanAmount, loanYears, interestRate);
            Console.WriteLine($"Total payment for fixed rate: {totalPayment:F2} USD.");
        }

        // Method for starting the calculation of a flaoting rate
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
