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
        public MainWindow()
        {
            InitializeComponent();
            Loaded += onLoad;
        }

        private async void onLoad(object sender, RoutedEventArgs e)
        {
            StatusLbl.Content = "Loading...";
            if (!File.Exists("Update.txt"))
            {
                MessageBox.Show("Update failed!");
                this.Hide();
                return;
            }
            try
            {
                ProgressBar.IsIndeterminate = true;
                string app = Directory.GetParent(startupPath).ToString();
                StatusLbl.Content = "Reading data...";
                string link = null;
                List<string> ignore = new List<string>();
                using (StreamReader sr = new StreamReader("Update.txt"))
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
                await Task.Delay(3000);
                StatusLbl.Content = "Deleting files...";
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
                        File.Delete(file);
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
                        Directory.Delete(dir, true);
                    }
                }
                ProgressBar.IsIndeterminate = false;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileAsync(new Uri(link), "temp.zip");
                }
                while (ProgressBar.Value < 100)
                {
                    await Task.Delay(100);
                }
                ProgressBar.IsIndeterminate = true;
                StatusLbl.Content = "Extracting files...";
                ZipFile.ExtractToDirectory("temp.zip", app);
                StatusLbl.Content = "Deleting temporary file...";
                File.Delete("temp.zip");
                this.Hide();
                if (File.Exists("Log.txt"))
                {
                    File.Delete("Log.txt");
                }
                File.Create("Log.txt").Close();
                using (StreamWriter sw = new StreamWriter("Log.txt"))
                {
                    sw.WriteLine("Successful");
                }
                if (File.Exists("Startup.txt"))
                {
                    using (StreamReader sr = new StreamReader("Startup.txt"))
                    {
                        string start = sr.ReadLine();
                        if (File.Exists(start))
                        {
                            Process.Start(start);
                        }
                    }
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed!" + Environment.NewLine + ex.Message);
                this.Hide();
                if (File.Exists("Log.txt"))
                {
                    File.Delete("Log.txt");
                }
                File.Create("Log.txt").Close();
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
