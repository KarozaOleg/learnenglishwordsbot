using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LearnEnglishWordsBot.Services
{
    public class LearnService : ILearnService
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly INotifyService _notifyService;
        readonly IMessagesRepository _messagesRepository;
        readonly ILearnTaskService _learnTaskService;
        readonly ILearnTaskRepository _learnTaskRepository;
        readonly IWordsRepository _wordsRepository;

        public LearnService(
            ILogger<LearnService> logger,
            INotifyService notifyService,
            IOptions<DatabaseSettings> databaseOptions,
            IMessagesRepository messagesRepository,
            ILearnTaskService learnTasksService,
            ILearnTaskRepository learnTaskRepository,
            IWordsRepository wordsRepository)
        {
            _logger = logger;
            _notifyService = notifyService;
            _connectionString = databaseOptions.Value.DefaultConnection;
            _messagesRepository = messagesRepository;
            _learnTaskService = learnTasksService;
            _learnTaskRepository = learnTaskRepository;
            _wordsRepository = wordsRepository;
        }

        public bool SetHandleMessage(int idUser, string message)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                if (_learnTaskRepository.GetSended(conn, idUser, out TaskToLearn taskCurrent) == false)
                    return false;

                var correctAnswer = taskCurrent.IsRevers ? taskCurrent.InRussian : taskCurrent.InEnglish;
                if (correctAnswer != message)
                {
                    _notifyService.SetSend(idUser, _messagesRepository.GetTaskIncorrectAnswer());
                    _wordsRepository.SetResetAmountSuccessAnswers(conn, idUser, taskCurrent.IdWord);
                    _learnTaskRepository.SetIncreaseAmountWrongAnswer(conn, idUser, taskCurrent.IdTask);

                    if (_learnTaskService.SetSendTask(conn, idUser, taskCurrent.IdTask) == false)
                        _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetErrorCommand());
                }
                else
                {
                    _notifyService.SetSend(idUser, _messagesRepository.GetTaskCorrectAnswer());
                    _wordsRepository.SetIncreaseAmountSuccessAnswers(conn, idUser, taskCurrent.IdWord);
                    _learnTaskRepository.SetRemove(conn, idUser, taskCurrent.IdTask);

                    if (_learnTaskRepository.GetRandom(conn, idUser, out TaskToLearn task) == false)
                        _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetEmptyTasks());
                    else
                        _learnTaskService.SetSendTask(conn, idUser, task.IdTask);
                }
            }
            return true;
        }
    }
}
