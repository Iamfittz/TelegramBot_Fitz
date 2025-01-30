using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz.Bot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, UserState> _userStates;
        private readonly FixedRateLoanCalculator _fixedRateLoanCalculator;
        private readonly FloatingRateLoanCalculator _floatingRateLoanCalculator;
        private readonly OISCalculator _oisCalculator;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();
            _fixedRateLoanCalculator = new FixedRateLoanCalculator();
            _floatingRateLoanCalculator = new FloatingRateLoanCalculator();
            _oisCalculator = new OISCalculator();
        }

        public void Start()
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        }

        private async Task ShowWelcomeMessage(long chatId)
        {
            var welcomeMessage =
                "👋 Welcome to the Derivatives Calculator Bot!\n\n" +
                "I'm your personal assistant for calculating " +
                "various derivative instruments. " +
                "I can help you evaluate different types " +
                "of derivatives and their rates.\n\n" +
                "Before we begin, please select the derivative " +
                "instrument you'd like to calculate:";

            var inlineKeybord = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("📊 IRS Fixed Float", "IRS_Fixed_Float") },
                new[] { InlineKeyboardButton.WithCallbackData("💹 IRS OIS", "IRS_OIS") }
            });

            await _botClient.SendMessage(
                chatId,
                welcomeMessage,
                replyMarkup: inlineKeybord
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            long chatId = GetChatId(update);
            if (chatId == 0) return;

            var userState = EnsureUserState(chatId);

            if (message?.Text != null)
            {
                var text = message.Text;

                if (text.StartsWith("/start") || text.StartsWith("/help"))
                {
                    await ShowWelcomeMessage(chatId);
                    return;
                }

                switch (userState.Step)
                {
                    case 2:
                        await HandleAmountInput(chatId, userState, text);
                        break;
                    case 3:
                        await HandleYearsInput(chatId, userState, text);
                        break;
                    case 4:
                        await HandleRateInput(chatId, userState, text);
                        break;
                    case 5:
                        await HandleSecondRateInput(chatId, userState, text);
                        break;
                }
            }

            if (callbackQuery != null)
            {
                await HandleCallbackQuery(chatId, userState, callbackQuery.Data);
            }
        }

        private long GetChatId(Update update)
        {
            if (update.Message != null)
                return update.Message.Chat.Id;
            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat.Id;
            return 0;
        }

        private async Task HandleAmountInput(long chatId, UserState state, string input)
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

        private async Task HandleYearsInput(long chatId, UserState state, string input)
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

        private async Task HandleFixedRateCalculation(long chatId, UserState state)
        {
            var calculationResult = _fixedRateLoanCalculator.CalculateLoan(
                state.LoanAmount,
                state.LoanYears,
                state.FirstRate
            );
            var resultMessage = _fixedRateLoanCalculator.FormatCalculationResult(
                calculationResult,
                state.LoanYears
            );
            await _botClient.SendMessage(chatId, resultMessage);
            state.Reset();
        }

        private async Task HandleFloatingRateCalculation(long chatId, UserState state)
        {
            _floatingRateLoanCalculator.LoanAmount = state.LoanAmount;
            _floatingRateLoanCalculator.TotalYears = state.LoanYears;
            _floatingRateLoanCalculator.FirstRate = state.FirstRate;
            _floatingRateLoanCalculator.SecondRate = state.SecondRate;

            decimal totalInterest = _floatingRateLoanCalculator.CalculateTotalInterest();
            decimal totalPayment = _floatingRateLoanCalculator.CalculateTotalPayment();

            var resultMessage =
                $"Loan calculation with floating rate:\n" +
                $"First 6 months rate: {state.FirstRate}%\n" +
                $"Second 6 months rate: {state.SecondRate}%\n" +
                $"First period interest: {_floatingRateLoanCalculator.LoanAmount * (state.FirstRate / 100) * (6 / 12m):F2} USD\n" +
                $"Second period interest: {_floatingRateLoanCalculator.LoanAmount * (state.SecondRate / 100) * (6 / 12m):F2} USD\n" +
                $"Total interest: {totalInterest:F2} USD\n" +
                $"Total payment: {totalPayment:F2} USD";

            await _botClient.SendMessage(chatId, resultMessage);
            state.Reset();
        }

        private async Task HandleRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                state.FirstRate = rate;

                switch (state.CalculationType)
                {
                    case CalculationType.FixedRate:
                        await HandleFixedRateCalculation(chatId, state);
                        break;
                    case CalculationType.FloatingRate:
                        await _botClient.SendMessage(chatId,
                            $"First 6-month period rate is set to {rate}%.\n" +
                            "Please enter the interest rate for the second 6-month period:");
                        state.Step = 5;
                        break;
                    case CalculationType.OIS:
                        _oisCalculator.NotionalAmount = state.LoanAmount;
                        _oisCalculator.Days = state.Days;
                        _oisCalculator.OvernightRate = state.FirstRate;

                        var calculationResult = _oisCalculator.CalculateOIS();
                        var resultMessage = _oisCalculator.FormatCalculationResult(calculationResult);
                        await _botClient.SendMessage(chatId, resultMessage);
                        state.Reset();
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

        private async Task HandleSecondRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                state.SecondRate = rate;
                await HandleFloatingRateCalculation(chatId, state);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Please enter a valid interest rate for the second period.");
            }
        }

        private async Task HandleCallbackQuery(long chatId, UserState state, string callbackData)
        {
            switch (callbackData)
            {
                case "IRS_Fixed_Float":
                    await ShowRateTypeSelection(chatId);
                    state.Step = 1;
                    break;
                case "IRS_OIS":
                    state.CalculationType = CalculationType.OIS;
                    await _botClient.SendMessage(chatId,
                        "You've selected OIS (Overnight Index Swap).\n" +
                        "Please enter the notional amount:");
                    state.Step = 2;
                    break;
                case "FixedRate":
                    state.CalculationType = CalculationType.FixedRate;
                    await _botClient.SendMessage(chatId,
                        "You selected Fixed Rate. Please enter the loan amount.");
                    state.Step = 2;
                    break;
                case "FloatingRate":
                    state.CalculationType = CalculationType.FloatingRate;
                    await _botClient.SendMessage(chatId,
                        "You selected Floating Rate. Please enter the loan amount.");
                    state.Step = 2;
                    break;
            }
        }

        private async Task ShowRateTypeSelection(long chatId)
        {
            await _botClient.SendMessage(chatId,
                "You've selected Interest Rate Swap (IRS) Fixed Float.\n" +
                "Please choose the type of rate calculation:");

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData("📈 Fixed Rate", "FixedRate") },
                new [] { InlineKeyboardButton.WithCallbackData("📊 Floating Rate", "FloatingRate") }
            });

            await _botClient.SendMessage(chatId,
                "Select your preferred rate type:",
                replyMarkup: inlineKeyboard);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

        private UserState EnsureUserState(long chatId)
        {
            if (!_userStates.ContainsKey(chatId))
            {
                _userStates[chatId] = new UserState();
            }
            return _userStates[chatId];
        }
    }
}