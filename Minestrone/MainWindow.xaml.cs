using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Collections.Generic;

namespace Crypt {
    public partial class MainWindow : Window {
        static ProcessStartInfo processStartInfo;
        static Process process;
        static string temp1, temp2 = "";
        public static int keysize, blo;
        public static bool decomp = settings.decomp;
        static bool go = false, cmmdp = false, v;
        static long val, numBytesRead, fileOffset;
        static double fac;
        static FileStream fs = null, fss1 = null;
        static Stopwatch st = new Stopwatch();
        static List<String> path = new List<string>(),filename = new List<string>();
        static List<int> size = new List<int>();

        public MainWindow() {
            InitializeComponent();
            keysize = 128;
            blo = 8320000;
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
        static byte[] EncryptStringToBytes(ref byte[] buffer, byte[] Key, byte[] IV, bool v) {
            Aes rijAlg = new AesCryptoServiceProvider();
            rijAlg.KeySize = keysize;
            rijAlg.Key = Key;
            rijAlg.IV = IV;
            if (v == false)
                rijAlg.Padding = PaddingMode.None;
            else
                rijAlg.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            using (var stream = new MemoryStream())
            using (var encryptr = rijAlg.CreateEncryptor())
            using (var encrypt = new CryptoStream(stream, encryptr, CryptoStreamMode.Write)) {
                encrypt.Write(buffer, 0, buffer.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        //Decryption method
        static byte[] DecryptStringFromBytes(ref byte[] buffer, byte[] Key, byte[] IV, bool v) {
            Aes rijAlg = new AesCryptoServiceProvider();
            rijAlg.KeySize = keysize;
            rijAlg.Key = Key;
            rijAlg.IV = IV;
            if (v == false)
                rijAlg.Padding = PaddingMode.None;
            else
                rijAlg.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            using (var stream = new MemoryStream())
            using (var decryptor = rijAlg.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write)) {
                encrypt.Write(buffer, 0, buffer.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        //started a background thread to perform encrypt
        private void enc() {
            st.Reset();
            st.Start();
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        //updating progress
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            pb1.Value = e.ProgressPercentage;
        }

        //run after background thread exists
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            st.Stop();

            // restoring default GUI
            pb1.Value = 0;
            datagrid1.Items.Clear();
            AnimateWindowHeight(this, 365.948);
            lb1.Content = "";
            bt1.IsEnabled = true; bt2.IsEnabled = true; bt3.IsEnabled = true; bt4.IsEnabled = true;
            if (go == true) {
                String sMessageBoxText = "Encrytion Complete \n\n" + "Time Taken " + st.ElapsedMilliseconds / 1000 + " seconds";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else {
                String sMessageBoxText = "Encrytion Interrupted";
                string sCaption = "Encryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            GC.Collect();
        }

        //main background thread
        private void bw_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int y = 0; y < path.Count; y++) {
                bool delete = false;
                worker.ReportProgress(0);

                //zip if selection is a folder
                if (size[y] == -1) {
                    Dispatcher.Invoke(() => lb1.Content = "Packing the Folder into an archive --> " + filename[y], DispatcherPriority.Send);
                    comp(path[y]);
                    path[y] = path[y] + ".zip";
                    delete = true;
                }
                Dispatcher.Invoke(() => lb1.Content = "Encrypting --> " + filename[y], DispatcherPriority.Send);

                //preparing salt and key for encryption
                v = false;
                byte[] salt = Encoding.ASCII.GetBytes(pass.pwd);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pass.pwd, salt);
                byte[] k = key.GetBytes(keysize / 8);
                byte[] i = key.GetBytes(16);
                try {
                    //opening I/O streams
                    fs = new FileStream(path[y], FileMode.Open);
                    path[y] = path[y] + ".caes";
                    fss1 = new FileStream(path[y], FileMode.Create);
                    val = 0; numBytesRead = 0; fileOffset = 0;
                    fac = fs.Length / 100.00;

                    //encryption
                    while (fileOffset < fs.Length && go == true) {
                        if ((fs.Length - fileOffset) < blo) {
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
                        worker.ReportProgress((int)(fileOffset / fac));
                    }

                    //closing I/O streams
                    if (fs != null) {
                        fs.Close();
                    }

                    if (fss1 != null) {
                        fss1.Close();
                    }

                    //deleting the temp. archive
                    if (delete == true) {
                        delete = false;
                        File.Delete(path[y].Substring(0, path[y].Length - 5));
                    }
                }
                catch (FileNotFoundException e4) {
                    String sMessageBoxText = "File Not Found  \n\n" + path[y];
                    string sCaption = "Encryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                catch (Exception e3) {
                    String sMessageBoxText = "Some Random Error  \n\n" + e3;
                    string sCaption = "Encryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                finally {
                    if (fs != null)
                        fs.Close();
                    if (fss1 != null)
                        fss1.Close();
                }
            }
        }


        private void dec() {
            st.Reset();
            st.Start();
            BackgroundWorker debw = new BackgroundWorker();
            debw.DoWork += new DoWorkEventHandler(debw_DoWork);
            debw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(debw_RunWorkerCompleted);
            debw.ProgressChanged += new ProgressChangedEventHandler(debw_ProgressChanged);
            debw.WorkerReportsProgress = true;
            debw.WorkerSupportsCancellation = true;
            debw.RunWorkerAsync();
        }

        private void debw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            pb1.Value = e.ProgressPercentage;
        }

        private void debw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            st.Stop();

            // restoring default GUI
            pb1.Value = 0;
            datagrid1.Items.Clear();
            AnimateWindowHeight(this, 365.948);
            lb1.Content = "";
            bt1.IsEnabled = true; bt2.IsEnabled = true; bt3.IsEnabled = true; bt4.IsEnabled = true;
            if (go == true) {
                String sMessageBoxText = "Decrytion Complete \n\n" + "Time Taken " + st.ElapsedMilliseconds / 1000 + " seconds";
                string sCaption = "Decryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else {
                String sMessageBoxText = "Decrytion Interrupted";
                string sCaption = "Decryption";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            GC.Collect();
        }

        private void debw_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int y = 0; y < path.Count; y++) {
                try {
                    worker.ReportProgress(0);

                    Dispatcher.Invoke(() => lb1.Content = "Decrypting --> " + Path.GetFileName(filename[y]), DispatcherPriority.Send);

                    //preparing salt and key for encryption
                    v = false;
                    byte[] salt = Encoding.ASCII.GetBytes(pass.pwd);
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pass.pwd, salt);
                    byte[] k = key.GetBytes(keysize / 8);
                    byte[] i = key.GetBytes(16);

                    //opening I/O streams
                    fs = new FileStream(path[y], FileMode.Open);
                    path[y] = path[y].Remove(path[y].Length - 5);
                    if (!File.Exists(path[y])) {
                        fss1 = new FileStream(path[y], FileMode.Create);
                        val = 0; numBytesRead = 0; fileOffset = 0;
                        fac = fs.Length / 100.00;

                        //decryption
                        while (fileOffset < fs.Length && go == true) {
                            if ((fs.Length - fileOffset) < blo) {
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
                            fss1.Write(roundtrip, 0, roundtrip.Length);
                            worker.ReportProgress((int)(fileOffset / fac));
                        }

                        //closing I/O streams
                        if (fs != null) {
                            fs.Close();
                        }

                        if (fss1 != null) {
                            fss1.Close();
                        }

                        //decompressing if file is zip archive
                        if (Path.GetExtension(path[y]) == ".zip" && decomp == true) {
                            Dispatcher.Invoke(() => lb1.Content = "Decompressing the archive --> " + Path.GetFileName(path[y]), DispatcherPriority.Send);
                            decom(path[y]);
                        }
                    }
                    else {
                        if (fss1 != null)
                            fss1.Close();
                        go = false;
                        String sMessageBoxText = "File Already Exists  \n\n" + path[y];
                        string sCaption = "Decryption";
                        MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                        MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                        MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                    }
                }
                catch (CryptographicException e1) {
                    go = false;
                    fss1.Close();
                    if (File.Exists(path[y]))
                        File.Delete(path[y]);
                    String sMessageBoxText = "Wrong Password  or Incorrect Key Size\n\n" + path[y];
                    string sCaption = "Decryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                catch (Exception e1) {
                    go = false;
                    String sMessageBoxText = "Some Random Error  \n\n" + e1;
                    string sCaption = "Decryption";
                    MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                }
                finally {
                    if (fs != null) {
                        fs.Close();
                    }

                    if (fss1 != null) {
                        fss1.Close();
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Stream myStream;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "d:\\";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;
            if (rb1.IsChecked == true) {
                ofd.Title = "Please Select File(s) for Encryption";
            }
            else if(rb2.IsChecked== true) {
                ofd.Filter = "CAES Files|*.caes";
                ofd.Title = "Please Select File(s) for Decryption";
            }
            else {
                ofd.Title = "Please Select File(s) for Shredding";
            }
            if (ofd.ShowDialog() == true) {
                int v = 0, h; h = datagrid1.Items.Count;
                foreach (String file in ofd.FileNames) {
                    v++; h++;
                    if ((myStream = ofd.OpenFile()) != null) {
                        using (myStream) {
                            FileInfo info = new FileInfo(file);
                            datagrid1.Items.Add(new { Col1 = h, Col2 = Path.GetFileName(file), Col3 = file, Col4 = Math.Round((double)(info.Length / 1024.0 / 1024.0), 3) + " MB" });
                        }
                    }
                }
            }
        }

        private void bt2_Click_1(object sender, RoutedEventArgs e) {

            pass ps = new pass();
            ps.ShowDialog();
            if (pass.passw == true) {
                go = true;
                bt1.IsEnabled = false; bt2.IsEnabled = false; bt3.IsEnabled = false; bt4.IsEnabled = false;

                //preparing arraylists
                path.Clear();filename.Clear();size.Clear();
                String[] temp, g;
                for (int x = 0; x < datagrid1.Items.Count; x++) {
                    datagrid1.SelectedIndex++;
                    temp2 = datagrid1.SelectedItem.ToString();

                    temp = temp2.Split(new string[] { "Col4 =" }, StringSplitOptions.None);
                    g = temp[1].Split(new string[] { " KB" }, StringSplitOptions.None);
                    temp = temp2.Split(new string[] { "Col3 = " }, StringSplitOptions.None);
                    g = temp[1].Split(new string[] { ", Col4 =" }, StringSplitOptions.None);

                    path.Add(g[0]);
                    filename.Add(Path.GetFileName(g[0]));
                    FileAttributes att = File.GetAttributes(g[0]);
                    if ((att & FileAttributes.Directory) == FileAttributes.Directory)
                        size.Add(-1);
                    else
                        size.Add((int)new FileInfo(g[0]).Length);
                }

                AnimateWindowHeight(this, 490.994);
                if (rb1.IsChecked == true)
                    enc();
                else if (rb2.IsChecked == true)
                    dec();
                else
                    shredder();
            }
        }

        private void shredder() {
            st.Reset();
            st.Start();
            BackgroundWorker shbw = new BackgroundWorker();
            shbw.DoWork += new DoWorkEventHandler(shredderBW);
            shbw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(shredderBW_RunWorkerCompleted);
            shbw.ProgressChanged += new ProgressChangedEventHandler(shredderBW_ProgressChanged);
            shbw.WorkerReportsProgress = true;
            shbw.WorkerSupportsCancellation = true;
            shbw.RunWorkerAsync();
        }
        private void shredderBW_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            pb1.Value = e.ProgressPercentage;
        }

        private void shredderBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            st.Stop();

            // restoring default GUI
            pb1.Value = 0;
            datagrid1.Items.Clear();
            AnimateWindowHeight(this, 365.948);
            lb1.Content = "";
            bt1.IsEnabled = true; bt2.IsEnabled = true; bt3.IsEnabled = true; bt4.IsEnabled = true;
            if (go == true) {
                String sMessageBoxText = "Shredding Complete \n\n" + "Time Taken " + st.ElapsedMilliseconds / 1000 + " seconds";
                string sCaption = "Shredding";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else {
                String sMessageBoxText = "Shredding Interrupted";
                string sCaption = "Shredding";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            GC.Collect();
        }

        private void shredderBW(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int y = 0; y < path.Count; y++) {
                Dispatcher.Invoke(() => lb1.Content = "Shredding --> " + Path.GetFileName(filename[y]), DispatcherPriority.Send);
                FileStream soDead = new FileStream(path[y], FileMode.Open, FileAccess.ReadWrite);
                int bufferSize = 4096, cap = (int)soDead.Length,noOfPasses=3;
                byte[] garbage = new byte[bufferSize];
                Random ran = new Random(1000000);
                for (int i = 0; i < noOfPasses; i++) {
                    for (int x = 0; x < cap; x += bufferSize) {
                        ran.NextBytes(garbage);
                        soDead.Write(garbage, 0, bufferSize);
                        soDead.Seek(x + bufferSize, SeekOrigin.Begin);
                        soDead.Flush();
                        worker.ReportProgress(((i * (x / bufferSize)) / noOfPasses * (cap / bufferSize)) / 100);
                    }

                }
                soDead.Close();soDead.Dispose();
                String randomName = "";
                for (int x = 0; x < 10; x++)
                    randomName += (char)ran.Next(65, 90);
                File.Move(path[y], Environment.SpecialFolder.ApplicationData + randomName);
                File.Delete(Environment.SpecialFolder.ApplicationData + randomName);
            }
        }

        private void rb1_Checked(object sender, RoutedEventArgs e) {
            try {
                datagrid1.Items.Clear();
                bt6.IsEnabled = true;
            }
            catch (Exception) {

            }
        }

        private void rb2_Checked(object sender, RoutedEventArgs e) {
            datagrid1.Items.Clear();
            bt6.IsEnabled = false;
        }
        protected override void OnChildDesiredSizeChanged(UIElement child) {
            AnimateWindowHeight(this, Application.Current.MainWindow.Height);
            base.OnChildDesiredSizeChanged(child);
        }

        private static void AnimateWindowHeight(Window window, double x) {
            //window.BeginInit();
            window.SizeToContent = SizeToContent.Height;
            double height = x;
            window.SizeToContent = SizeToContent.Manual;
            window.Dispatcher.BeginInvoke(new Action(() => {
                DoubleAnimation heightAnimation = new DoubleAnimation();
                heightAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                heightAnimation.From = window.ActualHeight;
                heightAnimation.To = height;
                heightAnimation.FillBehavior = FillBehavior.HoldEnd;
                window.BeginAnimation(Window.HeightProperty, heightAnimation);
            }), null);
            // window.EndInit();
        }

        private void bt3_Click(object sender, RoutedEventArgs e) {
            while (datagrid1.SelectedItems.Count > 0) {
                datagrid1.Items.RemoveAt(datagrid1.SelectedIndex);
            }
            datagrid1.Items.Refresh();
        }
        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e) {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            e.Row.Height = 30;
        }

        private void rb3_Checked(object sender, RoutedEventArgs e) {
            datagrid1.Items.Clear();
            bt6.IsEnabled = false;
        }

        private void datagrid1_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e) {

        }

        private void bt4_Click(object sender, RoutedEventArgs e) {
            settings set = new settings();
            set.ShowDialog();
            keysize = settings.keysize;
            decomp = settings.decomp;
            blo = 166400 * (int)(Math.Pow(10, settings.block + 1));

        }

        private void datagrid1_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int v = 0, h; h = datagrid1.Items.Count;
                foreach (String file in files) {
                    v++; h++;
                    FileInfo info = new FileInfo(file);
                    datagrid1.Items.Add(new { Col1 = h, Col2 = Path.GetFileName(file), Col3 = file, Col4 = Math.Round((double)(info.Length / 1024.0 / 1024.0), 3) + " MB" });
                }
            }
        }

        private void bt5_Click(object sender, RoutedEventArgs e) {
            go = false;
            if (fs != null) {
                fs.Close();
            }

            if (fss1 != null) {
                fss1.Close();
            }
            if (Process.GetProcessesByName("7za").Length > 0) {
                Process p = Process.GetProcessesByName("7za")[0];
                p.Kill();
            }
        }

        private void bt6_Click(object sender, RoutedEventArgs e) {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            ofd.ShowDialog();
            string fold = ofd.SelectedPath;
            if (fold != "") {
                int v = 0, h; h = datagrid1.Items.Count;
                String file = fold;
                v++; h++;
                FileInfo info = new FileInfo(file);
                datagrid1.Items.Add(new { Col1 = h, Col2 = fold, Col3 = file, Col4 = -1 + " KB" });
            }
        }
        public void comp(String fold) {
            string sourceName = "\"" + fold + "\"";
            string targetName = "\"" + fold + ".zip" + "\"";
            cmmdp = true;
            processStartInfo = new ProcessStartInfo("cmd.exe", @"/c 7za a " + targetName + " " + sourceName + " -mx0 -tzip -mmt on");
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            process = Process.Start(processStartInfo);
            process.WaitForExit();
            cmmdp = false;
        }
        public void decom(String fold) {
            string sourceName = "\"" + fold + "\"";
            string targetName = "\"" + Path.GetDirectoryName(fold) + "\"";
            cmmdp = true;
            processStartInfo = new ProcessStartInfo("cmd.exe", @"/c 7za x " + sourceName + " -o" + targetName);
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            process = Process.Start(processStartInfo);
            process.WaitForExit();
            File.Delete(fold);
            cmmdp = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (Process.GetProcessesByName("7za").Length > 0) {
                Process p = Process.GetProcessesByName("7za")[0];
                p.Kill();
            }
            Environment.Exit(0);
        }
    }
}