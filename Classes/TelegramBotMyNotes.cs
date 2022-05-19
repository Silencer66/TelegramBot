using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Classes;

namespace TelegramBot
{

    internal class TelegramBotMyNotes
    {
        string StringWithNotes;
        int count = 0;
        private const string TEXT0 = "/start";
        private const string TEXT1 = "Посмотреть список заметок";
        private const string TEXT2 = "Добавить новую заметку";
        private const string TEXT3 = "Отметить выполненную заметку";
        private const string TEXT4 = "Очистить список заметок";
        private const string TEXT5 = "Отмена";
        enum Mod
        {
            ADD,
            MARK,
            NON
        }

        Mod mod = Mod.NON;
        private string _token;
        Telegram.Bot.TelegramBotClient _client; // чтобы не подклюбчать using Telegram.Bot.    
        public TelegramBotMyNotes(string token)
        {
            this._token = token;
        }

        //УБРАТЬ В ОТДЕЛЬНЫЙ КЛАСС ВМЕСТЕ С ПАРАМЕТРОМ В GETUPDATES


        internal void GetUpdates()
        {

            _client = new Telegram.Bot.TelegramBotClient(_token);
            var me = _client.GetMeAsync().Result;     // получаем информацию о нашем телеграм боте 
            if (me != null && !string.IsNullOrEmpty(me.Username))
            {
                int offset = 0;
                do
                {
                    try
                    {
                        var updates = _client.GetUpdatesAsync(offset).Result;
                        if (updates != null && updates.Length > 0)
                        {
                            foreach (var update in updates)
                            {
                                processUpdate(update);
                                offset = update.Id + 1;
                            }
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    Thread.Sleep(1000);
                } while (true);
            }
        }

        private async void processUpdate(Telegram.Bot.Types.Update update)
        {

            switch (update.Type)
            {

                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var msg = update.Message.Text;

                    if (msg == TEXT5)
                    {
                        await _client.SendTextMessageAsync(update.Message.Chat.Id, TEXT5, replyMarkup: GetButtons());
                        mod = Mod.NON;
                        break;
                    }
                    else if (mod == Mod.ADD)
                    {
                        await _client.SendTextMessageAsync(update.Message.Chat.Id, DbRequests.DbInsertNewNode(update.Message.Chat.Id, msg), replyMarkup: GetButtons());
                        mod = Mod.NON;
                        break;
                    }
                    else if (mod == Mod.MARK)
                    {
                        try
                        {
                            int result = Int32.Parse(msg);
                            if(result > count || result < 0) 
                            {
                                await _client.SendTextMessageAsync(update.Message.Chat.Id, $"Неверный номер [{msg}]", replyMarkup: GetButtons());
                                mod = Mod.NON;
                                break;
                            }
                            if (DbRequests.DbMarkNote(update.Message.Chat.Id, result))
                                await _client.SendTextMessageAsync(update.Message.Chat.Id, "Заметка помечена", replyMarkup: GetButtons());
                            else
                                await _client.SendTextMessageAsync(update.Message.Chat.Id, $"Не удалось отметить заметку [{msg}]", replyMarkup: GetButtons());
                        }
                        catch (FormatException)
                        {
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, $"Неверный номер1 - [{msg}]", replyMarkup: GetButtons());
                        }
                        mod = Mod.NON;
                        break;
                    }
                    switch (msg)
                    {
                        case TEXT0:

                            Console.WriteLine("Пришел новый пользователь: " + update.Message.Chat.Username);
                            Console.WriteLine(DbRequests.DbInsertNewUser(update.Message.Chat.Id, update.Message.Chat.Username));
                            await _client.SendStickerAsync(update.Message.Chat.Id, "CAACAgIAAxkBAAIEkmJXI76nGeEnQbt2p_gL1-HhPJmeAAIrBwACYyviCZ_EebUOZXzyIwQ", replyMarkup: GetButtons());
                            break;

                        case TEXT1:
                            count = 0;
                            StringWithNotes = DbRequests.DbShowNotes(update.Message.Chat.Id, ref count);
                            if (StringWithNotes == null)
                            {
                                await _client.SendTextMessageAsync(update.Message.Chat.Id, "Список заметок пуст ", replyMarkup: GetButtons());
                                break;
                            }
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, StringWithNotes, replyMarkup: GetButtons());
                            break;

                        case TEXT2:
                            
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите текст заметки...", replyMarkup: CancelButton());
                            mod = Mod.ADD;
                            break;

                        case TEXT3:
                            count = 0;
                            StringWithNotes = DbRequests.DbShowNotes(update.Message.Chat.Id, ref count);
                            if (StringWithNotes == null)
                            {
                                await _client.SendTextMessageAsync(update.Message.Chat.Id, "Список заметок пуст ", replyMarkup: GetButtons());
                                break;
                            }
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер заметки, которую выполнили.", replyMarkup: CancelButton());
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, StringWithNotes, replyMarkup: CancelButton());
                            mod = Mod.MARK;
                            break;

                        case TEXT4:
                            await _client.SendTextMessageAsync(update.Message.Chat.Id, DbRequests.DbClearNotes(update.Message.Chat.Id), replyMarkup: GetButtons());
                            break;

                        default:

                            await _client.SendTextMessageAsync(update.Message.Chat.Id, "Неизвестная команда :(", replyMarkup: GetButtons());
                            break;
                    }
                    Console.WriteLine($"пришло сообщение: " + msg);
                    break;
                default:
                    Console.WriteLine(update.Type + "Не обрабатываем!"); //обрабатываем только Message
                    break;
            }
            //Thread.Sleep(1000);
        }


        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> //это один ряд кнопок
                    {
                        new KeyboardButton { Text = TEXT1 }, new KeyboardButton { Text = TEXT2 }
                    },
                    new List<KeyboardButton>//это второй ряд кнопок
                    {
                        new KeyboardButton { Text = TEXT3 }, new KeyboardButton { Text = TEXT4 }
                    }
                },
                ResizeKeyboard = true,
            };
        }

        //Кнопка отмены 
        private IReplyMarkup CancelButton()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>
                    {
                        new KeyboardButton { Text = TEXT5 }
                    }
                },

                ResizeKeyboard = true,
            };
        }
    }
}