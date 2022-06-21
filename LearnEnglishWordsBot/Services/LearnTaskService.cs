using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Data;
using System.Linq;

namespace LearnEnglishWordsBot.Services
{
    public class LearnTaskService : ILearnTaskService
    {
        private const int AmountRandomWordsToAnswers = 4;
        private const int AmountTasksToLearn = 10;
        private string ConnectionString { get; }
        private ILogger Logger { get; }
        private ILearnTaskRepository LearnTaskRepository { get; }
        private ILearnSetRepository LearnSetRepository { get; }
        private IWordsRepository WordsRepository { get; }
        private INotifyService NotifyService { get; }
        private IMessagesRepository MessagesRepository { get; }

        public LearnTaskService(IOptions<DatabaseSettings> databaseOptions, ILogger<LearnTaskService> logger, ILearnTaskRepository learnTaskRepository, ILearnSetRepository learnSetRepository, IWordsRepository wordsRepository, INotifyService notifyService, IMessagesRepository messagesRepository)
        {
            ConnectionString = databaseOptions.Value.DefaultConnection;
            Logger = logger;
            LearnTaskRepository = learnTaskRepository;
            LearnSetRepository = learnSetRepository;
            WordsRepository = wordsRepository;
            NotifyService = notifyService;
            MessagesRepository = messagesRepository;
        }

        public void SetCreateTasksToLearn(int idUser)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {             
                var learningSets = LearnSetRepository.GetForUser(conn, idUser);
                if(learningSets.Length < 1)
                {
                    NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetEmptyLearingSets());
                    return;
                }

                for (int i = 0; i < learningSets.Length; i++)                
                    if(LearnSetRepository.GetIsLearned(conn, idUser, learningSets[i].Id))                    
                        NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetLearnSetIsLearned(learningSets[i].Id, learningSets[i].Name)); 

                var words = WordsRepository.GetRandomForLearnTask(conn, idUser, learningSets.Select(ls => ls.Id).ToArray(), AmountTasksToLearn);
                if (words == null || words.Length < 1)
                {
                    NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetEmptyLearingSets());
                    return;
                }

                var rand = new Random(Guid.NewGuid().GetHashCode());
                for (int i = 0; i < words.Length; i++)
                    LearnTaskRepository.SetCreate(conn, idUser, words[i], rand.Next(0, 11) > 5);
            }
        }        

        public bool SetSendRandomTask(int idUser)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                if (LearnTaskRepository.GetRandom(conn, idUser, out TaskToLearn task) == false)
                    return false;

                SendTask(conn, idUser, task);
                return true;
            }
        }

        public bool SetSendTask(IDbConnection conn, int idUser, int idTask)
        {
            if(LearnTaskRepository.GetTask(conn, idUser, idTask, out TaskToLearn task) == false)
            {
                Logger.LogError($"Error in LearnTasksService.SetSendTask, can't get task for user with id:{idUser}, and task with id:{idTask}");
                return false;
            }
            SendTask(conn, idUser, task);
            return true;
        }

        private void SendTask(IDbConnection conn, int idUser, TaskToLearn task)
        {
            var message = task.IsRevers ? task.InEnglish : task.InRussian;
            var amountRandomWords = (task.AmountWrongAnswer > 2) ? 2 : AmountRandomWordsToAnswers - task.AmountWrongAnswer;
            var exceptWord = task.IsRevers ? task.InRussian : task.InEnglish;
            var answers = WordsRepository.GetRandom(conn, task.IsRevers, exceptWord, amountRandomWords);
            var voteMessage = new VoteMessage(message, answers);

            NotifyService.SetSend(idUser, voteMessage);
            LearnTaskRepository.SetMarkAsSended(conn, idUser, task.IdTask);
        }             
    }
}
