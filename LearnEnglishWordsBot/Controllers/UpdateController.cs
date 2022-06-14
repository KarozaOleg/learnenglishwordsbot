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
        const double _secondsWhileUpdateIsFresh = 20;

        readonly ILogger _logger;
        readonly IUsersRepository _usersRepository;
        readonly IMessagesRepository _messagesRepository;
        readonly INotifyService _notifyService;
        readonly ILearnTaskService _learnTaskService;
        readonly ILearnService _learnService;
        readonly ILearnSetService _learnSetService;

        public UpdateController(
            ILogger<UpdateController> logger,
            IUsersRepository usersRepository,
            IMessagesRepository messageCommandRepository,
            INotifyService notifyService,
            ILearnTaskService learnTasksService,
            ILearnService learnWordsService,
            ILearnSetService learnSetService)
        {
            _logger = logger;
            _usersRepository = usersRepository;
            _messagesRepository = messageCommandRepository;
            _notifyService = notifyService;
            _learnTaskService = learnTasksService;
            _learnService = learnWordsService;
            _learnSetService = learnSetService;
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
                _logger.LogError($"exc: {exc.Message} exc.inner: {exc.InnerException?.Message}");
                _notifyService.SetSendWithBotCommandAnswers(update.Message.Chat.Id);
            }
            return Ok();
        }

        void HandlerUpdate(Update update)
        {
            var message = update.Message;

            var isUserAlreadyExist = false;
            var idUser = _usersRepository.SetCreate(ReturnUsername(message), ref isUserAlreadyExist);
            _notifyService.SetCreateRecipient(idUser, message.Chat.Id, ref isUserAlreadyExist);

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
                            if (_learnService.SetHandleMessage(idUser, message.Text))                            
                                return;                            
                            break;

                        default:
                            _logger.LogInformation($"Don't know how work with message type: {message.Type.ToString()}");
                            break;
                    }
                    break;

                default:
                    _logger.LogInformation($"Don't know how work with update type: {update.Type.ToString()}");
                    break;
            }

            HandlerCommandUndefined(idUser, message.Text);
        }

        bool IsUpdateFresh(Update update)
        {
            var diff = DateTime.UtcNow.Subtract(update.Message.Date).TotalSeconds;
            return (diff < _secondsWhileUpdateIsFresh);
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
                    _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetStart());
                    break;

                case BotCommand.CreateTasks:
                    _learnTaskService.SetCreateTasksToLearn(idUser);
                    _learnTaskService.SetSendRandomTask(idUser);
                    break;

                case BotCommand.LearnSets:
                    _learnSetService.SetSendLearnSetsInfo(idUser);
                    break;

                case BotCommand.LearnSets_start:
                case BotCommand.LearnSets_stop:
                    if (splittedMessage.Length != 2)
                        return false;
                    if (int.TryParse(splittedMessage[1], out int idLearnSet) == false)
                        return false;
                    if (command == BotCommand.LearnSets_start)
                    {
                        if (_learnSetService.SetLearnSetStart(idUser, idLearnSet, out string learnSetName) == false)
                            _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetLearnSetAddedError(idLearnSet, learnSetName));
                        else
                            _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetLearnSetAdded(idLearnSet, learnSetName));
                    }
                    else if (command == BotCommand.LearnSets_stop)
                    {
                        if (_learnSetService.SetLearnSetStop(idUser, idLearnSet, out string learnSetName) == false)
                            _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetLearnSetRemovedError(idLearnSet, learnSetName));
                        else
                            _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetLearnSetRemoved(idLearnSet, learnSetName));
                    }
                    else
                        _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetErrorCommand());

                    break;

                default:
                    _logger.LogInformation($"Error answer to command: {command.ToString()}");
                    _notifyService.SetSendWithBotCommandAnswers(idUser, _messagesRepository.GetErrorCommand());
                    break;
            }
            return true;
        }

        void HandlerCommandUndefined(int idUser, string messageFromUser)
        {
            _logger.LogInformation($"Undefined message: {messageFromUser} from idUser: {idUser}");
            _notifyService.SetSendWithBotCommandAnswers(idUser);
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
