using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для RecoveryPasswordWindow.xaml
    /// </summary>
    public partial class RecoveryPasswordWindow : Window
    {
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";
        string EmaiUser="null";
        public RecoveryPasswordWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string Email = EmailInput.Text;

            string verificationCode = GenerateVerificationCode(); // Генерация кода
            SendVerificationEmail(Email, verificationCode); // Отправка письма

            // Отобразить диалоговое окно для ввода кода
            VerificationCodeDialog dialog = new VerificationCodeDialog();
            if (dialog.ShowDialog() == true)
            {
                string enteredCode = dialog.VerificationCode;

                if (enteredCode == verificationCode)
                {
                    EmaiUser = Email;
                    EmailPanel.Visibility = Visibility.Collapsed;
                    PasswordPanel.Visibility = Visibility.Visible;

                }
            }
         }

        private string GenerateVerificationCode()
        {
            // Генерация случайного пятизначного кода
            Random random = new Random();
            int verificationCode = random.Next(10000, 99999);
            return verificationCode.ToString();
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
        private bool CheckPasswordStrength(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*?[A-Z])(?=.*?[a-z]).{6,}$");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string password = RegisterPassword.Password;
            string passwordRetry = RegisterPasswordRetry.Password;
            if (password != passwordRetry)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK);
                return;
            }

            if (password.Length > 20)
            {
                MessageBox.Show("Пароль слишком длинный. Пожалуйста, используйте пароль не более 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий. Пожалуйста, используйте в пароле не менее 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PasswordHashing hasher = new PasswordHashing();
            string hashedPassword = hasher.HashPassword(password);

            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                using (MySqlCommand command = new MySqlCommand("UPDATE `Users` SET `Password` = @Password WHERE `Users`.`Email` = @UserEmail;", connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", EmaiUser);
                    command.Parameters.AddWithValue("@Password", hashedPassword);

                    connection.Open(); // Open the connection

                    int rowsUpdated = command.ExecuteNonQuery(); // Execute the update

                    if (rowsUpdated == 1) // Check if exactly one row was updated
                    {
                        MessageBox.Show("Успешная смена пароля!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при смене пароля. Пожалуйста, попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}
