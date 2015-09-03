using System.Windows;

namespace Crypt
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Window
    {
        public static int keysize; public static int block; public static bool decomp;
        public settings()
        {
            InitializeComponent();
            keysize = MainWindow.keysize;
            decomp = MainWindow.decomp;
            cb1.SelectedIndex = (keysize - 128) / 64;
            cb2.SelectedIndex = block;
            ch1.IsChecked = decomp;
        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            keysize = 128+(cb1.SelectedIndex)*64;
            block = (cb2.SelectedIndex);
            this.Close();
            //MessageBox.Show(block.ToString());
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ch1.IsChecked == true)
                decomp = true;
            else
                decomp = false;
        }
    }
}
