using Dapper;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Data;
using System.Linq;

namespace LearnEnglishWordsBot.Repositories
{
    public class LearnTaskRepository : ILearnTaskRepository
    {
        private readonly string _connectionString;

        public LearnTaskRepository(
            IOptions<DatabaseSettings> databaseOptions)
        {
            _connectionString = databaseOptions.Value.DefaultConnection;
        }

        public void SetCreate(IDbConnection conn, int idUser, int idWord, bool isRevers)
        {
            var sql =
                @"INSERT INTO public.taskstolearn
                    (id_user, id_word, is_revers)
                VALUES
                    (@idUser, @idWord, @isRevers)";

            var result = conn.Execute(sql, new { idUser, idWord, isRevers });
            if (result != 1)
                throw new Exception($"Error create tasks to learn for user with id: {idUser}, insert query return result is not qual 1");
        }

        public void SetRemove(IDbConnection conn, int idUser, int idTask)
        {
            var sql =
                @"DELETE FROM public.taskstolearn 
                WHERE 
                    id = @idTask";

            var result = conn.Execute(sql, new { idTask });
            if (result != 1)
                throw new Exception($"Error delete task to learn for user with id: {idUser.ToString()} and idTask: {idTask.ToString()} delete query return result is not qual 1");
        }

        public void SetRemoveAll()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var sql = @"DELETE FROM public.taskstolearn";
                conn.Execute(sql);
            }
        }

        public bool GetTask(IDbConnection conn, int idUser, int idTask, out TaskToLearn task)
        {
            var sql =
                @"SELECT
                        ttl.id AS IdTask,
                        ttl.id_word as IdWord,
                        w.russian AS InRussian,
                        w.english AS InEnglish,
                        ttl.is_revers AS IsRevers,
                        ttl.amount_wrong_answer AS amountWrongAnswer
                    FROM public.taskstolearn AS ttl
                    JOIN public.words AS w ON (w.id = ttl.id_word)                    
                    WHERE
                            ttl.id_user = @idUser
                        AND ttl.id = @idTask";

            task = conn.Query<TaskToLearn>(sql, new { idUser, idTask }).FirstOrDefault();
            if (task == null)
                return false;

            return true;
        }

        public bool GetSended(IDbConnection conn, int idUser, out TaskToLearn task)
        {
            var sql =
                @"SELECT
                        ttl.id AS IdTask,
                        ttl.id_Word as IdWord,
                        w.russian AS InRussian,
                        w.english AS InEnglish,
                        ttl.is_revers AS IsRevers,
                        ttl.amount_wrong_answer AS amountWrongAnswer
                    FROM public.taskstolearn AS ttl
                    JOIN public.words AS w ON (w.id = ttl.id_Word)                    
                    WHERE
                            ttl.id_user = @idUser
                        AND ttl.sended = true";

            task = conn.Query<TaskToLearn>(sql, new { idUser }).FirstOrDefault();
            if (task == null)
                return false;

            return true;
        }

        public bool GetRandom(IDbConnection conn, int idUser, out TaskToLearn task)
        {
            var sql =
                    @"SELECT
                        ttl.id AS IdTask,
                        ttl.id_Word as IdWord,
                        w.russian AS InRussian,
                        w.english AS InEnglish,
                        ttl.is_revers AS IsRevers,
                        ttl.amount_wrong_answer AS AmountWrongAnswer
                    FROM public.taskstolearn AS ttl
                    JOIN public.words AS w ON (w.id = ttl.id_word)
                    WHERE
                        id_user = @idUser
                    LIMIT 1";

            task = conn.Query<TaskToLearn>(sql, new { idUser }).FirstOrDefault();
            if (task == null)
                return false;

            return true;
        }

        public void SetIncreaseAmountWrongAnswer(IDbConnection conn, int idUser, int idTask)
        {
            var sql =
                @"UPDATE public.taskstolearn
                    SET amount_wrong_answer = amount_wrong_answer + 1
                    WHERE id = @idTask";

            var result = conn.Execute(sql, new { idTask });
            if (result != 1)
                throw new Exception($"Error update task amount error answer with id: {idUser.ToString()} and idTask: {idTask.ToString()} update query return result is not qual 1");
        }

        public void SetMarkAsSended(IDbConnection conn, int idUser, int idTask)
        {
            var sql =
                @"UPDATE public.taskstolearn
                SET
                    sended = true
                WHERE
                    id = @idTask";
            var result = conn.Execute(sql, new { idTask });
            if (result != 1)
                throw new Exception($"Error update task to sended for user with id: {idUser.ToString()} and idTask: {idTask.ToString()} update query return result is not qual 1");
        }        
    }
}
