using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
namespace Crypt
{
    public partial class MainWindow : Window
    {
        public static Process x;
        public static string temp1,temp2 = ""; public static String pwd = pass.pwd; public static bool passw = pass.passw;
        public static int keysize = settings.keysize; public static int block = settings.block; public static bool decomp = settings.decomp;
        public static int blo;
        public static bool go = false,cmmdp=false,v;
        public static long val, numBytesRead, fileOffset;
        public static double fac;
        public static FileStream fs = null, fss1 = null;
        public MainWindow()
        {
            InitializeComponent();
           // SetWorkingSet(30000,10000);
            keysize = 256;
            blo = 16640000;
            block = 1;
            decomp = false;
        }

        /* To move a borderless window
         * private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }*/


        //Encrytion method
        static byte[] EncryptStringToBytes(ref byte[] buffer, byte[] Key, byte[] IV, bool v)
        {
            Aes rijAlg = new AesCryptoServiceProvider();
            rijAlg.KeySize = keysize;
            rijAlg.Key = Key;
            rijAlg.IV = IV;
            //MessageBox.Show(rijAlg.KeySize.ToString());
            if (v == false)
                rijAlg.Padding = PaddingMode.None;
            else
                rijAlg.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            using (var stream = new MemoryStream())
            using (var encryptr = rijAlg.CreateEncryptor())
            using (var encrypt = new CryptoStream(stream, encryptr, CryptoStreamMode.Write))
            {
                encrypt.Write(buffer, 0, buffer.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        //Decryption method
        static byte[] DecryptStringFromBytes(ref byte[] buffer, byte[] Key, byte[] IV, bool v)
        {
            Aes rijAlg = new AesCryptoServiceProvider();
            rijAlg.KeySize = keysize;
            rijAlg.Key = Key;
            rijAlg.IV = IV;
            //MessageBox.Show(rijAlg.KeySize.ToString());
            if (v == false)
                rijAlg.Padding = PaddingMode.None;
            else
                rijAlg.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            using (var stream = new MemoryStream())
            using (var decryptor = rijAlg.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(buffer, 0, buffer.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        private void enc()
        {
            // MessageBox.Show(keysize.ToString() + "    " + block.ToString());
            //Stopwatch st = new Stopwatch();
            string[] temp = new string[2];
            string[] g = new string[2];
            for (int y = 0; y < datagrid1.Items.Count; y++)
            {
                pb1.Value = 0;  
                try
                {
                    datagrid1.SelectedIndex++;
                    temp2 = datagrid1.SelectedItem.ToString();
                    temp = temp2.Split(new string[] { "Col4 =" }, StringSplitOptions.None);
                    g = temp[1].Split(new string[] { " KB" }, StringSplitOptions.None);
                    pb1.Value = 0;
                    if(g[0]==" -1")
                    {
                        temp2 = datagrid1.SelectedItem.ToString();
                        temp = temp2.Split(new string[] { "Col3 = " }, StringSplitOptions.None);
                        g = temp[1].Split(new string[] { ", Col4 =" }, StringSplitOptions.None);
                        comp(g[0]);
                        g[0] = g[0] + ".7z";
                    }
                    else
                    {
                        temp2 = datagrid1.SelectedItem.ToString();
                        temp = temp2.Split(new string[] { "Col3 =" }, StringSplitOptions.None);
                        //MessageBox.Show(pwd);
                        g = temp[1].Split(new string[] { ", Col4 =" }, StringSplitOptions.None);
                    }

                    temp1 = Path.Combine(Path.GetDirectoryName(g[0]), Path.GetFileNameWithoutExtension(g[0]));
                    lb1.Content = "Encrypting --> " + Path.GetFileName(g[0]);
                    v = false;
                    //st.Start();
                    
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                    bw.WorkerReportsProgress = true;
                    bw.WorkerSupportsCancellation = true;
                    bw.RunWorkerAsync(g[0]);
                }
                catch (FileNotFoundException e4)
                {
                    
                    String sMessageBoxText = "File Not Found  \n\n" + g[0];
                    string sCaption = "Encryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                catch (Exception e3)
                {
                    String sMessageBoxText = "Some Random Error  \n\n" + e3;
                    string sCaption = "Encryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                finally
                {
                    
                }

            }
            pb1.Value = 0;
            datagrid1.Items.Clear();
            AnimateWindowHeight(this, 345.948);
            lb1.Content = "";
            bt1.IsEnabled = true;
            bt2.IsEnabled = true;
            bt3.IsEnabled = true;
            bt4.IsEnabled = true;
            if (go == true)
            {
                String sMessageBoxText = "Encrytion Complete \n\n"+"Time Taken "+"st.ElapsedMilliseconds/1000"+" seconds";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else
            {
                String sMessageBoxText = "Encrytion Interrupted";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //st.Stop();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] salt = Encoding.ASCII.GetBytes("Some f**king salt");
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pwd, salt);
            byte[] k = key.GetBytes(keysize / 8);
            byte[] i = key.GetBytes(16);
            string t = (string)e.Argument;
            try
            {
                fs = new FileStream(t, FileMode.Open);
                MessageBox.Show(fs.Length.ToString());
                t = t + ".caes";
                fss1 = new FileStream(t, FileMode.Create);
                val = 0; numBytesRead = 0; fileOffset = 0;
                fac = fs.Length / 100.00;
                MessageBox.Show(fs.Length.ToString());
                while (fileOffset < fs.Length && go == true)
                {
                    if ((fs.Length - fileOffset) < blo)
                    {
                        val = fs.Length - fileOffset;
                        v = true;
                    }
                    else
                        val = blo;
                    byte[] buffer = new byte[val];
                    fs.Seek(fileOffset, SeekOrigin.Begin);
                    numBytesRead = fs.Read(buffer, 0, buffer.Length);
                    fileOffset = fileOffset + numBytesRead;
                    byte[] encrypted = EncryptStringToBytes(ref buffer, k, i, v);
                    fss1.Write(encrypted, 0, encrypted.Length);
                    buffer = null;
                    encrypted = null;
                    Dispatcher.Invoke(() => pb1.Value = fileOffset / fac, DispatcherPriority.Send);
                }
                if (fs != null)
                {
                    fs.Close();
                }

                if (fss1 != null)
                {
                    fss1.Close();
                }
            }
            catch (FileNotFoundException e4)
            {

                String sMessageBoxText = "File Not Found  \n\n" + t;
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            catch (Exception e3)
            {
                String sMessageBoxText = "Some Random Error  \n\n" + e3;
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            fs = null; fss1 = null;
            GC.Collect();
        }



        private void dec()
        {
            //MessageBox.Show(keysize.ToString() + "    " + block.ToString());  
            Stopwatch st = new Stopwatch();
            Dispatcher.Invoke(() => datagrid1.SelectedIndex = 0, DispatcherPriority.Send);
            string[] f = new string[2]; string[] g = new string[2];
            bool v; string fg1, loc; long val, numBytesRead, fileOffset; double fac;string deloc;
            for (int y = 0; y < datagrid1.Items.Count; y++)
            {
                FileStream fs = null, fss2 = null; fg1 = "";
                try
                {
                    Dispatcher.Invoke(() => datagrid1.SelectedIndex++, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => temp2 = datagrid1.SelectedItem.ToString(), DispatcherPriority.Send);
                    f = temp2.Split(new string[] { "Col4 =" }, StringSplitOptions.None);
                    g = f[1].Split(new string[] { " KB" }, StringSplitOptions.None);
                    Dispatcher.Invoke(() => pb1.Value = 0, DispatcherPriority.Send);

                    if (g[0] == " -1")
                    {
                        Dispatcher.Invoke(() => pb1.Value = 0, DispatcherPriority.Send);
                        Dispatcher.Invoke(() => temp2 = datagrid1.SelectedItem.ToString(), DispatcherPriority.Send);
                        f = temp2.Split(new string[] { "Col3 = " }, StringSplitOptions.None);
                        //MessageBox.Show(pwd);
                        g = f[1].Split(new string[] { ", Col4 =" }, StringSplitOptions.None);
                        MessageBox.Show("g  " + g[0]);
                        comp(g[0]);
                        g[0] = g[0] + ".7z.CAES";
                        MessageBox.Show("final  " + g[0]);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => temp2 = datagrid1.SelectedItem.ToString(), DispatcherPriority.Send);
                        f = temp2.Split(new string[] { "Col3 =" }, StringSplitOptions.None);
                        //MessageBox.Show(pwd);
                        g = f[1].Split(new string[] { ", Col4 =" }, StringSplitOptions.None);

                    }

                    fg1 = Path.Combine(Path.GetDirectoryName(g[0]), Path.GetFileNameWithoutExtension(g[0]));
                    Dispatcher.Invoke(() => lb1.Content = "Decrypting --> " + Path.GetFileName(g[0]), DispatcherPriority.Send);
                    if (!File.Exists(fg1))
                    {
                        byte[] salt = Encoding.ASCII.GetBytes("Some f**king salt");
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pwd, salt);
                        byte[] k = key.GetBytes(keysize / 8);
                        byte[] i = key.GetBytes(16);
                        v = false;
                        st.Start();
                        fs = new FileStream(g[0], FileMode.Open);
                        fac = fs.Length / 100.00;
                        val = 0;
                        fileOffset = 0;
                        deloc = fg1;
                        MessageBox.Show(fg1);
                        fss2 = new FileStream(deloc, FileMode.Append);
                        while (fileOffset < fs.Length && go==true)
                        {
                            if ((fs.Length - fileOffset) < blo)
                            {
                                val = fs.Length - fileOffset;
                                v = true;
                            }
                            else
                                val = blo;
                            byte[] buffer = new byte[val];
                            fs.Seek(fileOffset, SeekOrigin.Begin);
                            numBytesRead = fs.Read(buffer, 0, buffer.Length);
                            fileOffset = fileOffset + numBytesRead;
                            byte[] roundtrip = DecryptStringFromBytes(ref buffer, k, i, v);
                            fss2.Write(roundtrip, 0, roundtrip.Length);
                            Dispatcher.Invoke(() => pb1.Value = fileOffset / fac, DispatcherPriority.Send);
                        }
                        fss2.Close();
                        st.Stop();
                        if(decomp==true)
                        {
                            decom(fg1);
                        }
                        //MessageBox.Show(st.ElapsedMilliseconds.ToString());
                        //MessageBox.Show("done ");
                    }
                    else
                    {
                        String sMessageBoxText = "File Already Exists  \n\n" + fg1;
                        string sCaption = "Decryption";
                        MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                        MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                        MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                    }
                }
                catch (CryptographicException e1)
                {
                    String sMessageBoxText = "Wrong Password  or Incorrect Key Size\n\n" + fg1;
                    string sCaption = "Decryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                catch (Exception e1)
                {
                    String sMessageBoxText = "Some Random Error  \n\n" + e1;
                    string sCaption = "Decryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                    if (fss2 != null)
                        fss2.Close();
                    Dispatcher.Invoke(() => bt1.IsEnabled = true, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => bt2.IsEnabled = true, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => bt3.IsEnabled = true, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => bt4.IsEnabled = true, DispatcherPriority.Send);
                }
            }
            Dispatcher.Invoke(() => pb1.Value = 0, DispatcherPriority.Send);
            Dispatcher.Invoke(() => datagrid1.Items.Clear(), DispatcherPriority.Send);
            Dispatcher.Invoke(() => lb1.Content = "", DispatcherPriority.Send);
            Dispatcher.Invoke(() => AnimateWindowHeight(this, 345.948), DispatcherPriority.Send);
            if (go == true)
            {
                String sMessageBoxText = "Decrytion Complete \n\n" + "Time Taken " + st.ElapsedMilliseconds / 1000 + " seconds";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else
            {
                String sMessageBoxText = "Decrytion Interrupted";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Stream myStream;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "d:\\";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;
            if (rb1.IsChecked == true)
            {
                ofd.Title = "Please Select Source File(s) for Encryption";
            }
            else
            {
                ofd.Filter = "CAES Files|*.caes";
                ofd.Title = "Please Select Source File(s) for Decryption";
            }
            if (ofd.ShowDialog() == true)
            {
                int v = 0, h; h = datagrid1.Items.Count;
                foreach (String file in ofd.FileNames)
                {
                    v++; h++;
                    if ((myStream = ofd.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            FileInfo info = new FileInfo(file);
                            datagrid1.Items.Add(new { Col1 = h, Col2 = Path.GetFileName(file), Col3 = file, Col4 = info.Length / 1024 + " KB" });
                        }
                    }
                }
            }
        }

        private void bt2_Click_1(object sender, RoutedEventArgs e)
        {
            if (rb1.IsChecked == true)
            {
                pass ps = new pass();
                ps.ShowDialog();
                pwd = pass.pwd;
                passw = pass.passw;
                if (passw == true)
                {
                    go = true;
                    bt1.IsEnabled = false;
                    bt2.IsEnabled = false;
                    bt3.IsEnabled = false;
                    bt4.IsEnabled = false;
                    AnimateWindowHeight(this, 490.994);
                    enc();
                }
            }
            else
            {
                pass ps = new pass();
                ps.ShowDialog();
                pwd = pass.pwd;
                passw = pass.passw;
                if (passw == true)
                {
                    go = true;
                    bt1.IsEnabled = false; 
                    bt2.IsEnabled = false;
                    bt3.IsEnabled = false;
                    bt4.IsEnabled = false;
                    AnimateWindowHeight(this, 490.994);
                    Thread newThread = new Thread(new ThreadStart(dec));
                    newThread.IsBackground = true;
                    newThread.SetApartmentState(ApartmentState.STA);
                    newThread.Priority = System.Threading.ThreadPriority.Highest;
                    newThread.Start();
                }
            }
        }

        private void rb1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                datagrid1.Items.Clear();
                bt6.IsEnabled = true;
            }
            catch (Exception )
            {

            }
        }

        private void rb2_Checked(object sender, RoutedEventArgs e)
        {
            datagrid1.Items.Clear();
            bt6.IsEnabled = false;
        }
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            AnimateWindowHeight(this, Application.Current.MainWindow.Height);
            base.OnChildDesiredSizeChanged(child);
        }

        private static void AnimateWindowHeight(Window window, double x)
        {
            window.BeginInit();
            window.SizeToContent = System.Windows.SizeToContent.Height;
            double height = x;
            window.SizeToContent = System.Windows.SizeToContent.Manual;
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                DoubleAnimation heightAnimation = new DoubleAnimation();
                heightAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                heightAnimation.From = window.ActualHeight;
                heightAnimation.To = height;
                heightAnimation.FillBehavior = FillBehavior.HoldEnd;
                window.BeginAnimation(Window.HeightProperty, heightAnimation);
            }), null);
            window.EndInit();
        }

        private void bt3_Click(object sender, RoutedEventArgs e)
        {
            while (datagrid1.SelectedItems.Count > 0)
            {
                datagrid1.Items.RemoveAt(datagrid1.SelectedIndex);
            }
            datagrid1.Items.Refresh();
        }
        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            e.Row.Height = 30;
        }

        private void datagrid1_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

        }

        private void bt4_Click(object sender, RoutedEventArgs e)
        {
            settings set = new settings();
            set.ShowDialog();
            keysize = settings.keysize;
            block = settings.block;
            decomp = settings.decomp;
            blo = 166400 * (int)(Math.Pow(10, settings.block + 1));

        }

        private void datagrid1_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int v = 0, h; h = datagrid1.Items.Count;
                foreach (String file in files)
                {
                    v++; h++;
                    FileInfo info = new FileInfo(file);
                    datagrid1.Items.Add(new { Col1 = h, Col2 = Path.GetFileName(file), Col3 = file, Col4 = info.Length / 1024 + " KB" });
                }
            }
        }

        private void bt5_Click(object sender, RoutedEventArgs e)
        {
            go = false;
        }

        private void bt6_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            ofd.ShowDialog();
            string fold = ofd.SelectedPath;
            if(fold!="")
            {
                int v = 0, h; h = datagrid1.Items.Count;
                String file = fold;
                v++; h++;
                FileInfo info = new FileInfo(file);
                datagrid1.Items.Add(new { Col1 = h, Col2 = fold, Col3 = file, Col4 = -1 + " KB" });        
            } 
        }
        public void comp(String fold)
        {
            //MessageBox.Show("flolil "+fold);
            string sourceName = "\"" + fold + "\"";
            string targetName = "\"" + fold + ".7z" + "\" ";
            MessageBox.Show(sourceName + "        " + targetName);
            cmmdp = true;
            MessageBox.Show("a " + targetName + sourceName + " -r -mx9 -t7z");
            x.StartInfo.WorkingDirectory = @"c:";
            x.StartInfo.FileName = "7za.exe";
            x.StartInfo.Arguments = "a " + targetName  + sourceName + " -r -mx9 -t7z";
            x.Start();
            x.WaitForExit();
            cmmdp = false;
        }
        public void decom(String fold)
        {
            //MessageBox.Show("flolil " + fold);
            string sourceName = "\"" + fold + "\"";
            fold = fold.Remove(fold.Length - 3);
            string targetName = "\"" + fold + "\"";
            //MessageBox.Show("99"+sourceName + "999" + targetName+"99");
            cmmdp = true;
            x.StartInfo.FileName = "C:\\Users\\Siddharth\\Desktop\\7za.exe";
            x.StartInfo.Arguments = @"x " + sourceName + " -o" + targetName;
            x.Start();
            x.WaitForExit();
            cmmdp = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBox.Show(cmmdp.ToString());
            if(cmmdp==true)
                x.Close();
        }
    }
}