using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using TelegramBot.Classes;

namespace TelegramBot
{
    class Program
    {
        static private SqlConnection sqlConnection = null;
        static void Main(string[] args)
        {
            //Создаем подключение к бд
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TelegramBoteNotes"].ConnectionString);
            sqlConnection.Open(); 
            if (sqlConnection.State == ConnectionState.Open) 
            { 
                Console.WriteLine("Подключене к DB установлено");
                DbRequests dbRequests = new DbRequests(sqlConnection);
            }
            
            try
            {
                TelegramBotMyNotes bot = new TelegramBotMyNotes(token : "5208534938:AAGB7WXJl8HQ1TK67jF36m-SP0ESghtvOYg");
                bot.GetUpdates();
                sqlConnection.Close();
                if (sqlConnection.State == ConnectionState.Closed) { Console.WriteLine("Подключение к DB закрыто"); }
            }
            catch(Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
