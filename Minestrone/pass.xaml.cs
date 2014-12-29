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

namespace Crypt
{
    /// <summary>
    /// Interaction logic for pass.xaml
    /// </summary>
    public partial class pass : Window
    {
        public static String pwd = MainWindow.pwd; public static bool passw = MainWindow.passw;
        public pass()
        {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            passw = false;
            if (tb1.Password.Length > 7)
            {
                pwd = tb1.Password;
                passw = true;
                this.Close();
            }
            else
            {
                //MessageBox.Show("DSFsd");
                lb1.Content = lb1.Content + "\n\nPassword not Accepted";
                passw = false;
            }
        }
    }
}
