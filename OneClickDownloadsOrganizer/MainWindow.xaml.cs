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
            Organizer.OrganizeStarted += Organizer_OrganizeStarted;
            Max = Organizer.GetFileCount();
            
            MyProgressBar.Value = 0;
        }

        private void Organizer_OrganizeStarted(object source, EventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (Organizer.GetFileCount() == 0) MyProgressBar.Maximum = 1;
                else 
                {
                    Max = Organizer.GetFileCount();
                    MyProgressBar.Maximum = Organizer.GetFileCount();
                }
            });
        }
        int Max = 0;
        readonly FileOrganizer Organizer = new FileOrganizer();

        private void Organizer_FileCountUpdated(object source, EventArgs args)
        {
            UpdateProgressBar();
        }
        public void SayStatus()
        {
            this.Dispatcher.Invoke(() =>
            {
                Head.Text = "Files Remaining: " + Organizer.GetFileCount().ToString();
                //Thread.Sleep(0);
            });
        }
        public void UpdateProgressBar()
        {
            int tmp = Max - Organizer.GetFileCount();
            this.Dispatcher.Invoke(() =>
            {
                if (Organizer.GetFileCount() == 0) SayDone();
                else SayStatus();
                
                MyProgressBar.Value = tmp;
            });
        }
        public static bool AutoIsEnabled = false;

        private void Button_Click_Organize(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((O) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    AutoIsEnabled = AutoCheck.IsChecked.Value;
                });

                if (!AutoIsEnabled) Organizer.OrganizeDownloads();
                {
                    while(FileOrganizer.ProgressStatus != FileOrganizer.Status.Finished)
                    {
                        Organizer.OrganizeDownloads();
                        Thread.Sleep(60000);//1 min
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
        {
            Organizer.CreateDummieFiles(1000);
        }

        


    }
}
