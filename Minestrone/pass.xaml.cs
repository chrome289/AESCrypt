using System;
using System.Windows;

namespace Crypt {
    /// <summary>
    /// Interaction logic for pass.xaml
    /// </summary>
    public partial class pass : Window {
        public static String pwd = MainWindow.pwd; public static bool passw = MainWindow.passw;
        public pass() {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {
            passw = false;
            if (tb1.Password.Length > 7) {
                pwd = tb1.Password;
                passw = true;
                this.Close();
            }
            else {
                //MessageBox.Show("DSFsd");
                lb1.Content = lb1.Content + "\n\nPassword not Accepted";
                passw = false;
            }
        }

        
        private void checkBox_Checked(object sender, RoutedEventArgs e) {
            if (checkBox.IsChecked==true)
                textBox.IsEnabled = true;
            else
                textBox.IsEnabled = false;
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e) {
            textBox.Text = "";
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e) {
            textBox.Text = "Enter Name";
        }
    }
}
