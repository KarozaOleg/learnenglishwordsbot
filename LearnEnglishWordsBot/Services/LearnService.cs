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
        private string ConnectionString { get; }
        private ILogger Logger { get; }
        private INotifyService NotifyService { get; }
        private IMessagesRepository MessagesRepository { get; }
        private ILearnTaskService LearnTaskService { get; }
        private ILearnTaskRepository LearnTaskRepository;
        private IWordsRepository WordsRepository { get; }

        public LearnService(ILogger<LearnService> logger, INotifyService notifyService, IOptions<DatabaseSettings> databaseOptions, IMessagesRepository messagesRepository, ILearnTaskService learnTasksService, ILearnTaskRepository learnTaskRepository, IWordsRepository wordsRepository)
        {
            Logger = logger;
            NotifyService = notifyService;
            ConnectionString = databaseOptions.Value.DefaultConnection;
            MessagesRepository = messagesRepository;
            LearnTaskService = learnTasksService;
            LearnTaskRepository = learnTaskRepository;
            WordsRepository = wordsRepository;
        }

        public bool SetHandleMessage(int idUser, string message)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                if (LearnTaskRepository.GetSended(conn, idUser, out TaskToLearn taskCurrent) == false)
                    return false;

                var correctAnswer = taskCurrent.IsRevers ? taskCurrent.InRussian : taskCurrent.InEnglish;
                if (correctAnswer != message)
                {
                    NotifyService.SetSend(idUser, MessagesRepository.GetTaskIncorrectAnswer());
                    WordsRepository.SetResetAmountSuccessAnswers(conn, idUser, taskCurrent.IdWord);
                    LearnTaskRepository.SetIncreaseAmountWrongAnswer(conn, idUser, taskCurrent.IdTask);

                    if (LearnTaskService.SetSendTask(conn, idUser, taskCurrent.IdTask) == false)
                        NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetErrorCommand());
                }
                else
                {
                    NotifyService.SetSend(idUser, MessagesRepository.GetTaskCorrectAnswer());
                    WordsRepository.SetIncreaseAmountSuccessAnswers(conn, idUser, taskCurrent.IdWord);
                    LearnTaskRepository.SetRemove(conn, idUser, taskCurrent.IdTask);

                    if (LearnTaskRepository.GetRandom(conn, idUser, out TaskToLearn task) == false)
                        NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetEmptyTasks());
                    else
                        LearnTaskService.SetSendTask(conn, idUser, task.IdTask);
                }
            }
            return true;
        }
    }
}
