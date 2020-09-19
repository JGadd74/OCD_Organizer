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
    /// Plans: Using data binding for AutoCheck
    ///        refactor
    ///        Undo? v .6
    ///        Select custom folder for organize v1.0
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
            this.Dispatcher.Invoke(() => RestoreButton.IsEnabled = true);
        }

        private void Organizer_Organizing_Started(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => RestoreButton.IsEnabled = false);
        }

        public static bool AutoIsEnabled = false;
 
        readonly FileOrganizer Organizer = new FileOrganizer();

        private void Organizer_FileCountUpdated(object source, FileCountUpdatedEventArgs e)
        {
            double p;
            p = e.CompletionPercentage;
            UpdateProgressBar(p);
        }
        public void SayStatus()
        {
            this.Dispatcher.Invoke(() =>
            {
                Head.Text = "Loose Files: " + Organizer.GetFileCount().ToString();
                //Thread.Sleep(0);
            });
        }
        public void UpdateProgressBar(double value)
        {
            this.Dispatcher.Invoke(() =>
            {
                SayStatus();
                //Head.Text = value.ToString(); //testing
                MyProgressBar.Value = value;
            });
        }

        public int AutoRate = 6000;
        private void Button_Click_Organize(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((O) =>
            {
                this.Dispatcher.Invoke(() => AutoIsEnabled = AutoCheck.IsChecked.Value);

                if (!AutoIsEnabled) Organizer.OrganizeDownloads();
                {
                    while(FileOrganizer.ProgressStatus != FileOrganizer.Status.Finished)
                    {
                        Organizer.OrganizeDownloads();
                        Thread.Sleep(AutoRate);//1 min
                        if (!AutoIsEnabled) break;
                    }
                }
            });       
        }

       
        
        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            FileOrganizer.ProgressStatus = FileOrganizer.Status.Finished;
            System.Windows.Application.Current.Shutdown();
        }

       public void SayDone()
        {
            this.Dispatcher.Invoke(() =>
            {
                Head.Text = "Done!";
            });
        }
        private void CreateDummyFiles_Click(object sender, RoutedEventArgs e)
        { // testing purposes
            Organizer.CreateDummieFiles(1000);
        }

        private void UnpackButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((o) => 
            {
                Organizer.Unpack();
            });
            
        }

        

        
    }
}
