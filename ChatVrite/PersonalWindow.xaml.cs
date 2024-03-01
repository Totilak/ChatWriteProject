using System.Windows;
using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using System.Windows.Controls;

namespace ChatVrite
{
    public partial class PersonalWindow : Window
    {
       public string name;
        public const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        public PersonalWindow(string userName)
        {
            InitializeComponent();
            name = userName;
            try
            {LoadUserDataFromDatabase();}catch (Exception ex){ UserNameLabel.Text = "Привет, " + userName + "!"; }
            


            // Установить имя пользователя и дополнительные данные
            //
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            EditProfileWindow editProfileWindow = new EditProfileWindow(name);
            editProfileWindow.ProfileUpdated += EditProfileWindow_ProfileUpdated; // Подписываемся на событие
            editProfileWindow.ShowDialog();
        }

        private void EditProfileWindow_ProfileUpdated(object sender, EventArgs e)
        {
            // Здесь выполните загрузку данных после обновления профиля
            LoadUserDataFromDatabase();
        }

        private void EditConditionStatusInBaseTry(bool condition)
        {
            try
            {
                EditConditionsStatusInBase(condition);
              //  MessageBox.Show($"Успешно установили статус на  {condition}!");


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены статуса. {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void EditConditionsStatusInBase(bool condition)
        {
            string ConditionString;
            if(condition)
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Open_Chats(object sender, RoutedEventArgs e)
        {
            Chat chat = new Chat(name);
            this.Close();
            chat.ShowDialog();
        }
        private void LoadUserDataFromDatabase()
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserName, Status, City, Birthday FROM Users WHERE Username = @UserName";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", name);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userName = reader.GetString("UserName");
                                string status = reader.GetString("Status");
                                string city = reader.GetString("City");
                                DateTime birthday = reader.GetDateTime("Birthday");

                                UserNameLabel.Text = userName;
                                Status.Text = "Статус: " + status;
                                City.Text = "Город: " + city;
                                DateBithday.Text = "День рождения: " + birthday.ToString("dd.MM.yyyy");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка при получении данных пользователя: {ex.Message}");
                }
            }
        }

        private void Change(object sender, RoutedEventArgs e)
        {
            SessionManager.ClearUserData(); // Очистка данных о пользователе при выходе
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            EditConditionStatusInBaseTry(false);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EditConditionStatusInBaseTry(true);
        }
    }
}
