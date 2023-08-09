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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameLauncher
{
    enum LauncherState
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
