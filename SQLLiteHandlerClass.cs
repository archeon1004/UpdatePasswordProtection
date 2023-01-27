using System;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;

namespace ThreatDetectionModule
{
    internal static class SQLLiteHandlerClass
    {
        //TODO init database from code
        //private static readonly string DBFilePath = @"C:\dev\uplockout.db"; //TODO DB path from config
        private static readonly string databaseFileName = "uplockout.db";
        private static string _configuredDbPath;
        //private static readonly Logger fileLogger = new Logger(@"C:\dev\DBDebug.log"); //TODO DB log from config plus disabled by default 
        //private static Logger Logger = fileLogger;
        private static void InsertFailedLogon(string UserName)
        {
            using (var connection = new SQLiteConnection($"Data Source={_configuredDbPath}"))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "INSERT INTO PasswordChanges (UserName, Count) VALUES (@name, @count)";
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
                using (var connection = new SQLiteConnection($"Data Source ={_configuredDbPath}"))
                {
                    connection.Open();
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    //WindowsLogger.WriteWinLogEvent($"Executing Query: {Query}", EventLogEntryType.Information);
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retCounter = reader.GetInt16(0);
                                //WindowsLogger.WriteWinLogEvent($"Returned value from SQL Get Counter Query:{retCounter}", EventLogEntryType.Information);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WindowsLogger.WriteWinLogEvent($"Exception occured \n\r{ex}", EventLogEntryType.Error);
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
                using (var connection = new SQLiteConnection($"Data Source ={_configuredDbPath}"))
                {
                    connection.Open();
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    //WindowsLogger.WriteWinLogEvent($"Executing Query: {Query}", EventLogEntryType.Information);
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                currentCounter = reader.GetInt16(0);
                                //WindowsLogger.WriteWinLogEvent($"Returned value from SQL Get Counter Query:{currentCounter}", EventLogEntryType.Information);
                            }
                        }
                    }
                    if(currentCounter == -1)
                    {
                        //Logger.WriteLine($"Initial value of currentCounter. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else if(currentCounter == null)
                    {
                        //.WriteLine($"User Not present in database. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else
                    { 
                        //.WriteLine($"Current Coutner Value: {currentCounter}");
                        int newCounter = (int)currentCounter + 1; 
                        var UpdateQuery = $"UPDATE PasswordChanges Set Count = {newCounter} WHERE UserName = '{UserName}'";
                        //Logger.WriteLine($"Executing Update Query: {UpdateQuery}");
                        using (var command = new SQLiteCommand(UpdateQuery, connection))
                        {
                            var res = command.ExecuteNonQuery();
                            //Logger.WriteLine($"Executed Update query and affeced {res} rows");
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WindowsLogger.WriteWinLogEvent($"Exception \n\r{ex}", EventLogEntryType.Error);
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
        }
        internal static void CreateDatabase(string databasePath)
        {
            _configuredDbPath = databasePath;
            var dbFile = databasePath + "\\" + databaseFileName;
            try
            {
                if (!File.Exists(dbFile))
                {
                    try
                    {
                        SQLiteConnection.CreateFile(dbFile);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    string sqlCreate = @"CREATE TABLE PasswordChanges(
                                        ""UserName""	TEXT NOT NULL UNIQUE,
                                    	""Count""	INTEGER
                                        );";
                    using (var con = new SQLiteConnection($"Data Source={dbFile}"))
                    {
                        try
                        {
                            con.Open();
                            var cmd = new SQLiteCommand(con);
                            cmd.CommandText = sqlCreate;
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
