using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace TelegramBot.Classes
{
    class DbRequests
    {

        private static SqlConnection sqlConnection;

        public DbRequests(SqlConnection Connection)
        {
            sqlConnection = Connection;
        }

        static public string DbInsertNewUser(long Chat_Id, string UserName)
        {
            SqlDataReader dataReader = null;

            try
            {
                var commandSelect = new SqlCommand($"SELECT chat_id FROM users WHERE chat_id = {Chat_Id}", sqlConnection);
                dataReader = commandSelect.ExecuteReader();
                if (dataReader.Read())
                {
                    dataReader.Close();
                    return "Данный пользователь уже существует ";
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed) { dataReader.Close(); }
            }

            var commandINSERT = new SqlCommand($"INSERT INTO [users] (chat_id, user_name) VALUES(@chat_id, @user_name)", sqlConnection);
            if (String.IsNullOrEmpty(UserName))
                commandINSERT.Parameters.AddWithValue("user_name", DBNull.Value);
            else
                commandINSERT.Parameters.AddWithValue("user_name", UserName);

            commandINSERT.Parameters.AddWithValue("chat_id", Chat_Id);
            return "Добавлено строк в бд: " + commandINSERT.ExecuteNonQuery().ToString();

        }

        static public string DbInsertNewNode(long ChatId, string Node)
        {
            var commandINSERT = new SqlCommand($"INSERT INTO [notes] (chat_id, content, mark) VALUES(@chat_id, @content, {0})", sqlConnection);

            commandINSERT.Parameters.AddWithValue("chat_id", ChatId);
            if (String.IsNullOrEmpty(Node))
                commandINSERT.Parameters.AddWithValue("content", DBNull.Value);
            else
                commandINSERT.Parameters.AddWithValue("content", Node);
            if (commandINSERT.ExecuteNonQuery() == 1)
                return "Запись успешно добавлена.";
            else
                return "Не удалось добавить заметку.";
        }

        static public string DbShowNotes(long Chat_Id, ref int count)
        {
            SqlDataReader dataReader = null;
            List<string> notes = new List<string>();
            string str = null;
            try
            {
                var commandSelect = new SqlCommand($"SELECT content, mark FROM notes WHERE chat_id = {Chat_Id}", sqlConnection);
                dataReader = commandSelect.ExecuteReader();
                int i = 0;
                while (dataReader.Read())
                {
                    notes.Add(Convert.ToString(dataReader["content"]));
                    bool mark = Convert.ToBoolean(dataReader["mark"]);
                    if(!mark)
                    {
                        str += $"[{i + 1}] - {notes[i]}\n";
                    }
                    else
                    {
                        str += $"[{i + 1}] - **{notes[i]}** \n";
                    }
                    i++;
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex);
            }
            finally
            {
                if(dataReader != null && !dataReader.IsClosed) { dataReader.Close(); }
            }
            return str;
        }
        
        static public string DbClearNotes(long Chat_Id)
        {
            var commandDELETE = new SqlCommand($"DELETE FROM notes WHERE chat_id = {Chat_Id}", sqlConnection);
            commandDELETE.ExecuteNonQuery();
            var commandINSERT = new SqlCommand($"SELECT count(*) FROM notes WHERE chat_id = {Chat_Id}", sqlConnection);
            var a =((uint)commandINSERT.ExecuteNonQuery());
            if (a == 0) return "Не удалось очистить список заметок!";
            else return "Список заметок пуст.";            
        }

        static public bool DbMarkNote(long Chat_Id, int number)
        {
            SqlDataReader dataReader = null;
            var list_of_number = new List<int>();
            try
            {
                var commandSelect = new SqlCommand($"SELECT number FROM notes WHERE chat_id = {Chat_Id}", sqlConnection);
                dataReader = commandSelect.ExecuteReader();
                while (dataReader.Read())
                {
                    list_of_number.Add(Convert.ToInt32(dataReader["number"]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex);
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed) { dataReader.Close(); }
            }
            var commandUPDATE = new SqlCommand($"UPDATE notes SET mark = {1} WHERE number = {list_of_number[number - 1]}", sqlConnection);
            if (commandUPDATE.ExecuteNonQuery() == 1)
                return true;
            else
                return false;
        }
    }
}
