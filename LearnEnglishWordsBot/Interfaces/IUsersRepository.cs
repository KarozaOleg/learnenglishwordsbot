namespace LearnEnglishWordsBot.Interfaces
{
    public interface IUsersRepository
    {
        int SetCreate(string username, ref bool isPlayerAlreadyExist);
        int[] GetIdAllUsers();
        bool GetUsername(int idUser, out string username);
    }
}
