using System;
using System.Data.SQLite;

namespace ThreatDetectionModule
{
    internal static class SQLLiteHandlerClass
    {
        private static readonly string DBFilePath = @"C:\dev\uplockout.db";
        private static readonly Logger fileLogger = new Logger(@"C:\dev\DBDebug.log");
        private static Logger Logger = fileLogger;
        private static void InsertFailedLogon(string UserName)
        {
            using (var connection = new SQLiteConnection($"Data Source={DBFilePath}"))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "INSERT INTO PasswordChanges (UserName, Counter) VALUES (@name, @count)";
                    command.Parameters.AddWithValue("@name", UserName);
                    command.Parameters.AddWithValue("@count", 1);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        internal static int CheckCounter(string UserName)
        {
            int retCounter = 0;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source ={DBFilePath}"))
                {
                    connection.Open();
                    var Query = $"SELECT Counter FROM PasswordChanges WHERE Username = '{UserName}'";
                    Logger.WriteLine($"Executing Query: {Query}");
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retCounter = reader.GetInt16(0);
                                Logger.WriteLine($"Returned value from SQL Get Counter Query:{retCounter}");
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Exception \n\r{ex}");
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
            return retCounter;
        }
        internal static void IncreaseCounter(string UserName)
        {
            int? currentCounter = -1;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source ={DBFilePath}"))
                {
                    connection.Open();
                    var Query = $"SELECT Counter FROM PasswordChanges WHERE Username = '{UserName}'";
                    Logger.WriteLine($"Executing Query: {Query}");
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                currentCounter = reader.GetInt16(0);
                                Logger.WriteLine($"Returned value from SQL Get Counter Query:{currentCounter}");
                            }
                        }
                    }
                    if(currentCounter == -1)
                    {
                        Logger.WriteLine($"Initial value of currentCounter. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else if(currentCounter == null)
                    {
                        Logger.WriteLine($"User Not present in database. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else
                    { 
                        Logger.WriteLine($"Current Coutner Value: {currentCounter}");
                        int newCounter = (int)currentCounter + 1; 
                        var UpdateQuery = $"UPDATE PasswordChanges Set Counter = {newCounter} WHERE Username = '{UserName}'";
                        Logger.WriteLine($"Executing Update Query: {UpdateQuery}");
                        using (var command = new SQLiteCommand(UpdateQuery, connection))
                        {
                            var res = command.ExecuteNonQuery();
                            Logger.WriteLine($"Executed Update query and affeced {res} rows");
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Exception \n\r{ex}");
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
        }
    }
}
