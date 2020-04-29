using System.Data;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface ILearnSetRepository
    {
        bool SetAddToLearning(int idUser, int idLearnSet);
        void SetRemoveFromLearning(int idUser, int idLearnSet);
        void SetAdd(string name, out int idLearnSet);
        void SetAddWord(int idWord, int idLearnSet, out bool isAlreasyExist);
        (int Id, string Name, int AmountWords)[] GetAll(IDbConnection conn);
        (int Id, string Name)[] GetForUser(IDbConnection conn, int idUser);
        bool GetIsLearned(IDbConnection conn, int idUser, int idLearnSet);
        bool GetName(int idLearnSet, out string name);
        bool GetIsExist(int idLearSet);
        void GetAmountLearned(int idUser, int idLearnSet, out int amount);
    }
}
