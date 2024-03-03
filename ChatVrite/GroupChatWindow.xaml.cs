using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChatVrite
{
    /// <summary>
    /// Логика взаимодействия для GroupChatWindow.xaml
    /// </summary>
    public partial class GroupChatWindow : Window
    {
        public string name;
        public DispatcherTimer chatUpdateTimer;
        private DispatcherTimer timer;
        private GroupChat groupChat;
        private string currentChat;
        private int currentChatId;
        private int userId;
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        public GroupChatWindow(string UserName)
        {
            InitializeComponent();
            name = UserName;
            groupChat = new GroupChat();


            chatUpdateTimer = new DispatcherTimer();
            chatUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            chatUpdateTimer.Tick += ChatUpdateTimer_Tick;
            chatUpdateTimer.Start();

        }

        private void ChatUpdateTimer_Tick(object sender, EventArgs e)
        {
            if(SearchTextBox.Text == "Поиск")
            {
                LoadChatList();
            }

            if (ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight && ShowUsersListBtn.Visibility == Visibility.Visible)
            {
                LoadSelectedChat();
            }




        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EditConditionStatusInBaseTry(true);
            userId = GetUserIdByName(name);
            LoadChatList();
        }


        public void LoadChatList()
        {
            try
            {
                // Получаем список групповых чатов
                List<GroupChat> Groupchats = groupChat.GroupChats;
                Groupchats = groupChat.GetAllGroupChats(userId);

                // Привязываем список групповых чатов к элементу управления
                GroupChatContainer.ItemsSource = null;
                GroupChatContainer.ItemsSource = Groupchats;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void AddGroupChatBtn_Click(object sender, RoutedEventArgs e)
        {
            AddGroupChat addGroupChat = new AddGroupChat(name,userId);
            addGroupChat.ShowDialog();
            LoadChatList();

            //MessageBox.Show("Class");
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            PersonalWindow personalWindow = new PersonalWindow(name);
            this.Close();
            personalWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Chat chat = new Chat(name);
            this.Close();
            chat.ShowDialog();
        }

        private void change(object sender, RoutedEventArgs e)
        {
            SessionManager.ClearUserData(); // Очистка данных о пользователе при выходе
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

         private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            EditConditionStatusInBaseTry(false);
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(GroupChatContainer == null)
                {
                   // MessageBox.Show("GroupChatContainer is null");
                    return;
                }

                string searchText = SearchTextBox.Text;
                List<GroupChat> chatList = new List<GroupChat>();
                GroupChat groupChats = new GroupChat();
                chatList = groupChats.GetChatsByName(searchText,userId);
                //GroupChatContainer.Items.Clear();
                GroupChatContainer.ItemsSource = chatList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            




        }

        private void Canhel_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "Поиск";
            LoadChatList();
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            int currentUserId = GetUserIdByName(name);
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                MessageBox.Show("Введите текст сообщения.");
                return;
            }
            if (currentChatId == -1)
            {
                MessageBox.Show("Выберите беседу.");
                return;
            }
            string messageText = MessageTextBox.Text;
            DateTime timestamp = DateTime.Now;

            if (SendMessageToDatabase(currentUserId, currentChatId, messageText, timestamp))
            {
                MessageTextBox.Clear();
                LoadChat(currentChatId);
                //MarkChatAsRead(currentUserId, currentReceiverUserId);
            }
            else
            {
                MessageBox.Show("Ошибка отправки сообщения.");
            }
        }

        private bool SendMessageToDatabase(int senderUserId, int currentGroupChatId, string messageText, DateTime timestamp)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO `GroupChatMessages` ( `GroupChatId`, `SenderId`, `MessageText`, `Timestamp`, `IsRead`) VALUES ( @GroupChatId, @SenderId, @MessageText, @Timestamp, @IsRead)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupChatId",  currentGroupChatId);
                        command.Parameters.AddWithValue("@SenderId", senderUserId);
                        command.Parameters.AddWithValue("@MessageText", messageText);
                        command.Parameters.AddWithValue("@Timestamp", timestamp);
                        command.Parameters.AddWithValue("@IsRead", 0); // Устанавливаем начальное значение IsRead в 0 (не прочитано)
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
                    return false;
                }
            }
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {


        }






        private void SelectChat_Click(object sender, RoutedEventArgs e)
        {
            Button userButton = (Button)sender;

            currentChatId = (int)userButton.Tag;
            LoadSelectedChat();




        }

        public void LoadSelectedChat()
        {
            ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight + 1000);
            GroupChat groupChatThis = new GroupChat();
            groupChatThis = groupChatThis.GetChatById(currentChatId, userId);
            ChatName.Text = groupChatThis.Name;
            LoadChat(currentChatId);
            ShowChatAndHideUsersList();
            ShowUsersListBtn.Visibility = Visibility.Visible;
            ShowAllPeople.Visibility = Visibility.Collapsed;
            AdduserListPanel.Visibility = Visibility.Collapsed;
        }

        int GetUserIdByName(string username)
        {
            int userId = 0;

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT UserID FROM Users WHERE Username='{username}'", connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userId = reader.GetInt32(0);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                }
            }
            return userId;
        }

        string GetUserNameById(int id)
        {
            string username = "UnName";

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT Username FROM Users WHERE UserID='{id}'", connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            username = reader[0].ToString();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                }
            }
            return username;
        }
        private void LoadChat(int groupChatId)
        {
            try
            {
                
                ChatPanel.Children.Clear();
                ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight);
                LoadChatMain(groupChatId);
                ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight + 1000);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при получении сообщений: {ex.Message}");
            }
        }


        private void LoadChatMain(int groupChatId)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = "SELECT SenderId, MessageText, Timestamp FROM GroupChatMessages WHERE GroupChatId = @groupChatId ORDER BY Timestamp";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@groupChatId", groupChatId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int userId = reader.GetInt32("SenderId");
                            string messageText = reader.GetString("MessageText");
                            DateTime timestamp = reader.GetDateTime("Timestamp");
                            int currentUserId = GetUserIdByName(name);
                            string SenderMessageName = GetUserNameById(userId);

                            Border messageBorder = new Border
                            {
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(1),
                                Padding = new Thickness(5),
                                Margin = new Thickness(5, 2, 5, 2),
                                CornerRadius = new CornerRadius(10),
                                Background = Brushes.White,
                                HorizontalAlignment = userId == currentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                            };

                            TextBlock messageTextBlock = new TextBlock
                            {
                                Text = messageText,
                                TextWrapping = TextWrapping.Wrap,
                                FontFamily = new FontFamily("Montserrat bold"),
                                FontSize = 25,
                                MaxWidth = 800
                            };

                            TextBlock FooterTextBlock = new TextBlock
                            {
                                Text = $"{SenderMessageName}  {timestamp.ToString("HH:mm")}",
                                Margin = new Thickness(10, 0, 10, 0),
                                FontSize = 15,
                                FontFamily = new FontFamily("Montserrat bold"),
                                Foreground = Brushes.Gray,
                                TextAlignment = userId == currentUserId ? TextAlignment.Right : TextAlignment.Left,
                            };

                            messageBorder.Child = messageTextBlock;
                            ChatPanel.Children.Add(messageBorder);
                            ChatPanel.Children.Add(FooterTextBlock);
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ShowUsersListBtn.Visibility = Visibility.Collapsed;
            ChatScrollViewer.Visibility = Visibility.Collapsed;
            SendMessagePanel.Visibility = Visibility.Collapsed;
            ShowChatScroll.Visibility = Visibility.Visible;
            ShowAllPeople.Visibility = Visibility.Visible;

            if (CreatorGroupChat(currentChatId,userId))
            {//adminstaffInfo
                ChatScrollViewer.Visibility=Visibility.Collapsed;
                SendMessagePanel.Visibility=Visibility.Collapsed;
                ChatUsersPanel.Visibility=Visibility.Visible;
                AdminsUsersListPanel.Visibility=Visibility.Visible;

                List<User> users = new List<User>();
                users= GetuserInfoByIdGroup(currentChatId);
                AdminsUsersList.ItemsSource = users;



            }
            else
            {//userstaffInfo
                ChatScrollViewer.Visibility = Visibility.Collapsed;
                SendMessagePanel.Visibility = Visibility.Collapsed;
                ChatUsersPanel.Visibility=Visibility.Visible;
                UserListPanel.Visibility = Visibility.Visible;

                List<User> users = new List<User>();
                users = GetuserInfoByIdGroup(currentChatId);
                usersList.ItemsSource = users;
            }



        }

        private List<User> GetuserInfoByIdGroup(int id)
        {
            Random random = new Random();
            List<User> userslist = new List<User>();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT u.* FROM Users u INNER JOIN GroupChatMembers gcm ON u.UserID = gcm.UserId WHERE gcm.GroupChatId =  @ChatId", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                UserID = Convert.ToInt32(reader["UserId"]),
                                UserName = reader["UserName"].ToString(),
                                ColorUser= new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256))),
                                City= reader["City"].ToString()
                               
                            };
                            if (GetInfoBycreatorById(id,user. UserID))
                            {
                                user.Creatorgroup = "Создатель беседы";
                            }
                            else
                            {
                                user.Creatorgroup = "";
                            }
                                userslist.Add(user);
                        }
                    }
                }
            }


            return userslist;
        }


        private bool GetInfoBycreatorById(int chatid,int UserIdCheck)
        {


            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM `GroupChats` WHERE `GroupChatId` = @ChatId", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatid);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["GorupCreaterId"]);
                            if (id == UserIdCheck)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private void removeUserInChat_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int userId = (int)button.Tag;
                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = $"DELETE FROM GroupChatMembers WHERE UserId = {userId} AND GroupChatId = {currentChatId};";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int rowsInserted = command.ExecuteNonQuery();
                        if (rowsInserted > 0)
                        {
                            MessageBox.Show("Успешно удалили пользователя из беседы", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                            List<User> users = new List<User>();
                            users = GetuserInfoByIdGroup(currentChatId);
                            AdminsUsersList.ItemsSource = null;
                            AdminsUsersList.ItemsSource = users;
                            LoadChatList();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private bool CreatorGroupChat(int GroupId,int UserId)
        {
            GroupChat chat = new GroupChat();
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = $"SELECT `GorupCreaterId`  FROM `GroupChats` WHERE GroupChatId={GroupId}";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToInt32(reader["GorupCreaterId"]) == userId)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void ShowChatAndHideUsersList()
        {
            ShowUsersListBtn.Visibility = Visibility.Visible;
            ChatScrollViewer.Visibility = Visibility.Visible;
            SendMessagePanel.Visibility = Visibility.Visible;
            ShowChatScroll.Visibility = Visibility.Collapsed;
            ShowAllPeople.Visibility = Visibility.Collapsed;
            AdduserListPanel.Visibility = Visibility.Collapsed;

            AdminsUsersListPanel.Visibility = Visibility.Collapsed;
            UserListPanel.Visibility = Visibility.Collapsed;
        }
        private void ShowChatScroll_Click(object sender, RoutedEventArgs e)
        {
         
            ShowChatAndHideUsersList();
        }

        public void ShowpeopleWhoNotInGroupList()
        {
            ChatUsersPanel.Visibility = Visibility.Visible;
            AdminsUsersListPanel.Visibility = Visibility.Collapsed;
            UserListPanel.Visibility = Visibility.Collapsed;

            AdduserListPanel.Visibility = Visibility.Visible;

            List<User> users = new List<User>();
            try
            {
                AddUserList.ItemsSource = null;
                users = GetUserListWhoNotInThisGroup(currentChatId);
                AddUserList.ItemsSource = users;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ShowAllPeople_Click(object sender, RoutedEventArgs e)
        {
            ShowpeopleWhoNotInGroupList();
        }

        private List<User> GetUserListWhoNotInThisGroup(int id)
        {
            Random random = new Random();
            List<User> users = new List<User>();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = $"SELECT u.* FROM Users u LEFT JOIN GroupChatMembers gcm ON u.UserID = gcm.UserId AND gcm.GroupChatId = {id} WHERE gcm.UserId IS NULL";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                UserName=reader["UserName"].ToString(),
                                UserID = Convert.ToInt32(reader["UserID"]),
                                City = reader["City"].ToString(),
                                Condition= reader["Сondition"].ToString(),
                                ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256))),
                            };
                          users.Add(user);
                        }
                    }
                }
            }

            return users;
        }
        private void addNewUserInGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Получаем ID пользователя из свойства Tag кнопки
                try
                {
                    int ReqestID = (int)button.Tag;
                    

                    using (MySqlConnection connection = new MySqlConnection(DbConnection))
                    {
                        try
                        {
                            connection.Open();
                            string query = "INSERT INTO `GroupChatMembers` ( `GroupChatId`, `UserId`) VALUES ( @GroupChatId, @UserId)";

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@GroupChatId", currentChatId);
                                command.Parameters.AddWithValue("@UserId", ReqestID);
                                command.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Вы успешно добавили пользователя в беседу!");
                            ShowpeopleWhoNotInGroupList();



                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine($"Ошибка при добавлении пользователя! {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }



            }
        }


        private void EditConditionStatusInBaseTry(bool condition)
        {
            try
            {
                EditConditionsStatusInBase(condition);
                //MessageBox.Show($"Успешно установили статус на  {condition}!");


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены статуса. {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void EditConditionsStatusInBase(bool condition)
        {
            string ConditionString;
            if (condition)
            {
                ConditionString = "Online";
            }
            else
            {
                ConditionString = DateTime.Now.ToString();
            }
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                string query = "UPDATE `Users` SET `Сondition` = @Condition WHERE `Users`.`Username` =@UserName";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", name);
                    command.Parameters.AddWithValue("@Condition", ConditionString);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
