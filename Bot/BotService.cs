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
            // Извлекаем message или callbackQuery
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            // Получаем chatId из message или callbackQuery
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
                // Если оба message и callbackQuery равны null, ничего не делаем
                return;
            }

            // Теперь chatId гарантированно не null, можем работать с ним

            // Проверяем или создаем состояние пользователя
            var userState = EnsureUserState(chatId);

            if (message?.Text != null)
            {
                // Если сообщение не пустое, выполняем логику для текста
                var text = message.Text;

                // Обработка команды /help
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

                // Начинаем или продолжаем диалог с пользователем
                if (userState.Step == 0)
                {
                    // Предлагаем выбрать тип расчета
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
                    // Если пользователь выбрал тип расчета
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
                        await botClient.SendMessage(chatId, "Please enter the interest rate (e.g., 4 for 4%).");
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
                        userState.InterestRate = rate;

                        // Выполняем расчет в зависимости от типа расчета
                        decimal totalInterest;
                        decimal totalPayment;

                        if (userState.CalculationType == CalculationType.FixedRate)
                        {
                            totalInterest = userState.LoanAmount * (userState.InterestRate / 100) * userState.LoanYears;
                            totalPayment = userState.LoanAmount + totalInterest;
                        }
                        else // FloatingRate
                        {
                            // Для плавающего процента можно применить другую логику
                            totalInterest = userState.LoanAmount * (userState.InterestRate / 100) * userState.LoanYears * 1.1m; // например, +10% к процентной ставке для плавающего
                            totalPayment = userState.LoanAmount + totalInterest;
                        }

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

            // Если callbackQuery не равен null, обрабатываем коллбэк
            if (callbackQuery != null)
            {
                var callbackData = callbackQuery.Data;

                // Обработка выбора типа расчета из callbackData
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

    public enum CalculationType
    {
        None,
        FixedRate,
        FloatingRate
    }

    // Класс для хранения состояния пользователя
    public class UserState
    {
        public int Step { get; set; } = 0;  // Шаг диалога
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public decimal InterestRate { get; set; }
        public CalculationType CalculationType { get; set; } = CalculationType.None; // Тип расчета (Fixed или Floating)

        // Сбросить состояние
        public void Reset()
        {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            InterestRate = 0;
            CalculationType = CalculationType.None; // сбрасываем тип расчета
        }
    }
}
