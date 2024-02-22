using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using System.Windows.Controls;

namespace ChatVrite
{
    /// <summary>
    /// Логика взаимодействия для EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        string nameme;
        public event EventHandler ProfileUpdated;

        public EditProfileWindow(string name)
        {
            InitializeComponent();
            nameme = name;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try { LoadUserDataFromDatabase(); } catch(Exception ez) { };
            
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
                        command.Parameters.AddWithValue("@UserName", nameme);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userName = reader.GetString("UserName");
                                string status = reader.GetString("Status");
                                string city = reader.GetString("City");
                                DateTime birthday = reader.GetDateTime("Birthday");

                                StatusTextBox.Text = status;
                                CityTextBox.Text = city;
                                BirthdayDatePicker.SelectedDate = birthday;
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string status = StatusTextBox.Text;
            string city = CityTextBox.Text;
            DateTime birthday = BirthdayDatePicker.SelectedDate.Value;

            //  Код для выполнения SQL-запроса и обновления данных в базе данных
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Users SET Status = @Status, City = @City, Birthday = @Birthday WHERE Username = @UserName";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@City", city);
                        command.Parameters.AddWithValue("@Birthday", birthday);
                        command.Parameters.AddWithValue("@UserName", nameme);
                        command.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Ошибка при обновлении данных пользователя: {ex.Message}");
                }
            }

            // Закрыть окно редактирования
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ProfileUpdated != null)
            {
                ProfileUpdated(this, EventArgs.Empty);
            }

        }
    }
}
