using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Crypt {
    public partial class sendTo : Window {
        public static Process x; public static ProcessStartInfo processStartInfo;
        public static Process process;
        public static string temp1, temp2 = "";
        public static int keysize = settings.keysize; public static int block = settings.block; public static bool decomp = settings.decomp;
        public static int blo;
        public static bool go = false, cmmdp = false, v;
        public static long val, numBytesRead, fileOffset;
        public static double fac;
        public static FileStream fs = null, fss1 = null;
        public static Stopwatch st = new Stopwatch();
        public static List<String> path = new List<string>();
        public static List<String> filename = new List<string>();
        public static List<int> size = new List<int>();

        public sendTo() {
            InitializeComponent();
            keysize = 128;
            blo = 8320000;
            block = 1;
            decomp = false;
            path = App.path;
            foreach (String filepath in path) {
                FileInfo finfo = new FileInfo(filepath);
                filename.Add(finfo.FullName);

                FileAttributes att = File.GetAttributes(filepath);
                if ((att & FileAttributes.Directory) == FileAttributes.Directory)
                    size.Add(-1);
                else
                    size.Add((int)finfo.Length / 1024 / 1024);
            }
            if (pass.packageFiles) {
                Directory.CreateDirectory(Path.GetDirectoryName(path[0]) + "\\" + pass.folderName);
                String temp = "";
                foreach (String filepath in path) {
                    FileInfo finfo = new FileInfo(filepath);
                    MessageBox.Show(filepath + "   " + Path.GetDirectoryName(filepath) + "\\" + pass.folderName + "\\" + Path.GetFileName(filepath));
                    FileAttributes att = File.GetAttributes(filepath);
                    if ((att & FileAttributes.Directory) == FileAttributes.Directory)
                        CopyAll(new DirectoryInfo(filepath), new DirectoryInfo(Path.GetDirectoryName(filepath) + "\\" + pass.folderName + "\\" + Path.GetFileName(filepath)));
                    else
                        File.Copy(filepath, Path.GetDirectoryName(filepath) + "\\" + pass.folderName + "\\" + Path.GetFileName(filepath), true);

                    temp = Path.GetDirectoryName(filepath) + "\\" + pass.folderName;
                }
                path.Clear(); filename.Clear(); size.Clear();
                path.Add(temp); filename.Add(pass.folderName); size.Add(-1);
            }
            if (pass.passw == true) {
                go = true;
                if (App.whattodo)
                    enc();
                else
                    dec();
            }
            else
                this.Close();

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
            lb1.Content = "";
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
            this.Close();
        }

        //main background thread
        private void bw_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int y = 0; y < path.Count; y++) {
                bool delete = false;
                worker.ReportProgress(0);

                //zip if selection is a folder
                if (size[y] == -1) {
                    Dispatcher.Invoke(() => lb1.Content = "Packing the Folder into an archive --> " + Path.GetFileName(path[y]), DispatcherPriority.Send);
                    comp(path[y]);
                    path[y] = path[y] + ".zip";
                    delete = true;
                }
                temp1 = Path.Combine(Path.GetDirectoryName(path[y]), Path.GetFileNameWithoutExtension(path[y]));
                Dispatcher.Invoke(() => lb1.Content = "Encrypting --> " + Path.GetFileName(path[y]), DispatcherPriority.Send);

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
            pb1.Value = 0;
            lb1.Content = "";
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
            this.Close();
        }

        private void debw_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int y = 0; y < path.Count; y++) {
                try {
                    worker.ReportProgress(0);

                    temp1 = Path.Combine(Path.GetDirectoryName(path[y]), Path.GetFileNameWithoutExtension(path[y]));
                    Dispatcher.Invoke(() => lb1.Content = "Decrypting --> " + Path.GetFileName(path[y]), DispatcherPriority.Send);

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


        private void letsdothis() {
            pass ps = new pass();
            ps.ShowDialog();
            if (pass.passw == true) {
                go = true;
                if (App.whattodo)
                    enc();
                else
                    dec();
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


        public void comp(String fold) {
            string sourceName = "\"" + fold + "\"";
            string targetName = "\"" + fold + ".zip" + "\"";
            cmmdp = true;
            processStartInfo = new ProcessStartInfo("cmd.exe", @"/c 7za a " + targetName + " " + sourceName + " -mx0 -tzip -mmt");
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
        public static void CopyAll(DirectoryInfo sdir, DirectoryInfo ddir) {
            if (Directory.Exists(ddir.FullName) == false) {
                Directory.CreateDirectory(ddir.FullName);
            }

            foreach (FileInfo f in sdir.GetFiles()) {
                f.CopyTo(Path.Combine(ddir.FullName, f.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in sdir.GetDirectories()) {
                DirectoryInfo tdir = ddir.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, tdir);
            }
        }
    }
}