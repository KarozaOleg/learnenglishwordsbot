using LearnEnglishWordsBot.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace LearnEnglishWordsBot.Jobs
{
    [DisallowConcurrentExecution]
    public class CreateTasksToLearnJob : IJob
    {
        private ILogger Logger { get; }
        private ILearnTaskService LearnTaskService { get; }
        private ILearnTaskRepository LearnTaskRepository { get; }
        private IUsersRepository UsersRepository { get; }
        private INotifyService NotifyService { get; }
        private IMessagesRepository MessagesRepository { get; }

        public CreateTasksToLearnJob(ILogger<CreateTasksToLearnJob> logger, ILearnTaskService learnTasksService, ILearnTaskRepository learnTaskRepository, IUsersRepository usersRepository, INotifyService notifyService, IMessagesRepository messagesRepository)
        {
            Logger = logger;
            LearnTaskService = learnTasksService;
            LearnTaskRepository = learnTaskRepository;
            UsersRepository = usersRepository;
            NotifyService = notifyService;
            MessagesRepository = messagesRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await Task.Factory.StartNew(CreateTasksToLearn);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Error CreateTasksToLearnJob");
            }
        }

        private void CreateTasksToLearn()
        {
            LearnTaskRepository.SetRemoveAll();

            var idUsers = UsersRepository.GetIdAllUsers();
            foreach (var idUser in idUsers)
            {
                NotifyService.SetSend(idUser, MessagesRepository.GetNewDayGreeting());

                LearnTaskService.SetCreateTasksToLearn(idUser);
                if (LearnTaskService.SetSendRandomTask(idUser) == false)
                    throw new Exception($"Error send random task for user with id: {idUser}");
            }
        }
    }
}
