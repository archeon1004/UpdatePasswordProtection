using System;
using System.Data.SQLite;

namespace UpdatePasswordPluginModule
{
    internal static class SQLHelper
    {
        private static readonly string databaseFileName = "uplockout.db";
        internal static void ResetUserCounter(string UserName,bool forcereset)
        {
            ConfigHelper config = new ConfigHelper();
            int retCounter = -1;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source = {config.DatabaseFilePath}\\{databaseFileName}"))
                {
                    connection.Open();
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retCounter = reader.GetInt16(0);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
            if(retCounter >= config.RequestThreshold || forcereset == true)
            {
                try
                {
                    using (var connection = new SQLiteConnection($"Data Source = {config.DatabaseFilePath}\\{databaseFileName}"))
                    {
                        connection.Open();
                        //var Query = $"SELECT Counter FROM PasswordChanges WHERE Username = '{UserName}'";
                        var Query = $"DELETE from PasswordChanges WHERE UserName = '{UserName}'";
                        using (var command = new SQLiteCommand(Query, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    retCounter = reader.GetInt16(0);
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
        internal static int GetUserCounter(string UserName)
        {
            ConfigHelper config = new ConfigHelper();
            int retCounter = -1;
            try
            {
                using (var connection = new SQLiteConnection($"Data Source = {config.DatabaseFilePath}\\{databaseFileName}"))
                {
                    connection.Open();
                    var Query = $"SELECT Count FROM PasswordChanges WHERE UserName = '{UserName}'";
                    using (var command = new SQLiteCommand(Query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retCounter = reader.GetInt16(0);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                var e = new Exception(ex.Message, ex.InnerException);
                throw e;
            }
            return retCounter;
        }
    }
}
