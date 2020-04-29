using System.Data;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface ILearnTaskService
    {
        void SetCreateTasksToLearn(int idUser);
        bool SetSendRandomTask(int idUser);
        bool SetSendTask(IDbConnection conn, int idUser, int idTask);
    }
}
