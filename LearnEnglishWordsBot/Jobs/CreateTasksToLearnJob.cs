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
        readonly ILogger _logger;
        readonly ILearnTaskService _learnTaskService;
        readonly ILearnTaskRepository _learnTaskRepository;
        readonly IUsersRepository _usersRepository;
        readonly INotifyService _notifyService;
        readonly IMessagesRepository _messagesRepository;

        public CreateTasksToLearnJob(
            ILogger<CreateTasksToLearnJob> logger,
            ILearnTaskService learnTasksService,
            ILearnTaskRepository learnTaskRepository,
            IUsersRepository usersRepository,
            INotifyService notifyService,
            IMessagesRepository messagesRepository)
        {
            _logger = logger;
            _learnTaskService = learnTasksService;
            _learnTaskRepository = learnTaskRepository;
            _usersRepository = usersRepository;
            _notifyService = notifyService;
            _messagesRepository = messagesRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await Task.Run(() => CreateTasksToLearn());
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error CreateTasksToLearnJob");
            }
        }

        private void CreateTasksToLearn()
        {
            _learnTaskRepository.SetRemoveAll();

            var idUsers = _usersRepository.GetId();
            for (int i = 0; i < idUsers.Length; i++)
            {
                _notifyService.SetSend(idUsers[i], _messagesRepository.GetNewDayGreeting());

                _learnTaskService.SetCreateTasksToLearn(idUsers[i]);
                if (_learnTaskService.SetSendRandomTask(idUsers[i]) == false)
                    throw new Exception($"Error send random task for user with id: {idUsers[i]}");
            }
        }
    }
}
