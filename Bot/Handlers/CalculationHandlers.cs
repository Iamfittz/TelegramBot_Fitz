using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
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
                state.YearlyRates
            );
            var resultMessage = _fixedCalculator.FormatCalculationResult(
                calculationResult,
                state.YearlyRates
            );
            await _botClient.SendMessage(chatId, resultMessage);
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
