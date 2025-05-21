using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace NetTalk.Shared
{
    public static class RoomStorage
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

        public static List<Room> LoadRooms()
        {
            var rooms = new List<Room>();

            try
            {
                using (var conn = new MySqlConnection(MySQLString))
                {
                    conn.Open();
                    Logger.Room("Loading rooms from database...");

                    string roomQuery = "SELECT room_id, name, creator FROM rooms";

                    using (var cmd = new MySqlCommand(roomQuery, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var room = new Room
                            {
                                RoomId = reader.GetString("room_id"),
                                Name = reader.GetString("name"),
                                Creator = reader.GetString("creator"),
                                Operators = new List<string>(),
                                BannedUsers = new List<string>(),
                                JoinedUsers = new List<string>()
                            };
                            rooms.Add(room);
                            Logger.Debug($"Loaded room '{room.RoomId}' created by {room.Creator}");
                        }
                    }

                    foreach (var room in rooms)
                    {
                        room.Operators = LoadList(conn, "room_operators", room.RoomId);
                        room.BannedUsers = LoadList(conn, "room_banned", room.RoomId);
                        room.JoinedUsers = LoadList(conn, "room_joined", room.RoomId);
                        Logger.Debug($"Room '{room.RoomId}' loaded with {room.Operators.Count} operators, {room.BannedUsers.Count} banned, {room.JoinedUsers.Count} joined.");
                    }

                    Logger.Room($"Total rooms loaded: {rooms.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load rooms: {ex.Message}");
            }

            return rooms;
        }

        private static List<string> LoadList(MySqlConnection conn, string tableName, string roomId)
        {
            var result = new List<string>();
            string query = $"SELECT user_id FROM {tableName} WHERE room_id = @roomId";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@roomId", roomId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString("user_id"));
                    }
                }
            }

            return result;
        }

        public static void SaveRooms(List<Room> rooms)
        {
            try
            {
                using (var conn = new MySqlConnection(MySQLString))
                {
                    conn.Open();
                    Logger.Room("Saving rooms to database...");

                    foreach (var room in rooms)
                    {
                        string insertRoom =
                            "INSERT INTO rooms (room_id, name, creator) " +
                            "VALUES (@roomId, @name, @creator) " +
                            "ON DUPLICATE KEY UPDATE name = @name, creator = @creator";

                        using (var cmd = new MySqlCommand(insertRoom, conn))
                        {
                            cmd.Parameters.AddWithValue("@roomId", room.RoomId);
                            cmd.Parameters.AddWithValue("@name", room.Name);
                            cmd.Parameters.AddWithValue("@creator", room.Creator);
                            cmd.ExecuteNonQuery();
                        }

                        SaveList(conn, "room_operators", room.RoomId, room.Operators);
                        SaveList(conn, "room_banned", room.RoomId, room.BannedUsers);
                        SaveList(conn, "room_joined", room.RoomId, room.JoinedUsers);

                        Logger.Debug($"Saved room '{room.RoomId}' with {room.Operators.Count} ops, {room.BannedUsers.Count} bans, {room.JoinedUsers.Count} joins.");
                    }

                    Logger.Room($"Saved {rooms.Count} room(s) successfully.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save rooms: {ex.Message}");
            }
        }

        private static void SaveList(MySqlConnection conn, string tableName, string roomId, List<string> users)
        {
            string deleteQuery = $"DELETE FROM {tableName} WHERE room_id = @roomId";
            using (var delCmd = new MySqlCommand(deleteQuery, conn))
            {
                delCmd.Parameters.AddWithValue("@roomId", roomId);
                delCmd.ExecuteNonQuery();
            }

            string insertQuery = $"INSERT INTO {tableName} (room_id, user_id) VALUES (@roomId, @userId)";
            foreach (var userId in users)
            {
                using (var insCmd = new MySqlCommand(insertQuery, conn))
                {
                    insCmd.Parameters.AddWithValue("@roomId", roomId);
                    insCmd.Parameters.AddWithValue("@userId", userId);
                    insCmd.ExecuteNonQuery();
                }
            }
        }

        public static void DelEveryRoom()
        {
            try
            {
                using (var conn = new MySqlConnection(MySQLString))
                {
                    conn.Open();
                    Logger.Warn("Deleting all rooms...");

                    string[] tables = { "room_operators", "room_banned", "room_joined", "rooms", "users" };

                    foreach (var table in tables)
                    {
                        string deleteQuery = $"DELETE FROM {table}";
                        using (var cmd = new MySqlCommand(deleteQuery, conn))
                        {
                            int affected = cmd.ExecuteNonQuery();
                            Logger.Debug($"Table '{table}': {affected} rows deleted.");
                        }
                    }

                    Logger.Room("All rooms have been deleted!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Fehler beim Löschen aller Räume: {ex.Message}");
            }
        }
    }
}
