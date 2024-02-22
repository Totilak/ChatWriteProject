using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Media;

namespace ChatVrite
{
    class SoundExample
    {
        public void PlaySound(string soundFilePath)
        {
            try
            {
                SoundPlayer player = new SoundPlayer(soundFilePath);
                player.Play(); // Воспроизвести звук
            }
            catch (Exception ex)
            {
                // Обработка ошибок при воспроизведении звука
            }
        }
    }
    public partial class Chat : Window
    {
        private DispatcherTimer chatUpdateTimer;
        private DispatcherTimer timer;
        private bool hasNewMessages = false; // Флаг для определения наличия новых сообщений
        private bool isChatActive = false; // Флаг чтобы отслеживать активность чата

        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";
        string name;
        int currentUserId;
        private int currentReceiverUserId = -1;
        private string receiverName;
        private string currentUser;
        private string currentReceiver;


        private SoundPlayer player;
        public Chat(string UserName)
        {
            InitializeComponent();

            if (!CheckDbConnection())
            {
                MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.");
                Close();
            }
            player = new SoundPlayer(@"sound.wav"); 
            name = UserName;
            currentUserId = GetUserIDFromName(name);

            chatUpdateTimer = new DispatcherTimer();
            chatUpdateTimer.Interval = TimeSpan.FromSeconds(1); 
            chatUpdateTimer.Tick += ChatUpdateTimer_Tick;
            chatUpdateTimer.Start();

            LoadChat(-1); // Загружаем начальный чат (пустой)

            Thread backgroundThread = new Thread(CheckForNewMessagesInBackground);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();

        }


        private DateTime lastReceivedMessageTimestamp = DateTime.MinValue; // Переменная для хранения времени последнего полученного сообщения






        //%Loaded/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<User> users = GetUsersFromDatabase();

            Style buttonStyle = this.FindResource("btnuser") as Style;
            foreach (User user in users)
            {
                if (user.UserName == name)
                {

                }
                else
                {
                    Button userButton = new Button
                    {
                        Tag = user.UserID,
                        Style = buttonStyle // Установка стиля
                    };
                    int msgCount = GetUnreadMessageCount(currentUserId, user.UserID);
                    userButton.Content = CreateUserButtonContent(user.UserName, msgCount);
                    userButton.Click += UserButton_Click;
                    UserButtonContainer.Items.Add(userButton);
                }

            }
        }
        //%Loaded/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        //%btn/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            Button userButton = (Button)sender;
            currentUser = name;

            int receiverUserId = (int)userButton.Tag;

            currentReceiverUserId = receiverUserId;


            MarkChatAsRead(currentUserId, currentReceiverUserId);
            previousMsgCount = 0;
            LoadChat(receiverUserId);
            ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight + 1000);
            isChatActive = true;

            receiverName = GetReceiverUserNameFromDialog(receiverUserId);
            reciv.Text = receiverName;//Имя сверху

            int msgCount = GetUnreadMessageCount(currentUserId, receiverUserId);
            userButton.Content = CreateUserButtonContent(receiverName, msgCount);

        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();

                string query = "SELECT UserID, UserName FROM Users WHERE UserName LIKE @searchText";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (UserButtonContainer != null)
                        {
                            UserButtonContainer.Items.Clear();

                        }

                        while (reader.Read())
                        {
                            int userId = reader.GetInt32("UserID");
                            string userName = reader.GetString("UserName");

                            if (userName == name)
                            {

                            }
                            else
                            {

                                Button userButton = new Button
                                {
                                    Tag = userId,
                                    Style = FindResource("btnuser") as Style
                                };
                                int msgCount = GetUnreadMessageCount(currentUserId, userId);
                                userButton.Content = CreateUserButtonContent(userName, msgCount);


                                userButton.Click += UserButton_Click;
                                UserButtonContainer.Items.Add(userButton);
                            }

                        }
                    }
                }
            }
        }

        private StackPanel CreateUserButtonContent(string userName, int msgCount)
        {
            TextBlock userNameTextBlock = new TextBlock();
            userNameTextBlock.Inlines.Add(new Run(userName + "   "));
            userNameTextBlock.FontSize = 30;

            if (msgCount <= 0)
            {
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { userNameTextBlock }
                };
            }
            else
            {
                Border unreadMessagesBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(58, 106, 235)),
                CornerRadius = new CornerRadius(7),
                    Padding = new Thickness(5),
                    Child = new TextBlock
                    {
                        Text = msgCount > 100 ? "99+" : msgCount.ToString(),
                        FontSize = 25,
                        Foreground = Brushes.White
                    }
                };

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { userNameTextBlock, unreadMessagesBorder }
                };
            }
        }



        //%btn/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////








        //%chat/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        int previousMsgCount;
        int newMsgCount;
        private void ChatUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (currentReceiverUserId != -1)
            {
                if (hasNewMessages)
                {
                    LoadChat(currentReceiverUserId);
                    hasNewMessages = false;
                    
                }
            }
            // Обновление содержимого StackPanel
            UserButtonContainer.Items.Clear();
            List<User> users = GetUsersFromDatabase();

            Style buttonStyle = this.FindResource("btnuser") as Style;
            foreach (User user in users)
            {
                if (user.UserName == name)
                {

                }
                else
                {
                    Button userButton = new Button
                    {
                        Tag = user.UserID,
                        Style = buttonStyle // Установка стиля
                    };
                    


                    int msgCount = GetUnreadMessageCount(currentUserId, user.UserID);
                    newMsgCount += msgCount;
                    userButton.Content = CreateUserButtonContent(user.UserName, msgCount);
                    userButton.Click += UserButton_Click;
                    UserButtonContainer.Items.Add(userButton);
                }

            }
            if(newMsgCount>previousMsgCount)
            {
                previousMsgCount = newMsgCount;
                player.Play();
                newMsgCount = 0;
            }
            else
            {
                newMsgCount = 0;
            }
        }
        private void LoadChat(int receiverUserId)
        {
            ChatPanel.Children.Clear();
            ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight);
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SenderUserID, MessageText, Timestamp, IsRead FROM Messages WHERE " +
                                   "((SenderUserID = @senderUserId AND ReceiverUserID = @receiverUserId) OR " +
                                   "(SenderUserID = @receiverUserId AND ReceiverUserID = @senderUserId)) " +
                                   "ORDER BY Timestamp";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@senderUserId", currentUserId);
                        command.Parameters.AddWithValue("@receiverUserId", receiverUserId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string messageText = reader.GetString("MessageText");
                                DateTime timestamp = reader.GetDateTime("Timestamp");
                                int senderUserId = reader.GetInt32("SenderUserID");
                                int isRead = reader.GetInt32("IsRead");

                                Border messageBorder = new Border
                                {
                                    BorderBrush = Brushes.LightGray,
                                    BorderThickness = new Thickness(1),
                                    Padding = new Thickness(5),
                                    Margin = new Thickness(5, 2, 5, 2),
                                    CornerRadius = new CornerRadius(10),
                                    Background = Brushes.White,
                                    HorizontalAlignment = senderUserId == currentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                                };
                                TextBlock messageTextBlock = new TextBlock
                                {
                                    Text = messageText,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontFamily = new FontFamily("Montserrat bold"),
                                    FontSize = 25,
                                    MaxWidth =800
                                };
                                TextBlock timestampTextBlock = new TextBlock
                                {
                                    Text = $"{timestamp.ToString("HH:mm")} {(isRead == 1 ? "◈" : "◇")}",
                                    FontSize = 15,
                                    FontFamily = new FontFamily("Montserrat bold"),
                                    Foreground = Brushes.Gray,
                                    TextAlignment = senderUserId == currentUserId ? TextAlignment.Right : TextAlignment.Left,
                                };
                                messageBorder.Child = messageTextBlock;
                                ChatPanel.Children.Add(messageBorder);
                                ChatPanel.Children.Add(timestampTextBlock);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при получении сообщений: {ex.Message}");
                }
            }
            ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ScrollableHeight + 1000);
        }

        private void MarkChatAsRead(int receiverUserName, int senderUserName)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Messages SET IsRead = 1 WHERE (SenderUserID = (SELECT UserID FROM Users WHERE Userid = @SenderUserName) AND ReceiverUserID = (SELECT UserID FROM Users WHERE Userid = @ReceiverUserName))";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SenderUserName", senderUserName);
                        command.Parameters.AddWithValue("@ReceiverUserName", receiverUserName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка обновления статуса прочтения чата: {ex.Message}");
                }
            }
        }
        //%chat/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////








        //%checkers/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void CheckForNewMessagesInBackground()
        {
            while (true)
            {
                if (currentReceiverUserId != -1)
                {
                    // Здесь делаем запрос в базу данных, чтобы получить количество новых сообщений
                    DateTime newMessageTimestamp = GetLastReceivedMessageTimestamp(currentReceiverUserId);

                    if (newMessageTimestamp > lastReceivedMessageTimestamp)
                    {
                        hasNewMessages = true;
                        lastReceivedMessageTimestamp = newMessageTimestamp;
                    }

                    // Обновление статуса прочтения сообщений
                    if (isChatActive)
                    {
                        MarkChatAsRead(currentUserId, currentReceiverUserId);
                    }
                }

                Thread.Sleep(1000); // Ожидаем некоторое время перед следующей проверкой (1 секунда)
            }
        }
        private bool CheckDbConnection()
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                    return false;
                }
            }
        }
        //%checkers/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////






        //%get/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DateTime GetLastReceivedMessageTimestamp(int receiverUserId)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT MAX(Timestamp) FROM Messages WHERE (ReceiverUserID = @receiverUserId AND SenderUserID = @currentUserId) OR (ReceiverUserID = @currentUserId AND SenderUserID = @receiverUserId)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@currentUserId", currentUserId);
                        command.Parameters.AddWithValue("@receiverUserId", receiverUserId);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToDateTime(result);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при получении времени последнего полученного сообщения: {ex.Message}");
                }
            }
            return DateTime.MinValue;
        }
        private List<User> GetUsersFromDatabase()
        {
            List<User> users = new List<User>();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand("SELECT UserID, UserName FROM Users", connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                UserName = reader.GetString("UserName"),
                            };
                            users.Add(user);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                }
            }

            return users;
        }

        private int GetUserIDFromName(string userName)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand("SELECT UserID FROM Users WHERE UserName = @UserName", connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка при получении идентификатора пользователя: {ex.Message}");
                }
            }

            return -1; 
        }

        private string GetReceiverUserNameFromDialog(int id)
        {
            string recName = "Ошибка";

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserName FROM Users WHERE Userid = @ReciverId";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReciverId", id);
                        object result = command.ExecuteScalar();
                        recName = (string)result;
                        if (result != null)
                        {
                            return Convert.ToString(result);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка при получении идентификатора пользователя: {ex.Message}");
                }
            }

            return recName;
        }

        private int GetUnreadMessageCount(int senderName, int receiverName)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Messages " +
                                   "WHERE ReceiverUserID = (SELECT UserID FROM Users WHERE Userid = @SenderUserName) " +
                                   "AND SenderUserID = (SELECT UserID FROM Users WHERE Userid = @ReceiverUserName) " +
                                   "AND IsRead = 0";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SenderUserName", senderName);
                        command.Parameters.AddWithValue("@ReceiverUserName", receiverName);
                        int unreadCount = Convert.ToInt32(command.ExecuteScalar());
                        return unreadCount;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка при получении количества непрочитанных сообщений: {ex.Message}");
                    return 0;
                }
            }
        }

        //%get/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //%dbedit/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool SendMessageToDatabase(int senderUserId, int receiverUserId, string messageText, DateTime timestamp)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Messages (SenderUserID, ReceiverUserID, MessageText, Timestamp, IsRead) " +
                                   "VALUES (@SenderUserID, @ReceiverUserID, @MessageText, @Timestamp, @IsRead)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SenderUserID", senderUserId);
                        command.Parameters.AddWithValue("@ReceiverUserID", receiverUserId);
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
        //%dbedit/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        //%Click/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            PersonalWindow personalWindow = new PersonalWindow(name);
            personalWindow.Show();
            this.Close();
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                MessageBox.Show("Введите текст сообщения.");
                return;
            }
            if (currentReceiverUserId == -1)
            {
                MessageBox.Show("Выберите собеседника.");
                return;
            }
            string messageText = MessageTextBox.Text;
            DateTime timestamp = DateTime.Now;
            if (SendMessageToDatabase(currentUserId, currentReceiverUserId, messageText, timestamp))
            {
                MessageTextBox.Clear();
                LoadChat(currentReceiverUserId);
                MarkChatAsRead(currentUserId, currentReceiverUserId);
            }
            else
            {
                MessageBox.Show("Ошибка отправки сообщения.");
            }
        } 
        private void Window_Closed(object sender, EventArgs e)
        {

            chatUpdateTimer.Stop();

        }
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }
        private void Canhel_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void change(object sender, RoutedEventArgs e)
        {
            SessionManager.ClearUserData(); // Очистка данных о пользователе при выходе
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();

        }
        //%Click/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}

/* Козрина

 private int GetReceiverUserIdFromDialog(string receiverName)
{
    using (MySqlConnection connection = new MySqlConnection(DbConnection))
    {
        try
        {
            connection.Open();
            string query = "SELECT UserID FROM Users WHERE UserName = @ReceiverName";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ReceiverName", receiverName);
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Ошибка при получении идентификатора пользователя: {ex.Message}");
        }
    }
    return -1;


}

  private List<Message> GetMessagesForDialog(int receiverUserId)
        {
            List<Message> messages = new List<Message>();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SenderUserID, MessageText, Timestamp, IsRead FROM Messages WHERE " +
                                   "((SenderUserID = @senderUserId AND ReceiverUserID = @receiverUserId) OR " +
                                   "(SenderUserID = @receiverUserId AND ReceiverUserID = @senderUserId)) " +
                                   "ORDER BY Timestamp";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@senderUserId", currentUserId);
                        command.Parameters.AddWithValue("@receiverUserId", receiverUserId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string messageText = reader.GetString("MessageText");
                                DateTime timestamp = reader.GetDateTime("Timestamp");
                                int senderUserId = reader.GetInt32("SenderUserID");
                                int isRead = reader.GetInt32("IsRead");

                                Border messageBorder = new Border
                                {
                                    BorderBrush = Brushes.LightGray,
                                    BorderThickness = new Thickness(1),
                                    Padding = new Thickness(5),
                                    Margin = new Thickness(5, 2, 5, 2),
                                    CornerRadius = new CornerRadius(10),
                                    Background = Brushes.White,
                                };

                                TextBlock messageTextBlock = new TextBlock
                                {
                                    Text = $"{(isRead == 1 ? "1" : "0")} {messageText}",
                                    TextWrapping = TextWrapping.Wrap,
                                    
                                };

                                TextBlock timestampTextBlock = new TextBlock
                                {
                                    Text = timestamp.ToString("HH:mm"),
                                    FontSize = 10,
                                    Foreground = Brushes.Gray,
                                    TextAlignment = (senderUserId == currentUserId) ? TextAlignment.Right : TextAlignment.Left,
                                };

                                messageBorder.Child = messageTextBlock;

                                ChatPanel.Children.Add(messageBorder);
                                ChatPanel.Children.Add(timestampTextBlock);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при получении сообщений: {ex.Message}");
                }
            }

            return messages;
        }
 */