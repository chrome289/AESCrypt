using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using IWshRuntimeLibrary;

namespace Crypt {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static List<string> path = new List<string>();
        public static bool whattodo,calledFromMainWin=false;
        protected override void OnStartup(StartupEventArgs e) {
            if (Minestrone.Properties.Settings.Default.firstStart) {
                CreateShortCuts();
                Minestrone.Properties.Settings.Default.firstStart = false;
            }
            if (e.Args.Length != 0) {
                if (e.Args[0] == "-en")
                    whattodo = true;
                else
                    whattodo = false;
                for (int x = 1; x < e.Args.Length; x++)
                    path.Add(e.Args[x]);
                calledFromMainWin = false;
                pass ps = new pass();
                ps.Show();

            }
            else {
                calledFromMainWin = true;
                MainWindow mw = new MainWindow();
                mw.Show();
            }
        }
        private void CreateShortCuts() {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) + "\\Minestrone.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            MessageBox.Show(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "Minestrone.exe");
            shortcut.TargetPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\Minestrone.exe";
            shortcut.Arguments = "-de";
            shortcut.Save();
            System.IO.File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Minestrone.lnk", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\sendTo\\Decrypt with Minestrone.lnk",false);

            shDesktop = "Desktop";
            shell = new WshShell();
            shortcutAddress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) + "\\Minestrone.lnk";
            shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\Minestrone.exe";
            shortcut.Arguments = "-en";
            shortcut.Save();
            System.IO.File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Minestrone.lnk", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\sendTo\\Encrypt with Minestrone.lnk",false);
        }
    }
}
