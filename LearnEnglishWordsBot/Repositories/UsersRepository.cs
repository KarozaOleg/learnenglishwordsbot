using Dapper;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Linq;

namespace LearnEnglishWordsBot.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private string ConnectionString { get; }
        private ILogger Logger { get; }

        public UsersRepository(ILogger<UsersRepository> logger, IOptions<DatabaseSettings> databaseOptions)
        {
            Logger = logger;
            ConnectionString = databaseOptions.Value.DefaultConnection;
        }

        public int SetCreate(string username, ref bool isUserAlreadyExist)
        {
            if (IsUserExist(username, out int idUser))
            {
                isUserAlreadyExist = true;
                return idUser;
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                var sql =
                    @"INSERT INTO 
                        public.users (username) 
                    VALUES
                        (@username) 
                    RETURNING 
                        Id";
                return conn.ExecuteScalar<int>(sql, new { username });
            }
        }

        public int[] GetIdAllUsers()
        {
            using (IDbConnection db = new NpgsqlConnection(ConnectionString))
            {
                var sql =
                    @"SELECT
                        id 
                    FROM public.users;";
                return db.Query<int>(sql).ToArray();
            }
        }

        public bool GetUsername(int idUser, out string username)
        {
            using (IDbConnection db = new NpgsqlConnection(ConnectionString))
            {
                var sql =
                    @"SELECT
                        username 
                    FROM 
                        public.users
                    WHERE
                        id = @idUser";
                username = db.Query<string>(sql, new { idUser }).FirstOrDefault();
                if (username == null)
                {
                    Logger.LogError($"Error get username for idUser: {idUser}");
                    return false;
                }
                return true;
            }
        }

        bool IsUserExist(string username, out int idUser)
        {
            idUser = 0;
            using (var db = new NpgsqlConnection(ConnectionString))
            {
                var sql =
                    @"SELECT
                        id 
                    FROM
                        public.users
                    WHERE 
                        username=@username;";

                var idUserNullable = db.Query<int?>(sql, new { username }).FirstOrDefault();
                if (idUserNullable == null)
                {
                    return false;
                }

                idUser = (int)idUserNullable;
                return true;
            }
        }
    }
}
