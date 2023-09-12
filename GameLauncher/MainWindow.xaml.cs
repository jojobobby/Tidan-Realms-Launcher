using System;
using System.IO;
using System.Windows;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using System.Diagnostics;

namespace GameLauncher
{
    enum LauncherState
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
        extractingGame
    }

    public partial class MainWindow : Window
    {
        private readonly string VERSION_FILE_LINK = "https://jojobobby.github.io/client/Version.txt";
        private readonly string GAME_DOWNLOAD_LINK = "https://raw.githubusercontent.com/jojobobby/jojobobby.github.io/master/client/AboveTheSurface.zip";
        private readonly string GAME_EXE_FILE_NAME = "AboveTheSurface/Webmain.exe";
        private readonly string GAME_ZIP_FILE_NAME = "AboveTheSurface.zip";
        private readonly string GAME_FILE_NAME = "AboveTheSurface";

        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string gameFiles;

        private LauncherState _status;
        internal LauncherState Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherState.ready:
                        PlayButton.Content = "Play";
                        break;
                    case LauncherState.failed:
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherState.downloadingGame:
                        PlayButton.Content = "Downloading Game";
                        break;
                    case LauncherState.downloadingUpdate:
                        PlayButton.Content = "Updating Game";
                        break;
                    case LauncherState.extractingGame:
                        PlayButton.Content = "Extracting Game Data";
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, GAME_ZIP_FILE_NAME);
            gameExe = Path.Combine(rootPath, GAME_EXE_FILE_NAME);
            gameFiles = Path.Combine(rootPath, GAME_FILE_NAME);
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString(VERSION_FILE_LINK));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherState.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherState.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherState.downloadingUpdate;
                }
                else
                {
                    Status = LauncherState.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString(VERSION_FILE_LINK));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri(GAME_DOWNLOAD_LINK), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherState.failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                var oldFiles = new DirectoryInfo(gameFiles);
                if (oldFiles.Exists)
                {
                    foreach (FileInfo file in oldFiles.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in oldFiles.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }

                Status = LauncherState.extractingGame;
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath);

                File.WriteAllText(versionFile, onlineVersion);
                VersionText.Text = onlineVersion;
                Status = LauncherState.ready;

                File.Delete(gameZip);
            }
            catch (Exception ex)
            {
                Status = LauncherState.failed;
                MessageBox.Show($"Error finishing game download: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherState.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Build");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherState.failed)
            {
                CheckForUpdates();
            }

        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');

            if (_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            short[] version_1 = { major, minor, subMinor };
            short[] version_2 = { _otherVersion.major, _otherVersion.minor, _otherVersion.subMinor };

            if (version_1.SequenceEqual(version_2))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
