using Dapper;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LearnEnglishWordsBot.Repositories
{
    public class WordsRepository : IWordsRepository
    {
        private const int _amountSuccessAnswers = 3;
        private string ConnectionString { get;}
        private ILogger Logger { get; }

        public WordsRepository(IOptions<DatabaseSettings> databaseOptions, ILogger<LearnSetRepository> logger)
        {
            ConnectionString = databaseOptions.Value.DefaultConnection;
            Logger = logger;
        }

        public void SetAdd(string russian, string english, out int idWord)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                if (IsWordExist(conn, english, out idWord))
                    return;

                var sql =
                    @"INSERT INTO words
                        (russian, english)
                    VALUES
                        (@russian, @english)
                    RETURNING id";

                idWord = conn.ExecuteScalar<int>(sql, new { russian, english });
            }
        }

        public int GetAmountLearnedWords(IDbConnection conn, int idUser, int idLearnSet)
        {
            var sql =
                @"SELECT
                    count(*)
                FROM
                    words_learn_set
                WHERE
                        id_learn_set = @idLearnSet 
                    AND id_word IN
                    (
                        SELECT 
                            id_word 
                        FROM words_learned 
                        WHERE 
                                id_user = @idUser 
                            AND amount_success_answers < @amountSuccessAnswers
                    )";

            var toSql = new
            {
                idUser,
                idLearnSet,
                amountSuccessAnswers = _amountSuccessAnswers
            };

            return conn.Query<int?>(sql, toSql).FirstOrDefault().Value;
        }

        public int[] GetRandomForLearnTask(IDbConnection conn, int idUser, int[] userLearingSets, int amount)
        {
            var sql =
                @"SELECT
                        id
                    FROM words
                    WHERE 
                        id IN
                        (
                            SELECT
                                id_word
                            FROM words_learn_set
                            WHERE
                                    id_learn_set = ANY(@userLearingSets)
                                AND id_word NOT IN
                                    (
                                        SELECT 
                                            id_word 
                                        FROM words_learned 
                                        WHERE 
                                                id_user = @idUser 
                                            AND amount_success_answers >= @amountSuccessAnswers
                                    )
                        )
                    ORDER BY
                        random()
                    LIMIT @amount";

            var toSql = new
            {
                idUser,
                amountSuccessAnswers = _amountSuccessAnswers,
                amount,
                userLearingSets = userLearingSets
            };

            return conn.Query<int>(sql, toSql).ToArray();
        }

        public HashSet<string> GetRandom(IDbConnection conn, bool isRevers, string wordExcept, int amount)
        {
            var answers = new List<string>();
            var randomWords = new string[0];
            if (isRevers)
                randomWords = GetRandomWordsInRussian(conn, wordExcept, amount);
            else
                randomWords = GetRandomWordsInEnglish(conn, wordExcept, amount);

            answers.AddRange(randomWords);
            answers.Add(wordExcept);

            //shuffle
            var rnd = new Random();
            return answers.OrderBy(x => rnd.Next()).ToHashSet();
        }

        private string[] GetRandomWordsInEnglish(IDbConnection conn, string exceptWord, int amout)
        {
            var sql = String.Format(
                @"SELECT
                        english
                    FROM 
                        public.words
                    WHERE
                        english != @exceptWord  
                    ORDER BY
                        random()
                    LIMIT {0}", amout);

            return conn.Query<string>(sql, new { exceptWord }).ToArray();
        }

        private string[] GetRandomWordsInRussian(IDbConnection conn, string exceptWord, int amout)
        {
            var sql = String.Format(
                @"SELECT
                        russian
                    FROM words
                    WHERE
                        russian != @exceptWord  
                    ORDER BY
                        random()
                    LIMIT {0}", amout);

            return conn.Query<string>(sql, new { exceptWord }).ToArray();
        }

        public void SetResetAmountSuccessAnswers(IDbConnection conn, int idUser, int idWord)
        {
            SetCreateWordInLearnedIfDoNotExist(conn, idUser, idWord, out bool isExist);
            if (isExist == false)
                return;

            var sql =
                @"UPDATE words_learned 
                SET amount_success_answers = 0
                WHERE 
                        id_user = @idUser 
                    AND id_word = @idWord";

            conn.Execute(sql, new { idUser, idWord });
        }

        public void SetIncreaseAmountSuccessAnswers(IDbConnection conn, int idUser, int idWord)
        {
            SetCreateWordInLearnedIfDoNotExist(conn, idUser, idWord, out bool isExist);

            var sql =
                @"UPDATE words_learned 
                SET amount_success_answers = amount_success_answers + 1 
                WHERE 
                        id_user = @idUser 
                    AND id_word = @idWord";

            conn.Execute(sql, new { idUser, idWord });
        }

        void SetCreateWordInLearnedIfDoNotExist(IDbConnection conn, int idUser, int idWord, out bool isExist)
        {
            isExist = false;

            var sql =
                @"SELECT
                    count(*)
                FROM
                    public.words_learned
                WHERE
                        id_user = @idUser 
                    AND id_word = @idWord";

            var amount = conn
                .Query<int?>(sql, new { idUser, idWord })
                .FirstOrDefault();

            if (amount == null || amount == 0)
                SetCreateWord(conn, idUser, idWord);
            else if (amount != 1)
                throw new Exception("Wrong amount words in words_learned");
            else
                isExist = true;
        }

        void SetCreateWord(IDbConnection conn, int idUser, int idWord)
        {
            var sql =
                @"INSERT INTO public.words_learned
                    (id_user, id_word)
                VALUES
                    (@idUser, @idWord)";

            conn.Execute(sql, new { idUser, idWord });
        }

        public void SetDeleteAllWords(IDbConnection conn, int idUser)
        {
            var sql =
                @"DELETE FROM words_learned 
                WHERE 
                    id_user = @idUser";

            conn.Execute(sql, new { idUser });
        }

        private bool IsWordExist(IDbConnection conn, string english, out int idWord)
        {
            idWord = default;

            var sql =
                @"SELECT
                    id
                FROM words
                WHERE
                    english = @english";

            var id = conn.Query<int?>(sql, new { english }).FirstOrDefault();
            if (id == null)
                return false;

            idWord = id.Value;
            return true;
        }
    }
}
