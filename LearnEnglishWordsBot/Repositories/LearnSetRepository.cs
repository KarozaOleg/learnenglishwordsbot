using Dapper;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Data;
using System.Linq;

namespace LearnEnglishWordsBot.Repositories
{
    public class LearnSetRepository : ILearnSetRepository
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        readonly IWordsRepository _wordsRepository;

        public LearnSetRepository(
               IOptions<DatabaseSettings> databaseOptions,
               ILogger<LearnSetRepository> logger,
               IWordsRepository wordsRepository)
        {
            _connectionString = databaseOptions.Value.DefaultConnection;
            _logger = logger;
            _wordsRepository = wordsRepository;
        }

        public bool SetAddToLearning(int idUser, int idLearnSet)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                RemoveAllWordsFromLearned(conn, idUser, idLearnSet);

                var sql =
                @"INSERT INTO user_learning_set
                    (id_user, id_learn_set)
                VALUES
                    (@idUser, @idLearnSet)";

                var result = conn.Execute(sql, new { idUser, idLearnSet });
                if (result != 1)
                {
                    _logger.LogError($"Error SetAddLearnSetToLearning for user with id:{idUser} and learnSet id:{idLearnSet}, insert query return result is not qual 1");
                    return false;
                }
                return true;
            }
        }

        public void SetRemoveFromLearning(int idUser, int idLearnSet)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
                RemoveFromLearning(conn, idUser, idLearnSet);
        }

        public void SetAdd(string name, out int idLearnSet)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var sql =
                @"INSERT INTO learn_set
                    (name)
                VALUES
                    (@name)
                RETURNING id";

                idLearnSet = conn.ExecuteScalar<int>(sql, new { name });
            }
        }

        public void SetAddWord(int idWord, int idLearnSet, out bool isAlreasyExist)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                isAlreasyExist = IsWordExist(conn, idWord, idLearnSet);
                if (isAlreasyExist)
                    return;

                var sql =
                @"INSERT INTO words_learn_set
                    (id_word, id_learn_set)
                VALUES
                    (@idWord, @idLearnSet)";

                var result = conn.Execute(sql, new { idWord, idLearnSet });
                if (result != 1)
                    throw new Exception($"Error SetAddWord for word with id:{idWord} and learnSet id:{idLearnSet}, insert query return result is not qual 1");
            }
        }

        private bool IsWordExist(IDbConnection conn, int idWord, int idLearnSet)
        {
            var sql =
           @"SELECT
                    count(*)
                FROM words_learn_set
                WHERE
                        id_word = @idWord
                    AND id_learn_set = @idLearnSet";

            var amount = conn.Query<int>(sql, new { idWord, idLearnSet }).FirstOrDefault();
            return amount > 0;
        }

        public (int Id, string Name, int AmountWords)[] GetAll(IDbConnection conn)
        {
            var sql =
                @"SELECT
                    ls.id,
                    ls.name,
                    (
                        SELECT 
                            count(*) 
                        FROM words_learn_set 
                        WHERE 
                            id_learn_set = ls.id
                    ) as amountWords
                FROM learn_set AS ls";

            return conn.Query<(int, string, int)>(sql).ToArray();
        }

        public bool GetIsExist(int idLearnSet)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var sql =
               @"SELECT
                    count(*)
                FROM learn_set
                WHERE
                    id = @idLearnSet";

                var amount = conn.Query<int>(sql, new { idLearnSet }).FirstOrDefault();
                return amount > 0;
            }
        }

        public bool GetIsLearned(IDbConnection conn, int idUser, int idLearnSet)
        {
            var wordsToLearn = _wordsRepository.GetRandomForLearnTask(conn, idUser, new int[] { idLearnSet }, 1);            
            var isLearned = wordsToLearn.Length < 1;
            if (isLearned)
            {
                AddToLearned(conn, idUser, idLearnSet);
                RemoveFromLearning(conn, idUser, idLearnSet);
            }

            return isLearned;
        }        

        public (int Id, string Name)[] GetForUser(IDbConnection conn, int idUser)
        {
            var sql =
                @"SELECT
                    id_learn_set as id,
                    ls.name
                FROM user_learning_set AS uls
                JOIN learn_set AS ls ON (ls.id = uls.id_learn_set)
                WHERE
                    uls.id_user = @idUser";

            return conn.Query<(int, string)>(sql, new { idUser }).ToArray();
        }

        public bool GetName(int idLearnSet, out string name)
        {
            name = "error";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var sql =
               @"SELECT
                    name
                FROM learn_set
                WHERE
                    id = @idLearnSet";

                name = conn.Query<string>(sql, new { idLearnSet }).FirstOrDefault();
                return name != null;
            }
        }

        public void GetAmountLearned(int idUser, int idLearnSet, out int amount)
        {
            amount = 0;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                if (ReturnIdLearnedSet(conn, idUser, idLearnSet, out int idLearnedSet) == false)
                    return;

                var sql =
                @"SELECT
                    amount
                FROM user_learned_set
                WHERE
                        id_user = @idUser 
                    AND id_learn_set = @idLearnSet";

                amount = conn.Query<int>(sql, new { idUser, idLearnSet }).FirstOrDefault();
            }
        }

        private void RemoveFromLearning(IDbConnection conn, int idUser, int idLearnSet)
        {
            var sql =
                @"DELETE FROM user_learning_set
                WHERE 
                        id_user = @idUser
                    AND id_learn_set = @idLearnSet";

            var result = conn.Execute(sql, new { idUser, idLearnSet });
            if (result != 1)
                throw new Exception($"Error delete learn set for user with id: {idUser.ToString()} and learnSet with id: {idLearnSet.ToString()} delete query return result is not qual 1");
        }

        private void RemoveAllWordsFromLearned(IDbConnection conn, int idUser, int idLearnSet)
        {
            var sql =
              @"DELETE FROM words_learned
                WHERE 
                        id_user = @idUser
                    AND id_word IN 
                    (
                        SELECT
                            id_word
                        FROM words_learn_set
                        WHERE
                            id_learn_set = @idLearnSet
                    )";

            conn.Execute(sql, new { idUser, idLearnSet });
        }

        private void AddToLearned(IDbConnection conn, int idUser, int idLearnSet)
        {
            if (ReturnIdLearnedSet(conn, idUser, idLearnSet, out int idLearnedSet) == false)
                CreateLearnedSet(conn, idUser, idLearnSet, out idLearnedSet);

            IncreaseAmountLearnedSet(conn, idUser, idLearnedSet);
        }        

        private void IncreaseAmountLearnedSet(IDbConnection conn, int idUser, int idLearnedSet)
        {
            var sql =
                @"UPDATE user_learned_set 
                SET
                    amount = amount + 1
                WHERE 
                        id_user = @idUser 
                    AND id = @idLearnedSet";
            conn.Execute(sql, new { idUser, idLearnedSet });
        }

        private bool ReturnIdLearnedSet(IDbConnection conn, int idUser, int idLearnSet, out int idLearnedSet)
        {
            idLearnedSet = default;

            var sql =
                @"SELECT
                    id
                FROM user_learned_set
                WHERE
                        id_user = @idUser 
                    AND id_learn_set = @idLearnSet";

            var id = conn.Query<int?>(sql, new { idUser, idLearnSet }).FirstOrDefault();
            if (id == null)
                return false;

            idLearnedSet = id.Value;
            return true;
        }

        private void CreateLearnedSet(IDbConnection conn, int idUser, int idLearnSet, out int idLearnedSet)
        {
            var sql =
                @"INSERT INTO user_learned_set
                    (id_user, id_learn_set, amount)
                VALUES
                    (@idUser, @idLearnSet, 0) 
                RETURNING id";
            
            idLearnedSet = conn.ExecuteScalar<int>(sql, new { idUser, idLearnSet });
        }
    }
}
