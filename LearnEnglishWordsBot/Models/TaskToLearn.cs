namespace LearnEnglishWordsBot.Models
{
    public class TaskToLearn
    {
        public int IdTask { get; }
        public int IdWord { get; }
        public string InRussian { get; }
        public string InEnglish { get; }
        public int AmountWrongAnswer { get; }
        public bool IsRevers { get; }

        public TaskToLearn(int idTask, int idWord, string inRussian, string inEnglish, bool isRevers, int amountWrongAnswer)
        {
            IdTask = idTask;
            IdWord = idWord;
            InRussian = inRussian;
            InEnglish = inEnglish;
            IsRevers = isRevers;
            AmountWrongAnswer = amountWrongAnswer;
        }
    }   
}
