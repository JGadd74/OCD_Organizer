using System;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Runtime.CompilerServices;



namespace OneClickDownloadsOrganizer
{
    /// <summary>
    /// Plans: setup incremental time checks for auto mode
    ///        refactor
    ///        Undo? v .6 - Check
    ///        Select from set of folder options to organize v1.0
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Organizer.FileCountUpdated += Organizer_FileCountUpdated;
            Organizer.OrganizingStarted += Organizer_Organizing_Started;
            Organizer.OrganizeFinished += Organizer_Organizing_Finished;
            Organizer.UnpackStarted += Organizer_Unpack_Started;
            Organizer.UnpackFinished += Organizer_Unpack_Finished;
            MyProgressBar.Maximum = 100;
            Header.Text = "One Click\nDownloads \nOrganizer";
        }
        FileOrganizer Organizer = new FileOrganizer();
        private static bool AutoIsEnabled = false;

        private void Organizer_Unpack_Finished(object sender, EventArgs e)
        {
            EnableButtons();
        }
        private void Organizer_Unpack_Started(object sender, EventArgs e)
        {
            DisableButtons();
        }
        private void EnableButtons()
        {
            this.Dispatcher.Invoke(() =>
            {
                Button_Organize.IsEnabled = true;
                UnpackButton.IsEnabled = true;
                AutoCheck.IsEnabled = true;
            });
        }
        private void DisableButtons()
        {
            this.Dispatcher.Invoke(() =>
            {
                Button_Organize.IsEnabled = false;
                UnpackButton.IsEnabled = false;
                AutoCheck.IsEnabled = false;
            });
        }
        private void Organizer_Organizing_Finished(object sender, EventArgs e)
        {
                this.Dispatcher.Invoke(() =>
                {
                    if (!AutoIsEnabled)
                    {
                        Button_Organize.IsEnabled = true;
                        UnpackButton.IsEnabled = true;
                    }
                    AutoCheck.IsEnabled = true;
                });
        }
        private void Organizer_Organizing_Started(object sender, EventArgs e)
        {
            if (Organizer.GetFileCount() > 0)
                DisableButtons();
        }
        private void Organizer_FileCountUpdated(object source, FileCountUpdatedEventArgs e)
        {
            double p;
            p = e.CompletionPercentage;
            UpdateProgressBar(p);
        }
        private void CreateDummyFiles_Click(object sender, RoutedEventArgs e)
        { // testing purposes
            Organizer.CreateDummieFiles(1000);
        }
        private void UnpackButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((o) => Organizer.Unpack());
        }
        private async void Button_Click_Organize(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Organizer.OrganizeActivePath());
            //ThreadPool.QueueUserWorkItem((o) => Organizer.OrganizeDownloads());
        }
        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            FileOrganizer.ProgressStatus = FileOrganizer.Status.Finished;
            System.Windows.Application.Current.Shutdown();
        }
        private void AutoCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                AutoIsEnabled = false;
                autoData.Text = " ";
                UnpackButton.IsEnabled = true;
                Button_Organize.IsEnabled = true;
                DefaultLocationRadioButton.IsEnabled = true;
                CustomLocationRadioButton.IsEnabled = true;
            });
        }
        private void AutoCheck_Checked(object sender, RoutedEventArgs e)
        {
            //   BUG if a valid custom location isn't set then autoCheck gets disabled upon checking
            this.Dispatcher.Invoke(() =>
            {
                AutoIsEnabled = true;
                UnpackButton.IsEnabled = false;
                Button_Organize.IsEnabled = false;
                DefaultLocationRadioButton.IsEnabled = false;
                CustomLocationRadioButton.IsEnabled = false;

            });

            InitializeAutoMode();
        }

        private void SayStatus()
        {
            this.Dispatcher.Invoke(() => StatusBlock.Text = "Loose Files: " + Organizer.GetFileCount().ToString());
        }
        private void SayDone()
        {
            this.Dispatcher.Invoke(() => StatusBlock.Text = "Done!");
        }

        private void UpdateProgressBar(double value)
        {
            this.Dispatcher.Invoke(() =>
            {
                SayStatus();
                //StatusBlock.Text = value.ToString(); //testing
                MyProgressBar.Value = value;
            });
        }
        private void InitializeAutoMode()
        {
            int AutoRate = 1; // Milliseconds
            int tries = 0;
            int unitDivisor = 1;
            string unit = "Msec";
            const int oneSec = 1000;
            const int tenSec = oneSec * 10;
            const int oneMin = tenSec * 6;
            const int thirtyMin = oneMin * 30;
            const int OneHour = thirtyMin * 2;
            const int ThreeHour = OneHour * 3;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (AutoIsEnabled)
                {
                    if (Organizer.ThereAreFiles())
                    {
                        AutoRate = 1;
                        tries = 1;
                        Organizer.OrganizeActivePath();
                    }
                    else if (!Organizer.ThereAreFiles())
                    {
                        if (AutoRate < ThreeHour)
                        {
                            switch (tries)
                            {
                                case tenSec: //after 10 sec
                                    AutoRate = oneSec; //1sec
                                    unit = "Sec";
                                    unitDivisor = oneSec;
                                    break;
                                case 10060: //after 1 min
                                    AutoRate = oneMin; // 1 min
                                    unit = "Min";
                                    unitDivisor = oneMin;
                                    break;
                                case 10090:  // after 30 min
                                    AutoRate = thirtyMin;
                                    break;
                                case 10096: //after 3 hours
                                    unit = "Hr";
                                    unitDivisor = OneHour;
                                    AutoRate = ThreeHour;
                                    break;
                            }
                            tries++;
                        }
                    }
                    this.Dispatcher.Invoke(() => autoData.Text =
                    "Check interval(" + unit + "): " + ((double)AutoRate / (double)unitDivisor).ToString()
                     + '\n' + "Checks: " + tries.ToString());

                    Thread.Sleep(AutoRate);
                }
            });
        }

        private void CustomLocationBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                CustomLocationRadioButton.IsChecked = true;
                DefaultLocationRadioButton.IsChecked = false;
                Search.IsEnabled = true;
                CustomLocationBox.Text = CustomLocationBox.Text.Equals("Enter Custom Path") ? "" : CustomLocationBox.Text;
            });
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => //works
            {
                string dir = CustomLocationBox.Text;
                if (Organizer.ValidateDirectory(dir))
                {
                    autoData.Text = "Directory Found.";
                    CustomLocationBox.BorderBrush = Brushes.Green;
                    MyProgressBar.Value = 0;
                    Organizer.UseCustomLocation();
                    EnableButtons();
                    
                }
                else
                {
                    autoData.Text = "Directory not found.";
                    CustomLocationBox.BorderBrush = Brushes.Red;
                    MyProgressBar.Value = 0;
                    Organizer.UseDefaultLocation();
                    DisableButtons();
                }
            });
        }

        private void DefaultLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => 
            {
                if(Search != null) Search.IsEnabled = false;
                Organizer.UseDefaultLocation();
            });
        }

        private void DefaultLocationRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            string dir = CustomLocationBox.Text;
            if (!Organizer.ValidateDirectory(dir))
            {
                DisableButtons();
            }
        }

        private void CustomLocationRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableButtons();
        }

        private void CustomLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Search.IsEnabled = true;
                CustomLocationBox.Focus();
                string dir = CustomLocationBox.Text;
                if (Organizer.ValidateDirectory(dir))
                {
                    Organizer.UseCustomLocation();
                }
            });
            
        }
    }
}
