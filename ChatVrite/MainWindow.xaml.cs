using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using System.Windows.Controls;


namespace ChatVrite
{
    public partial class MainWindow : Window
    {
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        public MainWindow()
        {
            InitializeComponent();
            //CheckDbConnection();

            if (SessionManager.IsUserAuthenticated())
            {
                string username = SessionManager.Username;
                string password = SessionManager.GetSavedPassword();
                avtoses(username, password);
            }

        }

        private void CheckDbConnection()
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    // Если подключение успешно, можно выполнить дополнительные действия или просто вывести сообщение об успешном подключении.
                    MessageBox.Show("Соединение с базой данных установлено!");
                }
                catch (MySqlException ex)
                {
                    ErrorText.Text = "Ошибка подключения к БД: В данной сети нет доступа, обратитесь к Юрию. ";
                }
            }
        }
    
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginEmail.Text;
            string password = LoginPassword.Password;
            PasswordHashing hasher = new PasswordHashing();
            string hashedPassword = hasher.HashPassword(password);

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM Users WHERE Email = @email AND Password = @password", connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", hashedPassword);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"].ToString();
                            SessionManager.SetUserCredentials(username, hashedPassword);
                            OpenPersonalWindow(username);
                        }
                        else
                        {
                            MessageBox.Show("Вход не выполнен. Пожалуйста, проверьте ваши учетные данные.");
                            
                        }
                    }
                }
            }
        }

        private void avtoses(string name,string pass)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM Users WHERE Username = @name AND Password = @password", connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@password", pass);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            OpenPersonalWindow(name);

                        }
                        else
                        {
                            MessageBox.Show("Данные последней сессии были утеряны!");
                        }
                    }
                }
            }
        }

        private void OpenPersonalWindow(string username)
        {
            PersonalWindow personalWindow = new PersonalWindow(username);
            this.Close();
            personalWindow.ShowDialog();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsername.Text;
            string email = RegisterEmail.Text;
            string password = RegisterPassword.Password;




            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (username.Length > 10)
            {
                MessageBox.Show("Имя пользователя слишком длинное. Пожалуйста, используйте имя длиной не более 10 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsValidEmail(email))
            {
                if (email.Length > 30)
                {
                    MessageBox.Show("Адрес электронной почты слишком длинный. Пожалуйста, используйте адрес не более 30 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CheckPasswordStrength(password))
                {
                    if (password.Length > 20)
                    {
                        MessageBox.Show("Пароль слишком длинный. Пожалуйста, используйте пароль не более 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }




                    using (MySqlConnection connection = new MySqlConnection(DbConnection))
                    {
                        connection.Open();

                        // Проверка уникальности имени пользователя
                        using (MySqlCommand checkUsernameCommand = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @username", connection))
                        {
                            checkUsernameCommand.Parameters.AddWithValue("@username", username);
                            int usernameCount = Convert.ToInt32(checkUsernameCommand.ExecuteScalar());

                            if (usernameCount > 0)
                            {
                                MessageBox.Show("Пользователь с таким именем уже существует. Пожалуйста, выберите другое имя пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        string verificationCode = GenerateVerificationCode(); // Генерация кода
                        SendVerificationEmail(email, verificationCode); // Отправка письма

                        // Отобразить диалоговое окно для ввода кода
                        VerificationCodeDialog dialog = new VerificationCodeDialog();
                        if (dialog.ShowDialog() == true)
                        {
                            string enteredCode = dialog.VerificationCode;
                            PasswordHashing hasher = new PasswordHashing();
                            string hashedPassword = hasher.HashPassword(password);
                            // Сохранение hashedPassword в базе данных


                            if (enteredCode == verificationCode)
                            {
                                try
                                {
                                    using (MySqlCommand command = new MySqlCommand("INSERT INTO Users (Username, Email, Password, DateOfRegistration, Birthday, City, Status) VALUES (@username, @email, @password, @registrationDate, @birthday, @city, @status)", connection))
                                    {
                                        command.Parameters.AddWithValue("@username", username);
                                        command.Parameters.AddWithValue("@email", email);
                                        command.Parameters.AddWithValue("@password", hashedPassword);
                                        command.Parameters.AddWithValue("@registrationDate", DateTime.Now);
                                        command.Parameters.AddWithValue("@birthday", DBNull.Value);
                                        command.Parameters.AddWithValue("@city", DBNull.Value);
                                        command.Parameters.AddWithValue("@status", DBNull.Value);

                                        int rowsInserted = command.ExecuteNonQuery();

                                        if (rowsInserted > 0)
                                        {
                                            MessageBox.Show("Регистрация выполнена успешно!");
                                            tabcon.SelectedIndex = 0;
                                            LoginEmail.Text = email;
                                            LoginPassword.Focus();

                                        }
                                        else
                                        {
                                            MessageBox.Show("Регистрация не выполнена. Пожалуйста, попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Регистрация не выполнена. Пожалуйста, попробуйте еще раз. {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                            }
                            else
                            {
                                MessageBox.Show("Неправильный пятизначный код. Пожалуйста, повторите попытку.");
                                return;
                                // Дополнительные действия при неправильном коде
                            }
                        }
                        else
                        {
                            MessageBox.Show("Регистрация отменена.");
                            // Дополнительные действия при отмене регистрации
                        }


                    }
                }
                else
                {
                    MessageBox.Show("Пароль слишком простой. Пожалуйста, выберите более надежный пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неправильный формат адреса электронной почты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendVerificationEmail(string userEmail, string verificationCode)
        {
            try
            {
                EmailService emailService = new EmailService();
                emailService.SendVerificationCode(userEmail, verificationCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отправить письмо. Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateVerificationCode()
        {
            // Генерация случайного пятизначного кода
            Random random = new Random();
            int verificationCode = random.Next(10000, 99999);
            return verificationCode.ToString();
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckPasswordStrength(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*?[A-Z])(?=.*?[a-z]).{6,}$");
        }

        private void RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = RegisterPassword.Password;
            string passwordStrength = "Слабый";

            if (CheckPasswordStrength(password))
            {
                passwordStrength = "Сильный";
            }

            PasswordStrength.Text = "Уровень сложности пароля: " + passwordStrength;
        }

        private void RegisterEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string email = RegisterEmail.Text;
            if (IsValidEmail(email))
            {
                RegisterEmail.BorderBrush = Brushes.Green;
            }
            else
            {
                RegisterEmail.BorderBrush = Brushes.Red;
            }
        }



        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = e.AddedItems[0] as TabItem;

            if (selectedTab != null)
            {
                if (selectedTab.Header.ToString() == "Выход")
                {
                    SessionManager.ClearUserData(); // Очистка данных о пользователе при выходе
                    Application.Current.Shutdown();
                }
            }
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RecoveryPasswordWindow recoveryPasswordWindow = new RecoveryPasswordWindow();
            recoveryPasswordWindow.ShowDialog();
        }
    }
}
