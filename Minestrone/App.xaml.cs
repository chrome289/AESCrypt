using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Crypt {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static List<string> path = new List<string>();
        public static bool whattodo;
        protected override void OnStartup(StartupEventArgs e) {
            //e.Args is the string[] of command line argruments
            if (e.Args.Length != 0) {
                if (e.Args[0] == "-en")
                    whattodo = true;
                else
                    whattodo = false;
                for (int x = 1; x < e.Args.Length; x++)
                    path.Add(e.Args[x]);
                Crypt.sendTo st = new Crypt.sendTo();
                st.Show();
            }
            else {
                MainWindow mw = new MainWindow();
                mw.Show();
            }
               
        }
   
    }
}
