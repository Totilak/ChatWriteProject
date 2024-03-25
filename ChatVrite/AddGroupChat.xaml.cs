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
    /// Логика взаимодействия для AddGroupChat.xaml
    /// </summary>
    public partial class AddGroupChat : Window
    {
       public string name;
       public  int id;
        private const string DbConnection = "server=45.93.200.175; port=3306; username=toti; password=Toti345; database=pi";

        public AddGroupChat(string nameuser, int iduser)
        {
            InitializeComponent();
            name = nameuser;
            id = iduser;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string GroupName = ChatNameTextBox.Text;
            string GroupDescrption = ChatDescriptionTextBox.Text;

            if (!string.IsNullOrWhiteSpace(GroupName) && !string.IsNullOrWhiteSpace(GroupDescrption))
            {
                int groupId = -1; // Переменная для хранения ID только что созданной беседы

                using (MySqlConnection connection = new MySqlConnection(DbConnection))
                {
                    try
                    {
                        connection.Open();
                        string query = "INSERT INTO `GroupChats` (`Name`, `Description`, `GorupCreaterId`) VALUES (@Name, @Description, @GorupCreaterId); SELECT LAST_INSERT_ID();";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", GroupName);
                            command.Parameters.AddWithValue("@Description", GroupDescrption);
                            command.Parameters.AddWithValue("@GorupCreaterId", id); 
                            groupId = Convert.ToInt32(command.ExecuteScalar());
                        }


                        AddCreatorInyourGroup(groupId);
                        MessageBox.Show($"Успешно создали новую беседу!");

                        this.Close();
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show($"Ошибка создания беседы: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show($"Вы ввели не коректные данные!");
            }
        }


        private void AddCreatorInyourGroup(int groupId)
        {
            using (MySqlConnection connection = new MySqlConnection(DbConnection))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO `GroupChatMembers`  (`GroupChatId`, `UserId`) VALUES (@GroupChatId, @UserId)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupChatId", groupId);
                        command.Parameters.AddWithValue("@UserId", id);
                        groupId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка создания беседы: {ex.Message}");
                }
            }
        }

    }
}
