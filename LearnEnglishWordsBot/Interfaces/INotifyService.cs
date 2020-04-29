using LearnEnglishWordsBot.Models;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface INotifyService
    {
        void SetSend(int idUser, string message);
        void SetSend(int idUser, VoteMessage message);
        void SetSend(long idChat, VoteMessage message);
        void SetSendWithBotCommandAnswers(int idUser, string message = "");
        void SetSendWithBotCommandAnswers(long IdChat);

        void SetCreateRecipient(int idUser, long idChat, ref bool isPlayerAlreadyExist);
    }
}
