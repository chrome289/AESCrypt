using System;
using System.Windows;

namespace Crypt {
    /// <summary>
    /// Interaction logic for pass.xaml
    /// </summary>
    public partial class pass : Window {
        public static String pwd ; public static bool passw;
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
                if (!App.calledFromMainWin) {
                    sendTo st = new sendTo();
                    st.Show();
                }
                this.Close();
            }
            else {
                //MessageBox.Show("DSFsd");
                lb1.Content = "Password not Accepted\n" + lb1.Content;
                passw = false;
            }
        }


        private void checkBox_Checked(object sender, RoutedEventArgs e) {
            textBox.IsEnabled = true;
            packageFiles = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e) {
            textBox.IsEnabled = false;
            packageFiles = false;
        }
    }
}
