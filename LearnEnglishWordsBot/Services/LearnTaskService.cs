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
        readonly int _amountRandomWordsToAnswers = 4;
        readonly int _amountTasksToLearn = 10;
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly ILearnTaskRepository _learnTaskRepository;
        readonly ILearnSetRepository _learnSetRepository;
        readonly IWordsRepository _wordsRepository;
        readonly INotifyService _notifyService;
        readonly IMessagesRepository _messagesRepository;

        public LearnTaskService(
            IOptions<DatabaseSettings> databaseOptions,
            ILogger<LearnTaskService> logger,
            ILearnTaskRepository learnTaskRepository,
            ILearnSetRepository learnSetRepository,
            IWordsRepository wordsRepository,
            INotifyService notifyService,
            IMessagesRepository messagesRepository)
        {
            _connectionString = databaseOptions.Value.DefaultConnection;
            _logger = logger;
            _learnTaskRepository = learnTaskRepository;
            _learnSetRepository = learnSetRepository;
            _wordsRepository = wordsRepository;
            _notifyService = notifyService;
            _messagesRepository = messagesRepository;
        }

        public void SetCreateTasksToLearn(int idUser)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {             
                var learningSets = _learnSetRepository.GetForUser(conn, idUser);
                if(learningSets.Length < 1)
                {
                    _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetEmptyLearingSets());
                    return;
                }

                for (int i = 0; i < learningSets.Length; i++)                
                    if(_learnSetRepository.GetIsLearned(conn, idUser, learningSets[i].Id))                    
                        _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetLearnSetIsLearned(learningSets[i].Id, learningSets[i].Name)); 

                var words = _wordsRepository.GetRandomForLearnTask(conn, idUser, learningSets.Select(ls => ls.Id).ToArray(), _amountTasksToLearn);
                if (words == null || words.Length < 1)
                {
                    _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetEmptyLearingSets());
                    return;
                }

                var rand = new Random(Guid.NewGuid().GetHashCode());
                for (int i = 0; i < words.Length; i++)
                    _learnTaskRepository.SetCreate(conn, idUser, words[i], rand.Next(0, 11) > 5);
            }
        }        

        public bool SetSendRandomTask(int idUser)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                if (_learnTaskRepository.GetRandom(conn, idUser, out TaskToLearn task) == false)
                    return false;

                SendTask(conn, idUser, task);
                return true;
            }
        }

        public bool SetSendTask(IDbConnection conn, int idUser, int idTask)
        {
            if(_learnTaskRepository.GetTask(conn, idUser, idTask, out TaskToLearn task) == false)
            {
                _logger.LogError($"Error in LearnTasksService.SetSendTask, can't get task for user with id:{idUser}, and task with id:{idTask}");
                return false;
            }
            SendTask(conn, idUser, task);
            return true;
        }

        private void SendTask(IDbConnection conn, int idUser, TaskToLearn task)
        {
            var message = task.IsRevers ? task.InEnglish : task.InRussian;
            var amountRandomWords = (task.AmountWrongAnswer > 2) ? 2 : _amountRandomWordsToAnswers - task.AmountWrongAnswer;
            var exceptWord = task.IsRevers ? task.InRussian : task.InEnglish;
            var answers = _wordsRepository.GetRandom(conn, task.IsRevers, exceptWord, amountRandomWords);
            var voteMessage = new VoteMessage(message, answers);

            _notifyService.SetSend(idUser, voteMessage);
            _learnTaskRepository.SetMarkAsSended(conn, idUser, task.IdTask);
        }             
    }
}
