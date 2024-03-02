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

namespace ChatVrite
{
    /// <summary>
    /// Логика взаимодействия для FriendListWindow.xaml
    /// </summary>
    public partial class FriendListWindow : Window
    {
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";
        public string name;
        public int UserID;
        public FriendListWindow(string UserName)
        {
            InitializeComponent();
            name = UserName;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UserID = GetUserIdByName(name);
                EditConditionStatusInBaseTry(true);
                List<User> users = GetUsersFromDatabase();
                FriendsList.ItemsSource = users;
                if (FriendsList.Items.IsEmpty)
                {
                    ZeroText.Visibility = Visibility.Visible;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        int GetUserIdByName(string username)
        {
            int userId=0;

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
                            userId=reader.GetInt32(0);
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

        private List<User> GetUsersFromDatabase()
        {
            List<User> users = new List<User>();
            Random random = new Random();

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT u.* FROM Users u INNER JOIN ( SELECT UserID1 AS UserID FROM Friends WHERE UserID2 = {UserID} UNION SELECT UserID2 AS UserID FROM Friends WHERE UserID1 = {UserID} ) f ON u.UserID = f.UserID WHERE u.UserID != {UserID}", connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if(reader.GetString("UserName")==name)
                            {

                            }
                            else
                            {
                                User user = new User
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    UserName = reader.GetString("UserName"),
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition")

                                   
                                };
                                if(user.Condition !="Online"&& user.Condition =="")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if(user.Condition !="Online")
                                {
                                    user.Condition="Был в сети "+user.Condition;
                                }
                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                           
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







        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            try
            {
                if (FriendsList == null)
                {
                   // MessageBox.Show("FriendsList is null");
                    return;
                }

                string searchText = SearchTextBox.Text;
                List<User> users = new List<User>();
                Random random = new Random();

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = ($"SELECT u.* FROM Users u INNER JOIN ( SELECT UserID1 AS UserID FROM Friends WHERE UserID2 = {UserID} UNION SELECT UserID2 AS UserID FROM Friends WHERE UserID1 = {UserID} ) f ON u.UserID = f.UserID WHERE u.UserID != {UserID} AND u.UserName LIKE  @searchText");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string userName = reader.GetString("UserName");

                                if (userName == name)
                                {
                                    continue;
                                }

                                User user = new User
                                {
                                    UserID = userId,
                                    UserName = userName,
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition")
                                };

                                if (user.Condition != "Online" && user.Condition == "")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if (user.Condition != "Online")
                                {
                                    user.Condition = "Был в сети " + user.Condition;
                                }

                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                        }
                    }
                }

                FriendsList.ItemsSource = users; // Установка ItemsSource после завершения цикла
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        public int  GetUserIdByReqestID(int reqestId)
        {
            int IDUSER = 0;

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT * FROM `FriendRequests` WHERE `RequestID`={reqestId}", connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           IDUSER = reader.GetInt32("SenderID");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                }
            }

            return IDUSER;
        }
        private void AcceptReqest_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int ReqestID = (int)button.Tag;
                try
                {
                    int senderID = GetUserIdByReqestID(ReqestID);
                    UpdateReqestTabelstatus(ReqestID,1);
                    InsertIntoDbFriends(senderID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        public void UpdateReqestTabelstatus(int ReqestID,int status)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();

                string query = $"UPDATE `FriendRequests` SET `Status` = '{status}' WHERE `FriendRequests`.`RequestID` ={ReqestID} ";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    int rowsInserted = command.ExecuteNonQuery();

                    if (rowsInserted > 0)
                    {
                        switch(status)
                        {
                            case 0:
                                MessageBox.Show("Заявка снова активка!");
                                break;

                             case 1:
                                MessageBox.Show("Заявка успешно Принята! Теперь вы друзья.");
                                break;

                             case 2:
                                MessageBox.Show("Вы отклонили заявку.");
                                break;

                            case 3:
                                MessageBox.Show("Вы отменили заявку.");
                                break;

                            default:
                                MessageBox.Show("Ошибка, ожидалось другое значение.");
                                break;

                        }
                        ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
                        getreqestListToUser();

                    }
                    else
                    {
                        MessageBox.Show("Ошибка принятия заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public void InsertIntoDbFriends(int senderID)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();

                string query = "INSERT INTO `Friends` (`UserID1`, `UserID2`, `Status`, `DateCreated`, `Lastupdated`) VALUES (@UserID, @ReceiverID, 'Друзья', @DateCreated, @DateCreated)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.Parameters.AddWithValue("@ReceiverID", senderID);
                    command.Parameters.AddWithValue("@DateCreated", DateTime.Now);
                    int rowsInserted = command.ExecuteNonQuery();
                    if (rowsInserted > 0)
                    {
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении в друзья.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void DeclineReqest_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Получаем ID пользователя из свойства Tag кнопки
                int ReqestID = (int)button.Tag;
                try
                {
                    int senderID = GetUserIdByReqestID(ReqestID);
                    UpdateReqestTabelstatus(ReqestID,2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }
        private void recallMyReqest_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Получаем ID пользователя из свойства Tag кнопки
                int ReqestID = (int)button.Tag;
                try
                {
                    int senderID = GetUserIdByReqestID(ReqestID);
                    UpdateReqestTabelstatus(ReqestID, 3);
                    SendingReqestListCode();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }


        private void ViewFriend_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Получаем ID пользователя из свойства Tag кнопки
                int userID = (int)button.Tag;

                // Теперь вы можете использовать этот ID для выполнения необходимых действий, например, запроса информации о пользователе с этим ID
                // Например:
                MessageBox.Show($"Нажата кнопка для просмотра информации о пользователе с ID {userID}");
            }
        }

        public void removeFriendInDbByFriendID(int FriendID)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();

                string query = $"DELETE FROM Friends WHERE (UserID1 = {UserID} AND UserID2 = {FriendID}) OR (UserID1 = {FriendID} AND UserID2 = {UserID});";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    int rowsInserted = command.ExecuteNonQuery();
                    if (rowsInserted > 0)
                    {
                        MessageBox.Show("Успешно удалили друга....", "Успех?", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении друга.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
         private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int friendID = (int)button.Tag;
                removeFriendInDbByFriendID(friendID);
                MakeHidenSomeelements();
                FriendScroll.Visibility = Visibility.Visible;
                UserScroll.Visibility = Visibility.Collapsed;
                ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
                ReqestPaneToMeFromSenderl.Visibility = Visibility.Collapsed;
                SearchTextBox.Visibility = Visibility.Visible;
                ClearSeacrh.Visibility = Visibility.Visible;
                FriendsList.ItemsSource = null;
                List<User> users = GetUsersFromDatabase();
                FriendsList.ItemsSource = users;
            }
        }

        private void AddFriendToList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Получаем ID пользователя из свойства Tag кнопки
                int ReceiverID = (int)button.Tag;

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(DbConnection))
                    {
                        connection.Open();

                        string query = "INSERT INTO `FriendRequests` (`SenderID`, `ReceiverID`, `Date`) VALUES (@UserID, @ReceiverID, @Date)";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", UserID);
                            command.Parameters.AddWithValue("@ReceiverID", ReceiverID);
                            command.Parameters.AddWithValue("@Date", DateTime.Now);

                            int rowsInserted = command.ExecuteNonQuery();

                            if (rowsInserted > 0)
                            {
                                MessageBox.Show("Заявка успешно отправлена! Ожидайте ответа.");
                                AddFriendCode();

                            }
                            else
                            {
                                MessageBox.Show("Ошибка отправки заявки, возможно пользователь запретил это", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }




        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            PersonalWindow personalWindow = new PersonalWindow(name);
            this.Close();
            personalWindow.Show();
        }
        private void change(object sender, RoutedEventArgs e)
        {
            SessionManager.ClearUserData(); // Очистка данных о пользователе при выходе
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();

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








        

        private void CancelSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ZeroText.Visibility=Visibility.Collapsed;
            MakeHidenSomeelements();
            FriendScroll.Visibility = Visibility.Visible;
            UserScroll.Visibility = Visibility.Collapsed;
            ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
            ReqestPaneToMeFromSenderl.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Visible;
            ClearSeacrh.Visibility = Visibility.Visible;


            List<User> users = GetUsersFromDatabase();
            FriendsList.ItemsSource = users;
            if (FriendsList.Items.IsEmpty)
            {
                ZeroText.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ZeroText.Visibility = Visibility.Collapsed;
  
            MakeHidenSomeelements();
            FriendScroll.Visibility = Visibility.Visible;
            UserScroll.Visibility = Visibility.Collapsed;
            ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
            ReqestPaneToMeFromSenderl.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Visible;
            ClearSeacrh.Visibility = Visibility.Visible;

            try
            {
                if (FriendsList == null)
                {
                    // MessageBox.Show("FriendsList is null");
                    return;
                }

                string searchText = SearchTextBox.Text;
                List<User> users = new List<User>();
                Random random = new Random();

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = ($"SELECT u.* FROM Users u INNER JOIN ( SELECT UserID1 AS UserID FROM Friends WHERE UserID2 = {UserID} UNION SELECT UserID2 AS UserID FROM Friends WHERE UserID1 = {UserID} ) f ON u.UserID = f.UserID WHERE u.UserID != {UserID} AND u.Сondition = 'Online';");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string userName = reader.GetString("UserName");

                                if (userName == name)
                                {
                                    continue;
                                }

                                User user = new User
                                {
                                    UserID = userId,
                                    UserName = userName,
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition")
                                };

                                if (user.Condition != "Online" && user.Condition == "")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if (user.Condition != "Online")
                                {
                                    user.Condition = "Был в сети " + user.Condition;
                                }

                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                        }
                    }
                }

                FriendsList.ItemsSource = users; // Установка ItemsSource после завершения цикла
                if (FriendsList.Items.IsEmpty)
                {
                    ZeroText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            EditConditionStatusInBaseTry(false);
        }

        public void AddFriendCode()
        {
            MakeHidenSomeelements();
            FriendScroll.Visibility = Visibility.Collapsed;
            UserScroll.Visibility = Visibility.Visible;
            ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
            ReqestPaneToMeFromSenderl.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Visible;
            ClearSeacrh.Visibility = Visibility.Visible;

            try
            {
                if (FriendsList == null)
                {
                    // MessageBox.Show("FriendsList is null");
                    return;
                }

                string searchText = SearchTextBox.Text;
                List<User> users = new List<User>();
                Random random = new Random();

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = ($"SELECT * FROM Users WHERE UserID NOT IN ( SELECT UserID1 AS UserID FROM Friends WHERE UserID2 = {UserID} UNION SELECT UserID2 AS UserID FROM Friends WHERE UserID1 = {UserID} ) AND UserID NOT IN ( SELECT ReceiverID AS UserID FROM FriendRequests WHERE SenderID = {UserID} AND Status = 0 );");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string userName = reader.GetString("UserName");

                                if (userName == name)
                                {
                                    continue;
                                }

                                User user = new User
                                {
                                    UserID = userId,
                                    UserName = userName,
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition")
                                };

                                if (user.Condition != "Online" && user.Condition == "")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if (user.Condition != "Online")
                                {
                                    user.Condition = "Был в сети " + user.Condition;
                                }

                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                        }
                    }
                }

                UserList.ItemsSource = users; // Установка ItemsSource после завершения цикла
                if (UserList.Items.IsEmpty)
                {
                    ZeroText.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ZeroText.Visibility = Visibility.Collapsed;
            AddFriendCode();
        }
        public void SendingReqestListCode()
        {
            ReqestPaneToMeFromSenderl.Visibility = Visibility.Collapsed;

            ReqestPaneOnMeSenderl.Visibility = Visibility.Visible;


            try
            {
                if (ReqestListOnMe == null)
                {
                    return;
                }

                List<User> users = new List<User>();
                Random random = new Random();

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = ($"SELECT FR.*, U.* FROM FriendRequests FR JOIN Users U ON FR.ReceiverID = U.UserID WHERE FR.SenderID = {UserID} AND FR.Status= 0");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string userName = reader.GetString("UserName");

                                if (userName == name)
                                {
                                    continue;
                                }

                                User user = new User
                                {
                                    UserID = userId,
                                    UserName = userName,
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition"),
                                    RequestsID = Convert.ToInt32(reader["RequestID"])
                                };

                                if (user.Condition != "Online" && user.Condition == "")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if (user.Condition != "Online")
                                {
                                    user.Condition = "Был в сети " + user.Condition;
                                }

                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                        }
                    }
                }

                ReqestListOnMe.ItemsSource = users; 
                if (ReqestListOnMe.Items.IsEmpty)
                {
                    ZeroText.Visibility = Visibility.Visible;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void getreqestListToUser()
        {
            FriendScroll.Visibility = Visibility.Collapsed;
            UserScroll.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            ClearSeacrh.Visibility = Visibility.Collapsed;

            ReqestPaneToMeFromSenderl.Visibility = Visibility.Visible;

            try
            {
                if (ReqestListToMe == null)
                {
                    return;
                }

                List<User> users = new List<User>();
                Random random = new Random();

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    connection.Open();

                    string query = ($"SELECT FR.*, U.* FROM FriendRequests FR JOIN Users U ON FR.SenderID = U.UserID WHERE FR.ReceiverID = {UserID} AND FR.Status=0;");

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string userName = reader.GetString("UserName");

                                if (userName == name)
                                {
                                    continue;
                                }

                                User user = new User
                                {
                                    UserID = userId,
                                    UserName = userName,
                                    DateOfRegistration = reader.GetDateTime("DateOfRegistration"),
                                    Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? DateTime.MinValue : reader.GetDateTime("Birthday"),
                                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "" : reader.GetString("Status"),
                                    Condition = reader.IsDBNull(reader.GetOrdinal("Сondition")) ? "" : reader.GetString("Сondition"),
                                    RequestsID = Convert.ToInt32(reader["RequestID"])
                                };

                                if (user.Condition != "Online" && user.Condition == "")
                                {
                                    user.Condition = "Не появлялся";
                                }
                                else if (user.Condition != "Online")
                                {
                                    user.Condition = "Был в сети " + user.Condition;
                                }

                                user.ColorUser = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                                users.Add(user);
                            }
                        }
                    }
                }

                ReqestListToMe.ItemsSource = users; // Установка ItemsSource после завершения цикла
                if (ReqestListToMe.Items.IsEmpty)
                {
                    ZeroText.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
            receivedBtn.Visibility = Visibility.Visible;
            sendingBtn.Visibility = Visibility.Visible;
            ZeroText.Visibility = Visibility.Collapsed;
            getreqestListToUser();


        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ZeroText.Visibility= Visibility.Collapsed;
            ReqestPaneOnMeSenderl.Visibility = Visibility.Collapsed;
            ReqestPaneToMeFromSenderl.Visibility = Visibility.Visible;

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ZeroText.Visibility= Visibility.Collapsed;
            SendingReqestListCode();
        }

        

        public void MakeHidenSomeelements()
        {
            receivedBtn.Visibility = Visibility.Collapsed;
            sendingBtn.Visibility = Visibility.Collapsed;
        }



    }
}
