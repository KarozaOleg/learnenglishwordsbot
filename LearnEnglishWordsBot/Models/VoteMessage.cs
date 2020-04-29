using System.Collections.Generic;

namespace LearnEnglishWordsBot.Models
{
    public class VoteMessage
    {
        internal string _message { get; private set; }
        internal HashSet<string> Answers { get; private set; }

        internal VoteMessage(string message, HashSet<string> answers)
        {
            _message = message;
            Answers = answers;
        }
    }
}
