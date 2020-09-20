﻿using System;
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
    ///        Undo? v .6 - revised: Unpack - Check
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
        }
        readonly FileOrganizer Organizer = new FileOrganizer();
        private static bool AutoIsEnabled = true;

        private void Organizer_Unpack_Finished(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => Button_Organize.IsEnabled = true);
        }
        private void Organizer_Unpack_Started(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => Button_Organize.IsEnabled = false);
        }
        private void Organizer_Organizing_Finished(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                //if(!AutoIsEnabled)
                    UnpackButton.IsEnabled = true;
            });
        }
        private void Organizer_Organizing_Started(object sender, EventArgs e)
        {
            if(Organizer.GetFileCount() > 0)
            this.Dispatcher.Invoke(() => 
            {
                UnpackButton.IsEnabled = false;
                //AutoIsEnabled = false;
            });
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
        private void Button_Click_Organize(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((O) => Organizer.OrganizeDownloads());
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
                autoData.Visibility = Visibility.Hidden;
                AutoIsEnabled = false;
                UnpackButton.IsEnabled = true;
                Button_Organize.IsEnabled = true;
            });
        }
        private void AutoCheck_Checked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                autoData.Visibility = Visibility.Visible;
                AutoIsEnabled = true;
                UnpackButton.IsEnabled = false;
                Button_Organize.IsEnabled = false;
            });

            InitializeAutoMode();
        }
        private void SayStatus()
        {
            this.Dispatcher.Invoke(() => StatusBlock.Text = "Loose Files: " + Organizer.GetFileCount().ToString());
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
                        Organizer.OrganizeDownloads();
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

        private void Button_Organize_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
