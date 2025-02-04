﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz.Bot
{
    public class CalculationHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly FixedRateLoanCalculator _fixedCalculator;
        private readonly FloatingRateLoanCalculator _floatingCalculator;
        private readonly OISCalculator _oisCalculator;

        public CalculationHandlers(
            ITelegramBotClient botClient,
            FixedRateLoanCalculator fixedCalculator,
            FloatingRateLoanCalculator floatingCalculator,
            OISCalculator oisCalculator)
        {
            _botClient = botClient;
            _fixedCalculator = fixedCalculator;
            _floatingCalculator = floatingCalculator;
            _oisCalculator = oisCalculator;
        }

        public async Task HandleFixedRateCalculation(long chatId, UserState state)
        {
            
            var calculationResult = _fixedCalculator.CalculateLoan(
                state.LoanAmount,
                state.YearlyRates,
                state.InterestCalculationType
            );

            // Формируем сообщение
            StringBuilder message = new StringBuilder();

            // Добавляем заголовок
            message.AppendLine($"📊 {state.InterestCalculationType} Interest Calculation\n");
            message.AppendLine($"Initial amount: {state.LoanAmount:F2} USD\n");

            // Добавляем информацию по каждому году
            foreach (var yearCalc in calculationResult.YearlyCalculations)
            {
                message.AppendLine($"Year {yearCalc.Year}:");
                message.AppendLine($"Rate: {yearCalc.Rate}%");
                message.AppendLine($"Interest: {yearCalc.Interest:F2} USD");

                if (state.InterestCalculationType == InterestCalculationType.Compound)
                {
                    message.AppendLine($"Accumulated amount: {yearCalc.AccumulatedAmount:F2} USD");
                }
                message.AppendLine();
            }

            // Добавляем итоги
            message.AppendLine($"Total Interest: {calculationResult.TotalInterest:F2} USD");
            message.AppendLine($"Total Payment: {calculationResult.TotalPayment:F2} USD");

            // Добавляем кнопки для следующих действий
            var afterCalculation = new InlineKeyboardMarkup(new[]
            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("📊 New Calculation", "NewCalculation"),
            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "MainMenu")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("❓ Help", "Help")
        }
    });

            // Отправляем сообщение
            await _botClient.SendMessage(
                chatId,
                message.ToString() + "\n\nWhat would you like to do next, anon?",
                replyMarkup: afterCalculation
            );

            state.Reset();
        }

        public async Task HandleFloatingRateCalculation(long chatId, UserState state)
        {
            _floatingCalculator.LoanAmount = state.LoanAmount;
            _floatingCalculator.TotalYears = state.LoanYears;
            _floatingCalculator.FirstRate = state.FirstRate;
            _floatingCalculator.SecondRate = state.SecondRate;

            decimal totalInterest = _floatingCalculator.CalculateTotalInterest();
            decimal totalPayment = _floatingCalculator.CalculateTotalPayment();

            var resultMessage =
                $"Loan calculation with floating rate:\n" +
                $"First 6 months rate: {state.FirstRate}%\n" +
                $"Second 6 months rate: {state.SecondRate}%\n" +
                $"First period interest: {_floatingCalculator.LoanAmount * (state.FirstRate / 100) * (6 / 12m):F2} USD\n" +
                $"Second period interest: {_floatingCalculator.LoanAmount * (state.SecondRate / 100) * (6 / 12m):F2} USD\n" +
                $"Total interest: {totalInterest:F2} USD\n" +
                $"Total payment: {totalPayment:F2} USD";

            await _botClient.SendMessage(chatId, resultMessage);
            state.Reset();
        }

        public async Task HandleOISCalculation(long chatId, UserState state)
        {
            _oisCalculator.NotionalAmount = state.LoanAmount;
            _oisCalculator.Days = state.Days;
            _oisCalculator.OvernightRate = state.FirstRate;

            var calculationResult = _oisCalculator.CalculateOIS();
            var resultMessage = _oisCalculator.FormatCalculationResult(calculationResult);
            await _botClient.SendMessage(chatId, resultMessage);
            state.Reset();
        }
    }
}
