using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TelegramBot_Fitz.Core; // Для использования расчетов

namespace TelegramBot_Fitz.Bot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, UserState> _userStates;
        private readonly FixedRateLoanCalculator _fixedRateLoanCalculator;
        private readonly FloatingRateLoanCalculator _floatingRateLoanCalculator;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();
            _fixedRateLoanCalculator = new FixedRateLoanCalculator();
            _floatingRateLoanCalculator = new FloatingRateLoanCalculator();
        }
        public void Start()
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            long chatId;
            if (message != null)
            {
                chatId = message.Chat.Id;
            }
            else if (callbackQuery != null)
            {
                chatId = callbackQuery.Message.Chat.Id;
            }
            else
            {
                return;
            }

            var userState = EnsureUserState(chatId);

            if (message?.Text != null)
            {
                var text = message.Text;

                if (text.StartsWith("/help"))
                {
                    await botClient.SendMessage(chatId, "This is your loan calculator bot! \n\n" +
                                                        "To get started, please follow the steps. " +
                                                        "1. Enter the loan amount. " +
                                                        "2. Enter the loan duration. " +
                                                        "3. Enter the interest rate. " +
                                                        "The bot will calculate the total loan payment.");
                    return;
                }

                if (userState.Step == 0)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("Fixed Rate", "FixedRate") },
                        new [] { InlineKeyboardButton.WithCallbackData("Floating Rate", "FloatingRate") }
                    });
                    await botClient.SendMessage(chatId, "Please choose the type of calculation:\n" +
                                                        "1. Fixed Rate\n2. Floating Rate", replyMarkup: inlineKeyboard);
                    userState.Step = 1;
                }
                else if (userState.Step == 1)
                {
                    if (text.Equals("Fixed Rate", StringComparison.OrdinalIgnoreCase))
                    {
                        userState.CalculationType = CalculationType.FixedRate;
                        await botClient.SendMessage(chatId, "You selected Fixed Rate calculation. Please enter the loan amount.");
                        userState.Step = 2;
                    }
                    else if (text.Equals("Floating Rate", StringComparison.OrdinalIgnoreCase))
                    {
                        userState.CalculationType = CalculationType.FloatingRate;
                        await botClient.SendMessage(chatId, "You selected Floating Rate calculation. Please enter the loan amount.");
                        userState.Step = 2;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Invalid option. Please select Fixed Rate or Floating Rate.");
                    }
                }
                else if (userState.Step == 2)
                {
                    if (decimal.TryParse(text, out decimal loanAmount) && loanAmount > 0)
                    {
                        userState.LoanAmount = loanAmount;
                        await botClient.SendMessage(chatId, "Please enter the number of years.");
                        userState.Step = 3;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Please enter a valid positive loan amount.");
                    }
                }
                else if (userState.Step == 3)
                {
                    if (int.TryParse(text, out int loanYears) && loanYears > 0)
                    {
                        userState.LoanYears = loanYears;

                        if(userState.CalculationType == CalculationType.FixedRate)
                        {
                            await botClient.SendMessage(chatId, "Please enter the interest rate (e.g., 4 for 4%).");
                        }
                        else
                        {
                            await botClient.SendMessage(chatId,
                                "You've entered a floating rate calculation.\n" +
                                "Now please enter the interest rate for the first 6-month period:");
                        }
                        userState.Step = 4;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Please enter a valid number of years.");
                    }
                }
                else if (userState.Step == 4)
                {
                    if (decimal.TryParse(text, out decimal rate) && rate > 0)
                    {
                        userState.FirstRate = rate;
                        string resultMessage;

                        if (userState.CalculationType == CalculationType.FixedRate)
                        {
                            var calculationResult = _fixedRateLoanCalculator.CalculateLoan(
                                userState.LoanAmount,
                                userState.LoanYears,
                                userState.FirstRate
                            );
                            resultMessage = _fixedRateLoanCalculator.FormatCalculationResult(
                                calculationResult,
                                userState.LoanYears
                            );
                        }
                        else // FloatingRate
                        {
                            await botClient.SendMessage(chatId,
                                $"First 6-month period rate is set to {rate}%.\n" +
                                "Please enter the interest rate for the second 6-month period:");
                            userState.Step = 5;
                        }

                        
                    }
                    else
                    {
                        var errorMessage = userState.CalculationType == CalculationType.FixedRate
                        ? "Please enter a valid interest rate."
                        : "Please enter a valid interest rate for the first 6-month period.";

                        await botClient.SendMessage(chatId, errorMessage);

                        //await botClient.SendMessage(chatId, "Please enter a valid interest rate.");
                    }
                }
                else if (userState.Step == 5)
                {
                    if(decimal.TryParse(text,out decimal rate)&& rate > 0)
                    
                        userState.SecondRate = rate;
                    _floatingRateLoanCalculator.LoanAmount = userState.LoanAmount;
                    _floatingRateLoanCalculator.TotalYears = userState.LoanYears;
                    _floatingRateLoanCalculator.FirstRate = userState.FirstRate;
                    _floatingRateLoanCalculator.SecondRate = userState.SecondRate;

                    decimal totalInterest = _floatingRateLoanCalculator.CalculateTotalInterest();
                    decimal totalPayment = _floatingRateLoanCalculator.CalculateTotalPayment();

                    var resultMessage =
                        $"Loan calculation with floating rate:\n" +
                        $"First 6 months rate: {userState.FirstRate}%\n" +
                        $"Second 6 months rate: {userState.SecondRate}%\n" +
                        $"First period interest rate: {_floatingRateLoanCalculator.LoanAmount * (userState.FirstRate / 100) * (6 / 12m):F2} USD\n" +
                        $"Second period interest rate: {_floatingRateLoanCalculator.LoanAmount * (userState.SecondRate / 100) * (6 / 12m):F2} USD\n" +
                        $"Total interest: {totalInterest:F2} USD\n" +
                        $"Total payment: {totalPayment:F2} USD";

                    await botClient.SendMessage(chatId,resultMessage);
                    userState.Reset();
                }
                else
                {
                    await botClient.SendMessage(chatId, "Please enter a valid interest rate for the second rate");
                }
            }

            if (callbackQuery != null)
            {
                var callbackData = callbackQuery.Data;

                if (callbackData == "FixedRate")
                {
                    userState.CalculationType = CalculationType.FixedRate;
                    await botClient.SendMessage(chatId, "You selected Fixed Rate. Please enter the loan amount.");
                    userState.Step = 2;
                }
                else if (callbackData == "FloatingRate")
                {
                    userState.CalculationType = CalculationType.FloatingRate;
                    await botClient.SendMessage(chatId, "You selected Floating Rate. Please enter the loan amount.");
                    userState.Step = 2;
                }
            }
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
