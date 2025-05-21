using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace NetTalk.Shared
{
    public static class UserStorage
    {
        private static readonly string AppSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        private static readonly string MySQLString = GetConnectionString();

        private static string GetConnectionString()
        {
            if (!File.Exists(AppSettingsPath))
            {
                var defaultConfig =
                    "{\n" +
                    "  \"ConnectionStrings\": {\n" +
                    "    \"NetTalkDB\": \"server=127.0.0.1;port=3306;user=root;password=;database=nettalk\"\n" +
                    "  }\n" +
                    "}";

                File.WriteAllText(AppSettingsPath, defaultConfig);
            }

            var config = new ConfigurationBuilder()
                .AddJsonFile(AppSettingsPath, optional: false)
                .Build();

            var connStr = config.GetConnectionString("NetTalkDB");
            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("Connection string not found in configuration.");
            }

            return connStr;
        }

        public static Dictionary<string, User> LoadUsers()
        {
            var users = new Dictionary<string, User>();

            try
            {
                using (var conn = new MySqlConnection(MySQLString))
                {
                    conn.Open();
                    Logger.Info("Connected to MySQL to load users.");

                    string query = "SELECT id, username, password_hash FROM users";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetString("id"),
                                Username = reader.GetString("username"),
                                PasswordHash = reader.GetString("password_hash")
                            };

                            users[user.Id] = user;
                            Logger.Debug($"Loaded user: {user.Username} (ID: {user.Id})");
                        }
                    }

                    Logger.Info($"Total users loaded: {users.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load users from database: {ex.Message}");
            }

            return users;
        }

        public static void SaveUsers(Dictionary<string, User> users)
        {
            try
            {
                using (var conn = new MySqlConnection(MySQLString))
                {
                    conn.Open();
                    Logger.Info("Connected to MySQL to save users.");

                    foreach (var user in users.Values)
                    {
                        string insert =
                            "INSERT INTO users (id, username, password_hash) " +
                            "VALUES (@id, @username, @password_hash) " +
                            "ON DUPLICATE KEY UPDATE " +
                            "username = @username, password_hash = @password_hash";

                        using (var cmd = new MySqlCommand(insert, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", user.Id);
                            cmd.Parameters.AddWithValue("@username", user.Username);
                            cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);

                            cmd.ExecuteNonQuery();
                            Logger.Debug($"Saved user: {user.Username} (ID: {user.Id})");
                        }
                    }

                    Logger.Info("All users saved successfully.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save users to database: {ex.Message}");
            }
        }
    }
}
