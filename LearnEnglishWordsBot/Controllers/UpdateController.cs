using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = LearnEnglishWordsBot.Models.BotCommand;

namespace LearnEnglishWordsBot.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private const double SecondsWhileUpdateIsFresh = 20;
        private ILogger Logger { get; }
        private IUsersRepository UsersRepository { get; }
        private IMessagesRepository MessagesRepository { get; }
        private INotifyService NotifyService { get; }
        private ILearnTaskService LearnTaskService { get; }
        private ILearnService LearnService { get; }
        private ILearnSetService LearnSetService { get; }

        public UpdateController(ILogger<UpdateController> logger, IUsersRepository usersRepository, IMessagesRepository messageCommandRepository, INotifyService notifyService, ILearnTaskService learnTasksService, ILearnService learnWordsService, ILearnSetService learnSetService)
        {
            Logger = logger;
            UsersRepository = usersRepository;
            MessagesRepository = messageCommandRepository;
            NotifyService = notifyService;
            LearnTaskService = learnTasksService;
            LearnService = learnWordsService;
            LearnSetService = learnSetService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return StatusCode(200, "Hello world!");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            try
            {
                if (ModelState.IsValid == false)
                    throw new ArgumentException($"{nameof(update)}, model state is wrong");

                if (IsUpdateFresh(update))
                {
                    await Task.Run(() => HandlerUpdate(update));
                }
            }
            catch (Exception exc)
            {
                Logger.LogError($"exc: {exc.Message} exc.inner: {exc.InnerException?.Message}");
                NotifyService.SetSendWithBotCommandAnswers(update.Message.Chat.Id);
            }
            return Ok();
        }

        void HandlerUpdate(Update update)
        {
            var message = update.Message;

            var isUserAlreadyExist = false;
            var idUser = UsersRepository.SetCreate(ReturnUsername(message), ref isUserAlreadyExist);
            NotifyService.SetCreateRecipient(idUser, message.Chat.Id, ref isUserAlreadyExist);

            /*
            if (isUserAlreadyExist == false)
            {
                _gamePlayersRepository.SetFindNewPlayers();
            }
            */

            switch (update.Type)
            {
                case UpdateType.Message:
                    switch (message.Type)
                    {
                        case MessageType.Text:
                            if (HandlerCommand(idUser, message.Text))                            
                                return;                            
                            if (LearnService.SetHandleMessage(idUser, message.Text))                            
                                return;                            
                            break;

                        default:
                            Logger.LogInformation($"Don't know how work with message type: {message.Type.ToString()}");
                            break;
                    }
                    break;

                default:
                    Logger.LogInformation($"Don't know how work with update type: {update.Type.ToString()}");
                    break;
            }

            HandlerCommandUndefined(idUser, message.Text);
        }

        bool IsUpdateFresh(Update update)
        {
            var diff = DateTime.UtcNow.Subtract(update.Message.Date).TotalSeconds;
            return (diff < SecondsWhileUpdateIsFresh);
        }

        bool HandlerCommand(int idUser, string message)
        {
            var splittedMessage = message.Split(' ');
            if(splittedMessage.Length > 1)            
                message = splittedMessage[0];
            
            message = message.Trim(new char[] { '/', '.', '*' });
            if (ParserEnum.Parse(message, out BotCommand command) == false)
                return false;

            switch (command)
            {
                case BotCommand.Start:
                    NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetStart());
                    break;

                case BotCommand.CreateTasks:
                    LearnTaskService.SetCreateTasksToLearn(idUser);
                    LearnTaskService.SetSendRandomTask(idUser);
                    break;

                case BotCommand.LearnSets:
                    LearnSetService.SetSendLearnSetsInfo(idUser);
                    break;

                case BotCommand.LearnSets_start:
                case BotCommand.LearnSets_stop:
                    if (splittedMessage.Length != 2)
                        return false;
                    if (int.TryParse(splittedMessage[1], out int idLearnSet) == false)
                        return false;
                    if (command == BotCommand.LearnSets_start)
                    {
                        if (LearnSetService.SetLearnSetStart(idUser, idLearnSet, out string learnSetName) == false)
                            NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetLearnSetAddedError(idLearnSet, learnSetName));
                        else
                            NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetLearnSetAdded(idLearnSet, learnSetName));
                    }
                    else if (command == BotCommand.LearnSets_stop)
                    {
                        if (LearnSetService.SetLearnSetStop(idUser, idLearnSet, out string learnSetName) == false)
                            NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetLearnSetRemovedError(idLearnSet, learnSetName));
                        else
                            NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetLearnSetRemoved(idLearnSet, learnSetName));
                    }
                    else
                        NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetErrorCommand());

                    break;

                default:
                    Logger.LogInformation($"Error answer to command: {command.ToString()}");
                    NotifyService.SetSendWithBotCommandAnswers(idUser, MessagesRepository.GetErrorCommand());
                    break;
            }
            return true;
        }

        void HandlerCommandUndefined(int idUser, string messageFromUser)
        {
            Logger.LogInformation($"Undefined message: {messageFromUser} from idUser: {idUser}");
            NotifyService.SetSendWithBotCommandAnswers(idUser);
        }

        string ReturnUsername(Message message)
        {
            if (message.From.Username != null)
            {
                return message.From.Username;
            }
            if (message.From.FirstName != null)
            {
                return message.From.FirstName;
            }
            if (message.From.LastName != null)
            {
                return message.From.LastName;
            }
            if (message.Chat.Username != null)
            {
                return message.Chat.Username;
            }
            if (message.Chat.FirstName != null)
            {
                return message.Chat.FirstName;
            }
            if (message.Chat.LastName != null)
            {
                return message.Chat.LastName;
            }

            throw new NullReferenceException("username");
        }
    }
}
