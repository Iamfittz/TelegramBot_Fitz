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
        private readonly MessageHandlers _messageHandlers;
        private readonly CalculationHandlers _calculationHandlers;
        private readonly InputHandlers _inputHandlers;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();

            var fixedCalculator = new FixedRateLoanCalculator();
            var floatingCalculator = new FloatingRateLoanCalculator();
            var oisCalculator = new OISCalculator();

            _messageHandlers = new MessageHandlers(_botClient);
            _calculationHandlers = new CalculationHandlers(_botClient, fixedCalculator, floatingCalculator, oisCalculator);
            _inputHandlers = new InputHandlers(_botClient, _calculationHandlers);
        }

        public void Start()
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
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
                    await _messageHandlers.ShowWelcomeMessage(chatId);
                    return;
                }

                switch (userState.Step)
                {
                    case 2:
                        await _inputHandlers.HandleAmountInput(chatId, userState, text);
                        break;
                    case 3:
                        await _inputHandlers.HandleYearsInput(chatId, userState, text);
                        break;
                    case 4:
                        await _inputHandlers.HandleRateInput(chatId, userState, text);
                        break;
                    case 5:
                        await _inputHandlers.HandleSecondRateInput(chatId, userState, text);
                        break;
                }
            }

            if (callbackQuery != null)
            {
                await HandleCallbackQuery(chatId, userState, callbackQuery.Data);
            }
        }

        private async Task HandleCallbackQuery(long chatId, UserState state, string callbackData)
        {
            if (callbackData.StartsWith("SameRate_"))
            {
                int nextYear = int.Parse(callbackData.Split('_')[1]);
                state.YearlyRates[nextYear - 1] = state.YearlyRates[nextYear - 2]; // Копируем предыдущую ставку
                state.CurrentYear = nextYear;

                if (nextYear < state.LoanYears)
                {
                    // Если есть еще годы, спрашиваем про следующий
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Use same rate", $"SameRate_{nextYear + 1}"),
                    InlineKeyboardButton.WithCallbackData("Enter new rate", $"NewRate_{nextYear + 1}")
                }
            });

                    await _botClient.SendMessage(chatId,
                        $"Rate for year {nextYear} is set to {state.YearlyRates[nextYear - 1]}%.\n" +
                        $"What about year {nextYear + 1}?",
                        replyMarkup: keyboard);
                }
                else
                {
                    // Если это был последний год, делаем расчет
                    await _calculationHandlers.HandleFixedRateCalculation(chatId, state);
                }
            }
            else if (callbackData.StartsWith("NewRate_"))
            {
                int nextYear = int.Parse(callbackData.Split('_')[1]);
                state.CurrentYear = nextYear;
                await _botClient.SendMessage(chatId,
                    $"Please enter the interest rate for year {nextYear} (e.g., 4 for 4%):");
            }

            else
            {
                switch (callbackData)
                {
                    case "IRS_Fixed_Float":
                        await _messageHandlers.ShowRateTypeSelection(chatId);
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
        }

        private long GetChatId(Update update)
        {
            if (update.Message != null)
                return update.Message.Chat.Id;
            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat.Id;
            return 0;
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