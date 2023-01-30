using System;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;

namespace ThreatDetectionModule
{
    internal static class SQLLiteHandlerClass
    {
        //TODO introduce timestamp and interval for checing the time for timeout
        private static readonly string databaseFileName = "uplockout.db";
        private static string _configuredDbPath;
        private static void InsertFailedLogon(string UserName)
        {
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:InsertFailedLogon: Enter");
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:InsertFailedLogon.UserName: {UserName}");
            using (var connection = new SQLiteConnection($"Data Source = {_configuredDbPath}\\{databaseFileName}"))
            {
                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:InsertFailedLogon.Connecting to DB.Connection string:Data Source={_configuredDbPath}\\{databaseFileName}");
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "INSERT INTO PasswordChanges (UserName, Count) VALUES (@name, @count)";
                    command.Parameters.AddWithValue("@name", UserName);
                    command.Parameters.AddWithValue("@count", 1);
                    Debug.WriteLine($"ADFSUPPlugin:SQLHandler:InsertFailedLogon.executing query: {command.CommandText}");
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Debug.WriteLine($"SQLHandler:InsertFailedLogon: Exit");
        }
        internal static int CheckCounter(string UserName)
        {
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Enter");
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: UserName: {UserName}");
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Database path: {_configuredDbPath}\\{databaseFileName}");
            int retCounter = -1;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source = {_configuredDbPath}\\{databaseFileName}"))
                {
                    connection.Open();
                    Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Connecting to DB. Connection string: Data Source = {_configuredDbPath}\\{databaseFileName}");
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Query: SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}' ");
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retCounter = reader.GetInt16(0);
                                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Value Returned: {retCounter}");
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Exception caugth.");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException);
                Debug.WriteLine(ex.StackTrace);
                WindowsLogger.WriteWinLogEvent($"Exception occured \n\r{ex}", EventLogEntryType.Error);               
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CheckCounter: Exit");
            return retCounter;
        }
        internal static void IncreaseCounter(string UserName)
        {
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter: Enter");
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter: UserName: {UserName}");
            int? currentCounter = -1;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source = {_configuredDbPath}\\{databaseFileName}"))
                {
                    Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter: Connecting to DB: Data Source = {_configuredDbPath}\\{databaseFileName}");
                    connection.Open();
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter: Executing Query: {Query}");
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                currentCounter = reader.GetInt16(0);
                                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Returned value from SQL Get Counter Query:{currentCounter}");
                            }
                        }
                    }
                    if(currentCounter == -1)
                    {
                        Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Initial value of currentCounter. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else if(currentCounter == null)
                    {
                        Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:User Not present in database. Will create entry");
                        InsertFailedLogon(UserName);
                    }
                    else
                    {
                        Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Current Coutner Value: {currentCounter}");
                        int newCounter = (int)currentCounter + 1; 
                        var UpdateQuery = $"UPDATE PasswordChanges Set Count = {newCounter} WHERE UserName = '{UserName}'";
                        Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Executing Update Query: {UpdateQuery}");
                        using (var command = new SQLiteCommand(UpdateQuery, connection))
                        {
                            var res = command.ExecuteNonQuery();
                            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Executed Update query and affeced {res} rows");
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter:Excetpion caught.");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException);
                Debug.WriteLine(ex.StackTrace);
                WindowsLogger.WriteWinLogEvent($"Exception \n\r{ex}", EventLogEntryType.Error);
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:IncreaseCounter: Exit");
        }
        internal static void CreateDatabase(string databasePath)
        {
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Enter");
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: databasePath: {databasePath}");
            _configuredDbPath = databasePath;
            var dbFile = databasePath + "\\" + databaseFileName;
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: dbFile: {dbFile}");
            try
            {
                if (!File.Exists(dbFile))
                {
                    try
                    {
                        SQLiteConnection.CreateFile(dbFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Exception caught");
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.InnerException);
                        Debug.WriteLine(ex.StackTrace);
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
                            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Executing Query - {sqlCreate}");
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Exception caught");
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(ex.InnerException);
                            Debug.WriteLine(ex.StackTrace);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Exception caught");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException);
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
            Debug.WriteLine($"ADFSUPPlugin:SQLHandler:CreateDatabase: Exit");
        }
    }
}
