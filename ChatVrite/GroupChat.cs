using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ChatVrite
{
    internal class GroupChat
    {
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupChatId { get; set; }
        public List<User> ChatUsers { get; set; }
        public List<GroupChat> GroupChats { get; set; }

        public GroupChat()
        {
            GroupChats = new List<GroupChat>();
        }

        public List<GroupChat> GetAllGroupChats(int UserId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT gc.GroupChatId, gc.Name, gc.Description FROM GroupChats gc INNER JOIN GroupChatMembers gcm ON gc.GroupChatId = gcm.GroupChatId WHERE gcm.UserId = {UserId}", connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Очистка списка перед загрузкой новых элементов
                            GroupChats.Clear();
                            while (reader.Read())
                            {
                                GroupChat groupchat = new GroupChat()
                                {
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    GroupChatId = Convert.ToInt32(reader["GroupChatId"])
                                };
                                groupchat.ChatUsers = GetUsersIdGroupChatByGroupChatId(groupchat.GroupChatId);
                                GroupChats.Add(groupchat);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine(ex.Message);
            }

            return GroupChats;
        }

        public List<User> GetUsersIdGroupChatByGroupChatId(int chatId)
        {
            List<User> users = new List<User>();
            Random random = new Random();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM `GroupChatMembers` WHERE `GroupChatId` = @ChatId", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                UserID = Convert.ToInt32(reader["UserId"])
                             };
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        public List<GroupChat> GetChatsByName(string name, int userId)
        {
            List<GroupChat> groupChatByName = new List<GroupChat>();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = "SELECT gc.* FROM GroupChats gc " +
                               "INNER JOIN GroupChatMembers gcm ON gc.GroupChatId = gcm.GroupChatId " +
                               "WHERE gc.Name LIKE @SearchName AND gcm.UserId = @UserId";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchName", "%" + name + "%");
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupChat groupchat = new GroupChat()
                            {
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                GroupChatId = Convert.ToInt32(reader["GroupChatId"])
                            };
                            groupchat.ChatUsers = GetUsersIdGroupChatByGroupChatId(groupchat.GroupChatId);
                            groupChatByName.Add(groupchat);
                        }
                    }
                }
            }
            return groupChatByName;
        }


        public GroupChat GetChatById(int id, int userId)
        {
            GroupChat chat = new GroupChat();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = "SELECT gc.* FROM GroupChats gc " +
                               "INNER JOIN GroupChatMembers gcm ON gc.GroupChatId = gcm.GroupChatId " +
                               "WHERE gc.GroupChatId = @id AND gcm.UserId = @userId";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@userId", userId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chat.Name = reader["Name"].ToString();
                            chat.Description = reader["Description"].ToString();
                            chat.GroupChatId = Convert.ToInt32(reader["GroupChatId"]);
                        }
                    }
                }
            }
            return chat;
        }





    }
}
