using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnEnglishWordsBot.Repositories
{
    public class AnswersRepository : IAnswersRepository
    {
        public HashSet<string> GetAnswersBotCommand()
        {
            var botCommands = new HashSet<string>();
            foreach (BotCommand command in Enum.GetValues(typeof(BotCommand)))
            {
                var cstr = command.ToString();
                if (cstr.Contains("start") || cstr.Contains("stop") || cstr.Contains("reset"))
                    continue;

                botCommands.Add($"/{ command.ToString()}");
            }
            return botCommands;
        }
    }
}
