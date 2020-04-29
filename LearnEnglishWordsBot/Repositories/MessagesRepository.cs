using LearnEnglishWordsBot.Interfaces;

namespace LearnEnglishWordsBot.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        public string GetEmptyTasks()
        {
            return "Авeсом! :D\r\nСлова на сегодня закончились\r\nЕсли хочешь еще 10 - отправь команду /CreateTasks";
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
            return "Доброе утро!\r\nВот первое слово:\r\n";
        }

        public string GetEmptyLearingSets()
        {
            return
                "Не могу придумать слова\r\n" +
                "Не выбрано ни одного набора\r\n"+
                "Отправь команду /LearnSets, управляй реальностью!";
        }

        public string GetLearnSetIsLearned(int idLearnSet, string learnSetName)
        {
            return
                "Анбеливебл! :D\r\n" +
                $"Набор#{idLearnSet}\r\n" +
                $"Название: \"{learnSetName}\"\r\n" +
                "Тобой полностью разгадан!!1 Сомнений на счет твой не было у меня!";
        }

        public string GetLearnSetAdded(int idLearnSet, string learnSetName)
        {
            return
                $"Набор#{idLearnSet}\r\n" +
                $"Название: \"{learnSetName}\"\r\n" +
                "Успешно включен";
        }

        public string GetLearnSetAddedError(int idLearnSet, string learnSetName)
        {
            return
               $"Набор#{idLearnSet}\r\n" +
               $"Название: \"{learnSetName}\"\r\n" +
               "Ошибка при попытке включения, слеза";
        }

        public string GetLearnSetRemoved(int idLearnSet, string learnSetName)
        {
            return
                $"Набор#{idLearnSet}\r\n" +
                $"Название: \"{learnSetName}\"\r\n" +
                "Успешно выключен";
        }        

        public string GetLearnSetRemovedError(int idLearnSet, string learnSetName)
        {
            return
               $"Набор#{idLearnSet}\r\n" +
               $"Название: \"{learnSetName}\"\r\n" +
               "Ошибка при попытке отключения, слеза";
        }
    }
}
