using System.Collections.Generic;
using System.Data;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface IWordsRepository
    {
        void SetAdd(string russian, string english, out int idWord);
        int GetAmountLearnedWords(IDbConnection conn, int idUser, int idLearnSet);
        int[] GetRandomForLearnTask(IDbConnection conn, int idUser, int[] userLearingSets, int amount);
        HashSet<string> GetRandom(IDbConnection conn, bool isRevers, string wordExcept, int amount);
        void SetResetAmountSuccessAnswers(IDbConnection conn, int idUser, int idWord);
        void SetIncreaseAmountSuccessAnswers(IDbConnection conn, int idUser, int idWord);
        void SetDeleteAllWords(IDbConnection conn, int idUser);
    }
}
