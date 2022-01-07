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
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;

namespace AppUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string startupPath = Environment.CurrentDirectory;
        public static int rercursiveCheck = 0;
        public MainWindow()
        {
            InitializeComponent();
            if (!startupPath.Contains("Updater"))
            {
                startupPath = startupPath + "\\Updater";
            }
            Loaded += onLoad;
        }

        private async void onLoad(object sender, RoutedEventArgs e)
        {
            string cTask = "waiting";
            StatusLbl.Content = "Loading...";
            ProgressBar.IsIndeterminate = true;
            await Task.Delay(3000);
            try
            {
                string app = Directory.GetParent(startupPath).ToString();
                cTask = "reading data";
                StatusLbl.Content = "Reading data...";
                string link = null;
                List<string> ignore = new List<string>();
                using (StreamReader sr = new StreamReader(startupPath + "\\Update.txt"))
                {
                    string line = sr.ReadLine();
                    link = line;
                    line = sr.ReadLine();
                    while (line != null)
                    {
                        ignore.Add(line);
                        line = sr.ReadLine();
                    }
                }
                cTask = "deleting/creating backup folder";
                if (Directory.Exists(startupPath + "\\Backup")) { Directory.Delete(startupPath + "\\Backup", true); }
                Directory.CreateDirectory(startupPath + "\\Backup");
                cTask = "moving files to backup folder";
                StatusLbl.Content = "Creating backup of app...";
                foreach (string file in Directory.GetFiles(app))
                {
                    bool delete = true;
                    foreach (string check in ignore)
                    {
                        if (file == app + "\\" + check)
                        {
                            delete = false;
                        }
                    }
                    if (delete)
                    {
                        File.Move(file, startupPath + "\\Backup\\" + file.Split('\\')[file.Split('\\').Length - 1]);
                    }
                }
                foreach (string dir in Directory.GetDirectories(app))
                {
                    bool delete = true;
                    foreach (string check in ignore)
                    {
                        if (dir == app + "\\" + check)
                        {
                            delete = false;
                        }
                    }
                    if (dir != startupPath && delete)
                    {
                        Directory.Move(dir, startupPath + "\\Backup\\" + dir.Split('\\')[dir.Split('\\').Length - 1]);
                    }
                }
                ProgressBar.IsIndeterminate = false;
                cTask = "downloading zip file";
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileAsync(new Uri(link), startupPath + "\\temp.zip");
                }
                while (ProgressBar.Value < 100)
                {
                    await Task.Delay(100);
                }
                ProgressBar.IsIndeterminate = true;
                cTask = "extracting zip file";
                StatusLbl.Content = "Extracting files...";
                ZipFile.ExtractToDirectory(startupPath + "\\temp.zip", app);
                cTask = "deleting zip file";
                StatusLbl.Content = "Deleting temporary file...";
                File.Delete(startupPath + "\\temp.zip");
                cTask = "deleting backup file";
                StatusLbl.Content = "Deleting backup files...";
                Directory.Delete(startupPath + "\\Backup", true);
                cTask = "creating log file";
                this.Hide();
                if (File.Exists(startupPath + "\\Log.txt"))
                {
                    File.Delete(startupPath + "\\Log.txt");
                }
                File.Create(startupPath + "\\Log.txt").Close();
                using (StreamWriter sw = new StreamWriter(startupPath + "\\Log.txt"))
                {
                    sw.WriteLine("Successful");
                }
                if (File.Exists(startupPath + "\\Startup.txt"))
                {
                    using (StreamReader sr = new StreamReader(startupPath + "\\Startup.txt"))
                    {
                        string start = sr.ReadLine();
                        if (File.Exists(app + "\\" + start))
                        {
                            Process.Start(app + "\\" + start);
                        }
                    }
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed! Current Task: " + cTask + Environment.NewLine + ex.Message);
                this.Hide();
                if (File.Exists(startupPath + "\\Log.txt"))
                {
                    File.Delete(startupPath + "\\Log.txt");
                }
                File.Create(startupPath + "\\Log.txt").Close();
                using (StreamWriter sw = new StreamWriter("Log.txt"))
                {
                    sw.WriteLine("Failed");
                }
                
                this.Close();
            }
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            StatusLbl.Content = "Downloading file " + e.ProgressPercentage + "% done";
        }
    }
}