namespace LearnEnglishWordsBot.Interfaces
{
    public interface IMessagesRepository
    {
        string GetStart();
        string GetMisunderstood();
        string GetErrorCommand();
        string GetTaskCorrectAnswer();
        string GetTaskIncorrectAnswer();
        string GetEmptyTasks();
        string GetNewDayGreeting();
        string GetEmptyLearingSets();
        string GetLearnSetAdded(int idLearnSet, string learnSetName);
        string GetLearnSetAddedError(int idLearnSet, string learnSetName);
        string GetLearnSetRemoved(int idLearnSet, string learnSetName);
        string GetLearnSetRemovedError(int idLearnSet, string learnSetName);
        string GetLearnSetIsLearned(int idLearnSet, string learnSetName);
    }
}
