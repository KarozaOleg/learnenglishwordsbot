namespace LearnEnglishWordsBot.Interfaces
{
    public interface ILearnSetService
    {
        void SetSendLearnSetsInfo(int idUser);
        bool SetLearnSetStart(int idUser, int idLearSet, out string learnSetName);
        bool SetLearnSetStop(int idUser, int idLearnSet, out string learnSetName);
    }
}
