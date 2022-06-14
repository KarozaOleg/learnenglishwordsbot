using LearnEnglishWordsBot.Interfaces;
using System;

namespace LearnEnglishWordsBot.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        public string GetEmptyTasks()
        {
            return $"Авeсом! :D{Environment.NewLine}Слова на сегодня закончились{Environment.NewLine}Если хочешь еще 10 - отправь команду{Environment.NewLine}/CreateTasks";
        }

        public string GetErrorCommand()
        {
            return "10101011101 ошибка!";
        }

        public string GetMisunderstood()
        {
            return "Между нами возникло недопонимание, будет лучше если ты выберешь одну из команд";
        }

        public string GetStart()
        {
            return "Каждый день буду придумывать специально для тебя 10 новых слов которые ты должен будешь выучить";
        }

        public string GetTaskCorrectAnswer()
        {
            return "Правильно!";
        }

        public string GetTaskIncorrectAnswer()
        {
            return "Попробуй еще раз";
        }

        public string GetNewDayGreeting()
        {
            return $"Доброе утро!{Environment.NewLine}Вот первое слово:{Environment.NewLine}";
        }

        public string GetEmptyLearingSets()
        {
            return
                $"Не могу придумать слова{Environment.NewLine}" +
                $"Не выбрано ни одного набора{Environment.NewLine}" +
                "Отправь команду /LearnSets, управляй реальностью!";
        }

        public string GetLearnSetIsLearned(int idLearnSet, string learnSetName)
        {
            return
                $"Анбеливебл! :D{Environment.NewLine}" +
                $"Набор#{idLearnSet}{Environment.NewLine}" +
                $"Название: \"{learnSetName}\"{Environment.NewLine}" +
                "Тобой полностью разгадан!!1 Сомнений на счет твой не было у меня!";
        }

        public string GetLearnSetAdded(int idLearnSet, string learnSetName)
        {
            return
                $"Набор#{idLearnSet}{Environment.NewLine}" +
                $"Название: \"{learnSetName}\"{Environment.NewLine}" +
                "Успешно включен";
        }

        public string GetLearnSetAddedError(int idLearnSet, string learnSetName)
        {
            return
               $"Набор#{idLearnSet}{Environment.NewLine}" +
               $"Название: \"{learnSetName}\"{Environment.NewLine}" +
               "Ошибка при попытке включения, слеза";
        }

        public string GetLearnSetRemoved(int idLearnSet, string learnSetName)
        {
            return
                $"Набор#{idLearnSet}{Environment.NewLine}" +
                $"Название: \"{learnSetName}\"{Environment.NewLine}" +
                "Успешно выключен";
        }        

        public string GetLearnSetRemovedError(int idLearnSet, string learnSetName)
        {
            return
               $"Набор#{idLearnSet}{Environment.NewLine}" +
               $"Название: \"{learnSetName}\"{Environment.NewLine}" +
               "Ошибка при попытке отключения, слеза";
        }
    }
}
