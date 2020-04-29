namespace LearnEnglishWordsBot.Models
{
    public class TaskToLearn
    {
        public readonly int IdTask;
        public readonly int IdWord;
        public readonly string InRussian;
        public readonly string InEnglish;
        public readonly int AmountWrongAnswer;
        public readonly bool IsRevers;

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
