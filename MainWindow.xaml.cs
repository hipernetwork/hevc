using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace hevc_encoder
{
    public partial class MainWindow : Window
    {
        private readonly int m_TotalCPU = Environment.ProcessorCount;
        private readonly string m_FFMpegPath;

        public MainWindow()
        {
            InitializeComponent();
            m_FFMpegPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "ffmpeg.exe");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(m_FFMpegPath))
            {
                MessageBox.Show("FFmpeg not found!\r\nPath: " + m_FFMpegPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            TotalCPUCountText.Text = "/ " + m_TotalCPU;
            for (int i = 1; i <= m_TotalCPU; i++)
            {
                NumberOfCPUComboBox.Items.Add(new ComboBoxItem() { Content = i.ToString() });
            }
        }

        private void EnableUI()
        {
            EncodeButton.IsEnabled = true;
            SourceFileBrowseButton.IsEnabled = true;
            OutputFileBrowseButton.IsEnabled = true;
            NumberOfCPUComboBox.IsEnabled = true;
        }

        private void DisableUI()
        {
            EncodeButton.IsEnabled = false;
            SourceFileBrowseButton.IsEnabled = false;
            OutputFileBrowseButton.IsEnabled = false;
            NumberOfCPUComboBox.IsEnabled = false;
        }

        private void SourceFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select the video to encode with HEVC",
                Filter = "Supported Video Files(*.mp4;*.mkv;*.avi;*.mov;*.webm;*.flv;*.mpg;*.mpeg;*.ts;*.m2ts;*.wmv;*.ogv)|*.mp4;*.mkv;*.avi;*.mov;*.webm;*.flv;*.mpg;*.mpeg;*.ts;*.m2ts;*.wmv;*.ogv",
                Multiselect = false
            };
            if (ofd.ShowDialog() == true)
            {
                SourceFileTextBox.Text = ofd.FileName;
            }
        }

        private void OutputFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "HEVC Encoded Video File(*.mp4)|*.mp4"
            };
            if (sfd.ShowDialog() == true)
            {
                OutputFileTextBox.Text = sfd.FileName;
            }
        }

        private async void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SourceFileTextBox.Text))
            {
                if (File.Exists(SourceFileTextBox.Text))
                {
                    DisableUI();
                    StringBuilder argBuild = new StringBuilder();
                    argBuild.Append("-y");
                    if (NumberOfCPUComboBox.SelectedIndex != 0)
                    {
                        argBuild.Append(" -threads " + NumberOfCPUComboBox.Items[NumberOfCPUComboBox.SelectedIndex].ToString());
                    }
                    argBuild.Append(" -i \"" + SourceFileTextBox.Text + "\" -c:v libx265 -crf 28 -preset medium -c:a aac -b:a 128k \"" + OutputFileTextBox.Text + "\"");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    await Task.Run(() =>
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = m_FFMpegPath,
                            Arguments = argBuild.ToString(),
                            UseShellExecute = true,
                            CreateNoWindow = false
                        };
                        Process proc = Process.Start(psi);
                        proc.WaitForExit();
                    });
                    sw.Stop();
                    EnableUI();
                    TimeSpan elapsedTime = sw.Elapsed;
                    string totalTime = string.Format("{0:00}:{1:00}:{2:00}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                    MessageBox.Show("Your video is ready!\r\nTotal Time: " + totalTime, "HEVC Encoder", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Source file not found!\r\nFile: " + SourceFileTextBox.Text, "HEVC Encoder", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select the video file to encode with HEVC!", "HEVC Encoder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}