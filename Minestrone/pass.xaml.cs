using System;
using System.Windows;

namespace Crypt {
    /// <summary>
    /// Interaction logic for pass.xaml
    /// </summary>
    public partial class pass : Window {
        public static String pwd = MainWindow.pwd; public static bool passw = MainWindow.passw;
        public static bool packageFiles = false;
        public static String folderName;
        public pass() {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {
            passw = false;
            if (tb1.Password.Length > 7) {
                pwd = tb1.Password;
                passw = true;
                folderName = textBox.Text;
                sendTo st = new sendTo();
                st.Show();
                this.Close();
            }
            else {
                //MessageBox.Show("DSFsd");
                lb1.Content = lb1.Content + "\n\nPassword not Accepted";
                passw = false;
            }
        }

        
        private void checkBox_Checked(object sender, RoutedEventArgs e) {
            if (checkBox.IsChecked == true) {
                textBox.IsEnabled = true;
                packageFiles = true;
            }
            else {
                textBox.IsEnabled = false;
                packageFiles = false;
            }
        }
    }
}
