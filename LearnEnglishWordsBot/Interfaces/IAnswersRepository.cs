using System.Collections.Generic;

namespace LearnEnglishWordsBot.Interfaces
{
    public interface IAnswersRepository
    {
        HashSet<string> GetAnswersBotCommand();
    }
}
