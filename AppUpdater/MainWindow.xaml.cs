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
            if (!File.Exists("Update.txt"))
            {
                MessageBox.Show("Update failed!");
                this.Hide();
                return;
            }
            try
            {
                StatusLbl.Content = "Deleting files...";
                ProgressBar.IsIndeterminate = true;
                string app = Directory.GetParent(startupPath).ToString();
                foreach (string file in Directory.GetFiles(app))
                {
                    File.Delete(file);
                }
                foreach (string dir in Directory.GetDirectories(app))
                {
                    if (dir != startupPath)
                    {
                        Directory.Delete(dir, true);
                    }
                }
                string link = null;
                using (StreamReader sr = new StreamReader("Update.txt"))
                {
                    link = sr.ReadLine();
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
                File.Create("Log.txt");
                using (StreamWriter sw = new StreamWriter("Log.txt"))
                {
                    sw.WriteLine("Successful");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed!" + Environment.NewLine + ex.Message);
                this.Hide();
                if (File.Exists("Log.txt"))
                {
                    File.Delete("Log.txt");
                }
                File.Create("Log.txt");
                using (StreamWriter sw = new StreamWriter("Log.txt"))
                {
                    sw.WriteLine("Failed");
                }
            }
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            StatusLbl.Content = "Downloading file " + e.ProgressPercentage + "% done";
        }
    }
}
