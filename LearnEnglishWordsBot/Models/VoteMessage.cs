using System.Collections.Generic;

namespace LearnEnglishWordsBot.Models
{
    public class VoteMessage
    {
        internal string Message { get; private set; }
        internal HashSet<string> Answers { get; private set; }

        internal VoteMessage(string message, HashSet<string> answers)
        {
            Message = message;
            Answers = answers;
        }
    }
}
