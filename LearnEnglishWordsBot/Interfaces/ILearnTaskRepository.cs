using LearnEnglishWordsBot.Models;
using System.Data;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface ILearnTaskRepository
    {
        void SetCreate(IDbConnection conn, int idUser, int idWord, bool isRevers);
        void SetRemove(IDbConnection conn, int idUser, int idTask);
        void SetRemoveAll();
        bool GetTask(IDbConnection conn, int idUser, int idTask, out TaskToLearn task);
        bool GetSended(IDbConnection conn, int idUser, out TaskToLearn task);
        bool GetRandom(IDbConnection conn, int idUser, out TaskToLearn task);
        void SetIncreaseAmountWrongAnswer(IDbConnection conn, int idUser, int idTask);
        void SetMarkAsSended(IDbConnection conn, int idUser, int idTask);
    }
}
