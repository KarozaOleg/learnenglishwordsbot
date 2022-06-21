using Dapper;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LearnEnglishWordsBot.Services
{
    public class TelegramNotifyService : INotifyService, IDisposable
    {
        private ILogger Logger { get; }
        private TelegramBotClient Client { get; }
        private string ConnectionString { get; }
        private IMessagesRepository MessageCommandRepository { get; }
        private IAnswersRepository AnswersRepository { get; }
        private ThreadFlow<(int idUser, string message, ReplyKeyboardMarkup keyboad)> FlowMessages { get; }

        public TelegramNotifyService(ILogger<TelegramNotifyService> logger, IMessagesRepository messageCommandRepository, IAnswersRepository answersRepository, IOptions<BotSettings> settings, IOptions<DatabaseSettings> databaseSettings)
        {
            FlowMessages = new ThreadFlow<(int idUser, string message, ReplyKeyboardMarkup keyboad)>("telegram_messages", WorkWithFlowMessage);

            Logger = logger;
            MessageCommandRepository = messageCommandRepository;
            AnswersRepository = answersRepository;

            ConnectionString = databaseSettings.Value.DefaultConnection;

            var _settings = settings.Value;
            var httpClient = default(HttpClient);
            // use proxy if configured in appsettings.*.json
            if (string.IsNullOrEmpty(_settings.Socks5Host) == false)
            {
                var proxy = new HttpToSocks5Proxy(_settings.Socks5Host, _settings.Socks5Port);

                if (string.IsNullOrEmpty(_settings.Socks5Username) == false)
                    proxy = new HttpToSocks5Proxy(_settings.Socks5Host, _settings.Socks5Port, _settings.Socks5Username, _settings.Socks5Password);
                proxy.ResolveHostnamesLocally = true;
                httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });
            }

            Client = new TelegramBotClient(_settings.BotToken, httpClient);
        }
        
        public void Dispose()
        {
            FlowMessages?.Dispose();
        }

        public void SetSend(int idUser, string message)
        {
            SendMessage(idUser, message, null);
        }

        public void SetSend(int idUser, VoteMessage message)
        {
            SendMessage(idUser, message.Message, ReturnKeyboard(message.Answers));
        }

        public void SetSend(long idChat, VoteMessage message)
        {
            Client.SendTextMessageAsync(idChat, message.Message, ParseMode.Markdown, replyMarkup: ReturnKeyboard(message.Answers));
        }

        public void SetCreateRecipient(int idUser, long idChat, ref bool isUserAlreadyExist)
        {
            if (IsExist(idUser, out long idChatOld))
            {
                isUserAlreadyExist = true;
                if (idChat != idChatOld)
                {
                    UpdateIdChat(idUser, idChat);
                }
                return;
            }

            using (IDbConnection conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                var sql =
                    "INSERT INTO " +
                        "public.usersbytelegram (id, idUser, idChat) " +
                    "VALUES" +
                        "(DEFAULT, @idUser, @idChat);";
                conn.Execute(sql, new { idUser, idChat });
            }
        }

        void UpdateIdChat(int idUser, long idChat)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                var sql =
                    "UPDATE " +
                        "public.usersbytelegram " +
                     "SET " +
                        "idChat='@IdChat' " +
                     "WHERE " +
                        "idUser=@idUser;";
                conn.Execute(sql, new { idUser, idChat });
            }
        }

        ReplyKeyboardMarkup ReturnKeyboard(HashSet<string> answers)
        {
            var bRows = new KeyboardButton[answers.Count][];

            var i = 0;
            foreach (var answer in answers)
                bRows[i++] = new KeyboardButton[] { answer };

            return new ReplyKeyboardMarkup(bRows) { ResizeKeyboard = true };
        }

        void SendMessage(int idUser, string message, ReplyKeyboardMarkup keyboad)
        {
            FlowMessages.Set((idUser, message, keyboad));
        }

        bool IsExist(int idUser, out long idChat)
        {
            idChat = 0;
            using (IDbConnection db = new NpgsqlConnection(ConnectionString))
            {
                var sql =
                    "SELECT " +
                        "idChat " +
                    "FROM " +
                        "public.usersByTelegram " +
                     "WHERE " +
                        "idUser = @IdUser";
                var idChatNullable = db.Query<long?>(sql, new { idUser }).FirstOrDefault();

                if (idChatNullable == null)
                    return false;

                idChat = (int)idChatNullable;
                return true;
            }
        }

        public void SetSendWithBotCommandAnswers(int idUser, string message = "")
        {
            if (message.Length < 1)
            {
                message = MessageCommandRepository.GetMisunderstood();
            }
            var answers = AnswersRepository.GetAnswersBotCommand();
            SetSend(idUser, new VoteMessage(message, answers));
        }

        public void SetSendWithBotCommandAnswers(long idChat)
        {
            var message = MessageCommandRepository.GetMisunderstood();
            var answers = AnswersRepository.GetAnswersBotCommand();
            SetSend(idChat, new VoteMessage(message, answers));
        }

        void WorkWithFlowMessage((int idUser, string mess, ReplyKeyboardMarkup keyboad) message)
        {
            if (IsExist(message.idUser, out long idChat) == false)
            {
                Logger.LogError($"Can't send message for user: {message.idUser}, can't get idChat");
                return;
            }
            var msg = message.mess.Replace("_", "\\_");
            //todo: receive status, log errors
            Client.SendTextMessageAsync(idChat, msg, ParseMode.Markdown, replyMarkup: message.keyboad);
        }
    }
}
