using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBot_Fitz.Bot
{
    public class InputHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CalculationHandlers _calculationHandlers;

        public InputHandlers(ITelegramBotClient botClient, CalculationHandlers calculationHandlers)
        {
            _botClient = botClient;
            _calculationHandlers = calculationHandlers;
        }

        public async Task HandleAmountInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal amount) && amount > 0)
            {
                state.LoanAmount = amount;
                if (state.CalculationType == CalculationType.OIS)
                {
                    await _botClient.SendMessage(chatId, "Please enter the number of days for OIS calculation:");
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter the number of years:");
                }
                state.Step = 3;
            }
            else
            {
                await _botClient.SendMessage(chatId, "Please enter a valid positive amount.");
            }
        }

        public async Task HandleYearsInput(long chatId, UserState state, string input)
        {
            if (state.CalculationType == CalculationType.OIS)
            {
                if (int.TryParse(input, out int days) && days > 0)
                {
                    state.Days = days;
                    await _botClient.SendMessage(chatId,
                        "Please enter the overnight interest rate (e.g., 3.5 for 3.5%):");
                    state.Step = 4;
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter a valid number of days.");
                }
            }
            else
            {
                if (int.TryParse(input, out int years) && years > 0)
                {
                    state.LoanYears = years;

                    if (state.CalculationType == CalculationType.FixedRate)
                    {
                        await _botClient.SendMessage(chatId, "Please enter the interest rate (e.g., 4 for 4%).");
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId,
                            "You've entered a floating rate calculation.\n" +
                            "Now please enter the interest rate for the first 6-month period:");
                    }
                    state.Step = 4;
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter a valid number of years.");
                }
            }
        }

        public async Task HandleRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                state.FirstRate = rate;

                switch (state.CalculationType)
                {
                    case CalculationType.FixedRate:
                        await _calculationHandlers.HandleFixedRateCalculation(chatId, state);
                        break;
                    case CalculationType.FloatingRate:
                        await _botClient.SendMessage(chatId,
                            $"First 6-month period rate is set to {rate}%.\n" +
                            "Please enter the interest rate for the second 6-month period:");
                        state.Step = 5;
                        break;
                    case CalculationType.OIS:
                        await _calculationHandlers.HandleOISCalculation(chatId, state);
                        break;
                }
            }
            else
            {
                var errorMessage = state.CalculationType == CalculationType.FixedRate
                    ? "Please enter a valid interest rate."
                    : state.CalculationType == CalculationType.FloatingRate
                    ? "Please enter a valid interest rate for the first 6-month period."
                    : state.CalculationType == CalculationType.OIS
                    ? "Please enter a valid overnight interest rate."
                    : "Please enter a valid rate.";

                await _botClient.SendMessage(chatId, errorMessage);
            }
        }

        public async Task HandleSecondRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                state.SecondRate = rate;
                await _calculationHandlers.HandleFloatingRateCalculation(chatId, state);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Please enter a valid interest rate for the second period.");
            }
        }
    }
}
