using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace TelegramBot_Fitz.Bot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, UserState> _userStates;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();
        }

        public void Start()
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            if (message?.Text != null)
            {
                var chatId = message.Chat.Id;

                if (!_userStates.ContainsKey(chatId))
                {
                    _userStates[chatId] = new UserState();
                }

                var userState = _userStates[chatId];

                // Обработка команды /help
                if (message.Text.StartsWith("/help"))
                {
                    await botClient.SendMessage(chatId, "This is your loan calculator bot! \n\n" +
                                                        "To get started, please follow the steps. " +
                                                        "1. Enter the loan amount. " +
                                                        "2. Enter the loan duration. " +
                                                        "3. Enter the interest rate. " +
                                                        "The bot will calculate the total loan payment.");
                    return;
                }

                // Начинаем или продолжаем диалог с пользователем
                if (userState.Step == 0)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("Help", "/help") }
                    });
                    await botClient.SendMessage(chatId, "Welcome to the loan calculator! Please enter the loan amount.", replyMarkup: inlineKeyboard);
                    userState.Step = 1;
                }
                else if (userState.Step == 1)
                {
                    if (decimal.TryParse(message.Text, out decimal loanAmount) && loanAmount > 0)
                    {
                        userState.LoanAmount = loanAmount;
                        await botClient.SendMessage(chatId, "Please enter the number of years.");
                        userState.Step = 2;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Please enter a valid positive loan amount.");
                    }
                }
                else if (userState.Step == 2)
                {
                    if (int.TryParse(message.Text, out int loanYears) && loanYears > 0)
                    {
                        userState.LoanYears = loanYears;
                        await botClient.SendMessage(chatId, "Please enter the interest rate (e.g., 4 for 4%).");
                        userState.Step = 3;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Please enter a valid number of years.");
                    }
                }
                else if (userState.Step == 3)
                {
                    if (decimal.TryParse(message.Text, out decimal rate) && rate > 0)
                    {
                        userState.InterestRate = rate;

                        // Выполняем расчет
                        var totalInterest = userState.LoanAmount * (userState.InterestRate / 100) * userState.LoanYears;
                        var totalPayment = userState.LoanAmount + totalInterest;

                        var resultMessage = $"The total interest for {userState.LoanYears} years is: {totalInterest:F2} USD.\n" +
                                            $"The total payment is: {totalPayment:F2} USD.";

                        await botClient.SendMessage(chatId, resultMessage);

                        // Сбросить состояние пользователя
                        userState.Reset();
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Please enter a valid interest rate.");
                    }
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }

    // Класс для хранения состояния пользователя
    public class UserState
    {
        public int Step { get; set; } = 0;  // Шаг диалога
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public decimal InterestRate { get; set; }

        // Сбросить состояние
        public void Reset()
        {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            InterestRate = 0;
        }
    }
}
