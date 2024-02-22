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
    public partial class VerificationCodeDialog : Window
    {
        public string VerificationCode { get; private set; }

        public VerificationCodeDialog()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            VerificationCode = VerificationCodeInput.Text;
            DialogResult = true; // Ррезультат диалога как успешный
            Close(); // Закрываем окно
        }
    }
}