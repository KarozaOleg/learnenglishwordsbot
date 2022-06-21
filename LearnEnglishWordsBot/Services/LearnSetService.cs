using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LearnEnglishWordsBot.Services
{
    public class LearnSetService : ILearnSetService
    {
        private string ConnectionString { get; }
        private ILogger Logger { get; }
        private ILearnSetRepository LearnSetRepository { get; }
        private IWordsRepository WordsRepository { get; }
        private INotifyService NotifyService { get; }

        public LearnSetService(IOptions<DatabaseSettings> databaseOptions, ILogger<LearnSetService> logger, ILearnSetRepository learnSetRepository, IWordsRepository wordsRepository, INotifyService notifyService)
        {
            ConnectionString = databaseOptions.Value.DefaultConnection;
            Logger = logger;
            LearnSetRepository = learnSetRepository;
            WordsRepository = wordsRepository;
            NotifyService = notifyService;
        }

        public void SetSendLearnSetsInfo(int idUser)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                var infoMessage = new StringBuilder(200);
                var commands = new HashSet<string>();

                var learningSets = LearnSetRepository.GetForUser(conn, idUser);
                var learnSets = LearnSetRepository.GetAll(conn);
                for (int i = 0; i < learnSets.Length; i++)
                {
                    infoMessage.Append($"Набор#{learnSets[i].Id}").Append(Environment.NewLine);
                    infoMessage.Append($"Название: \"").Append(learnSets[i].Name).Append("\"").Append(Environment.NewLine);
                    infoMessage.Append("Состоит из ").Append(learnSets[i].AmountWords).Append(" слов").Append(Environment.NewLine);

                    LearnSetRepository.GetAmountLearned(idUser, learnSets[i].Id, out int amountLearned);
                    infoMessage.Append("Был изучен тобой ").Append(amountLearned).Append(" раз").Append(Environment.NewLine);

                    var isLearning = learningSets.Select(ls => ls.Id == learnSets[i].Id).Contains(true);
                    if (isLearning)
                    {
                        var commandStop = $"/{BotCommand.LearnSets_stop} {learnSets[i].Id}";
                        commands.Add(commandStop);

                        var amountLearnedWords = WordsRepository.GetAmountLearnedWords(conn, idUser, learnSets[i].Id);
                        infoMessage.Append("Уже угадано ").Append(amountLearnedWords).Append(" слов!").Append(Environment.NewLine);
                        infoMessage.Append("Чтобы выключить этот набор отправь команду:").Append(Environment.NewLine);
                        infoMessage.Append(" - ").Append(commandStop).Append(Environment.NewLine);
                    }
                    else
                    {
                        var commandStart = $"/{BotCommand.LearnSets_start} {learnSets[i].Id}";
                        commands.Add(commandStart);

                        infoMessage.Append("Чтобы включить этот набор отправь команду:").Append(Environment.NewLine);
                        infoMessage.Append(" - ").Append(commandStart).Append(Environment.NewLine);                        
                    }

                    if (i < learnSets.Length - 1)
                        infoMessage.Append(" ------- ").Append(Environment.NewLine);
                }

                var message = new VoteMessage(infoMessage.ToString(), commands);
                NotifyService.SetSend(idUser, message);
            }
        }

        public bool SetLearnSetStart(int idUser, int idLearnSet, out string learnSetName)
        {
            if (LearnSetRepository.GetName(idLearnSet, out learnSetName) == false)
            {
                Logger.LogError($"Error getting learnSetName in LearnSetService.SetLearnSetStart for set with id: {idLearnSet} and user id:{idUser}");
                return false;
            }
            if (LearnSetRepository.SetAddToLearning(idUser, idLearnSet) == false)
            {
                Logger.LogError($"Error SetAddToLearning in LearnSetService.SetLearnSetStart for set with id: {idLearnSet} and user id:{idUser}");
                return false;
            }
            return true;
        }

        public bool SetLearnSetStop(int idUser, int idLearnSet, out string learnSetName)
        {
            if (LearnSetRepository.GetName(idLearnSet, out learnSetName) == false)
            {
                Logger.LogError($"Error getting learnSetName in LearnSetService.SetLearnSetStop for set with id: {idLearnSet} and user id:{idUser}");
                return false;
            }
            try
            {
                LearnSetRepository.SetRemoveFromLearning(idUser, idLearnSet);
            }
            catch (Exception ex)

            {
                Logger.LogError(ex, $"Error SetAddToLearning in LearnSetService.SetLearnSetStop for set with id: {idLearnSet} and user id:{idUser}");
                return false;
            }
            return true;
        }        
    }
}
