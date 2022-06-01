using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Program
{
    public static async Task Main()
    {
        string botToken = "BOT TOKEN";
        TelegramBotClient botClient = new TelegramBotClient(botToken);
        var me = await botClient.GetMeAsync();

        using var cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);
        Console.ReadLine();

        cts.Cancel();
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine($"Receive message type: {message.Type}. Text: {message.Text}");
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "Menu" => GetMainInlineKeyboard(botClient, message, false),
            _ => StartMessage(botClient, message)
        };
        Message sentMessage = await action;
        Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

        static ReplyKeyboardMarkup GetStartReplyKeyboard(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "Menu" },
                })
            {
                ResizeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        static async Task<Message> StartMessage(ITelegramBotClient botClient, Message message)
        {
            const string startText = "<b>AllCity</b>\n\nAll activities in <i>{cityName}</i>";

            await Task.Delay(250);

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: startText,
                                                        parseMode: ParseMode.Html,
                                                        replyMarkup: GetStartReplyKeyboard(botClient, message));
        }
    }

    static InlineKeyboardMarkup GetCafeButtons(string instagramLink)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithUrl("Instagram", instagramLink),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Main", "BackToMain"),
                    },
            });

        return inlineKeyboard;
    }

    static async Task<Message> GetMainInlineKeyboard(ITelegramBotClient botClient, Message message, bool isEdit)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Food", "Food"),
                    },
            });

        if (isEdit)
        {
            return await botClient.EditMessageTextAsync(chatId: message.Chat.Id,
                                                        messageId: message.MessageId,
                                                        text: "Select <i>category</i>",
                                                        parseMode: ParseMode.Html,
                                                        replyMarkup: inlineKeyboard);
        }
        else
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                       text: "Select <i>category</i>",
                                                       parseMode: ParseMode.Html,
                                                       replyMarkup: inlineKeyboard);
        }
    }


    static int currentPage = 0;

    // Process Inline Keyboard callback data
    static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        int minPage = 1;
        int maxCafePage = 3;

        switch (callbackQuery.Data)
        {
            case "Food":
                currentPage = 1;
                break;

            case "BackToMain":
                currentPage = 0;
                await GetMainInlineKeyboard(botClient, callbackQuery.Message, true);
                break;

            case "Next":
                if (currentPage < maxCafePage)
                    currentPage += 1;
                break;
            case "Back":
                if (currentPage > minPage)
                    currentPage -= 1;
                break;

            case "cafe1":
                await GetCafeInfo("Cafe Name 1",
                    "Cafe description",
                    "10:00", "23:00",
                    "+123321456", "https://www.instagram.com/", botClient, callbackQuery);
                break;
            case "cafe2":
                await GetCafeInfo("Cafe name 2",
                    "Cafe description",
                    "10:00", "23:00",
                    "+123321456",
                    "https://www.instagram.com/", botClient, callbackQuery);
                break;
            case "cafe3":
                await GetCafeInfo("Cafe name 3",
                    "Cafe description",
                    "09:00", "23:00",
                    "123321456",
                    "https://www.instagram.com/", botClient, callbackQuery);
                break;
            case "cafe4":
                await GetCafeInfo("Cafe 4",
                    "Cafe description",
                    "10:00", "22:00",
                    "+123321456",
                    "https://www.instagram.com/", botClient, callbackQuery);
                break;
            case "cafe5":
                await GetCafeInfo("Cafe 5",
                    "Cafe description",
                    "08:00", "23:00",
                    "123321456",
                    "https://instagram.com/", botClient, callbackQuery);
                break;
            case "cafe6":
                await GetCafeInfo("Cafe 6",
                    "Cafe Description",
                    "08:30", "21:00",
                    "+123321456",
                    "https://instagram.com/", botClient, callbackQuery);
                break;
            case "cafe7":
                await GetCafeInfo("Cafe 7",
                    "Cafe description",
                    "10:00", "22:00",
                    "+123321456", "https://instagram.com/", botClient, callbackQuery);
                break;
            case "cafe8":
                await GetCafeInfo("Cafe 8",
                    "Cafe description",
                    "10:00", "22:00",
                    "+123321456",
                    "https://instagram.com/", botClient, callbackQuery);
                break;
            case "cafe9":
                await GetCafeInfo("Cafe 9",
                    "Cafe description",
                    "10:00", "23:00",
                    "+123321456",
                    "https://instagram.com/", botClient, callbackQuery);
                break;
        }

        switch (currentPage)
        {
            case 1:
                await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"Select <i>cafe</i>\n<b>{currentPage}/{maxCafePage}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: GetFirstCafeInlineKeyboard());
                break;
            case 2:
                await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"Select <i>cafe</i>\n<b>{currentPage}/{maxCafePage}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: GetSecondCafeInlineKeyboard());
                break;
            case 3:
                await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"Select <i>cafe</i>\n<b>{currentPage}/{maxCafePage}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: GetThirdCafeInlineKeyboard());
                break;
        }
    }

    static InlineKeyboardMarkup GetFirstCafeInlineKeyboard()
    {
        Task.Delay(100);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 1", "cafe1"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 2", "cafe2"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 3", "cafe3"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Back ⬅️", "Back"),
                        InlineKeyboardButton.WithCallbackData("Next ➡️", "Next"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Main", "BackToMain"),
                    },
            });

        return inlineKeyboard;
    }

    static InlineKeyboardMarkup GetSecondCafeInlineKeyboard()
    {
        Task.Delay(100);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 4", "cafe4"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 5", "cafe5"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 6", "cafe6"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Back ⬅️", "Back"),
                        InlineKeyboardButton.WithCallbackData("Next ➡️", "Next"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Main", "BackToMain"),
                    },
            });

        return inlineKeyboard;
    }

    static InlineKeyboardMarkup GetThirdCafeInlineKeyboard()
    {
        Task.Delay(100);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 7", "cafe7"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 8", "cafe8"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cafe 9", "cafe9"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Back ⬅️", "Back"),
                        InlineKeyboardButton.WithCallbackData("Next ➡️", "Next"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Main", "BackToMain"),
                    },
            });

        return inlineKeyboard;
    }

    static async Task<Message> GetCafeInfo(string cafeName, string kitchen, string workStart, string workEnd, string cafePhoneNumber, string instagramLink, ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        string description = $"<b>{cafeName}</b>" +
            $"\n\n<i>Description</i>:\n{kitchen}" +
            $"\n\n<i>Schedule</i>:\nFrom {workStart} to {workEnd}" +
            $"\n\n<i>Contacts</i>:\nPhone number: {cafePhoneNumber}";

        return await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id,
            text: description,
            replyMarkup: GetCafeButtons(instagramLink),
            parseMode: ParseMode.Html);
    }

    static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
    {
        return Task.CompletedTask;
    }

    static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        return Task.CompletedTask;
    }
}