using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;

namespace OneClickDownloadsOrganizer
{
    class FileOrganizer
    {
        private static readonly string activeUser = Environment.UserName;

        private static  readonly string Main = @"C:\Users\" + activeUser + @"\Downloads";
        //private static string Other = System.IO.Path.Combine(Main, "Unknown type");

        ExtensionsKit Ekit = new ExtensionsKit();
        private static readonly IEnumerable<string> Files = Directory.EnumerateFiles(Main);
        private const int CategoryName = 0;
        public static int InitialFileCount;
        
        public static Status ProgressStatus = Status.Ready;

        public void CreateDummieFiles(int count)
        {
            for(int i = 0; i<=count; i++)
            {
                string name = i.ToString() + ".txt";
                File.Create(Path.Combine(Main, name));
            }
            InitialFileCount = Files.Count() - 1;
        }
        public FileOrganizer()
        {
            InitialFileCount = Directory.EnumerateFiles(Main).Count()-1; 
            //MessageBox.Show(InitialFileCount.ToString());
        }

      
        public int GetFileCount()
        {
            int cnt = Directory.EnumerateFiles(Main).Count();
            return cnt-1;
        }
        public void MonitorFileCount()
        {
            OnFileCountUpdated();
            if (GetFileCount() == 0) 
            {
                ProgressStatus = Status.Finished;
                OnOrganizingFinished();
            }
        }

        public delegate void FileCountUpdate(object source, FileCountUpdatedEventArgs args);
        public event FileCountUpdate FileCountUpdated;
        protected virtual void OnFileCountUpdated()
        {
            FileCountUpdated?.Invoke(this, new FileCountUpdatedEventArgs(GetFileCount(), InitialFileCount));
        }


        public EventHandler<EventArgs> OrganizingStarted;
        protected virtual void OnOrganizingStarted()
        {
            OrganizingStarted?.Invoke(this, EventArgs.Empty);
        }



        public EventHandler<EventArgs> OrganizeFinished;
        protected virtual void OnOrganizingFinished()
        {
            OrganizeFinished?.Invoke(this, EventArgs.Empty);
        }


        public EventHandler<EventArgs> UnpackStarted;
        public EventHandler<EventArgs> UnpackFinished;
        protected virtual void OnUnpackStarted()
        {
            UnpackStarted?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnUnpackFinished()
        {
            UnpackFinished?.Invoke(this, EventArgs.Empty);
        }




        public void Unpack()
        {
            
            if (ProgressStatus != Status.Processing)
            {
                OnUnpackStarted();

                
                int totalCount = 0;

                var fileCounts = from d in Directory.EnumerateDirectories(Main)
                                 select Directory.EnumerateFiles(d).Count();
                foreach (int fileCount in fileCounts)
                {
                    totalCount += fileCount;
                }
                InitialFileCount = totalCount;



                var directories = Directory.EnumerateDirectories(Main);
                foreach( var directory in directories)
                {
                    var _files = Directory.EnumerateFiles(directory);
                    foreach(var _file in _files)
                    {
                        string name = Path.GetFileName(_file);
                        File.Move(_file, Path.Combine(Main, name));
                        MonitorFileCount();
                    }
                    Directory.Delete(directory);
                }
              

            }
            InitialFileCount = GetFileCount();
            OnUnpackFinished();

        }


        public void OrganizeDownloads()
        {

            OnOrganizingStarted();
            InitialFileCount = GetFileCount();
            ProgressStatus = Status.Processing;
          
            foreach (string file in Files)
            {
                foreach (string[] category in Ekit.ExtensionCategories) 
                {
                    foreach (string extension in category)
                    {
                        if (System.IO.Path.GetExtension(file) == extension)
                        {
                            if (!Directory.Exists(System.IO.Path.Combine(Main, category[CategoryName])))
                            {
                                Directory.CreateDirectory(System.IO.Path.Combine(Main, category[CategoryName]));
                            }
                            string fileName = Path.GetFileName(file);
                            File.Move(file, Path.Combine(Main, category[CategoryName], fileName));
                            MonitorFileCount();
                        }
                    }
                }
            }
            MonitorFileCount();
        }

        public enum Status
        {
            Ready,
            Processing,
            Finished
        }
    }

    public class FileCountUpdatedEventArgs
    {
        public FileCountUpdatedEventArgs(int newFileCount, int oldFileCount)
        {
            NewFileCount = newFileCount;
            OldFileCount = oldFileCount > 0 ? oldFileCount : oldFileCount + 1;

            CompletionPercentage = 100 - (100 * (NewFileCount/OldFileCount));
           
           // MessageBox.Show(CompletionPercentage.ToString() + " " + NewFileCount.ToString() + " " + OldFileCount.ToString());
        }
        
        public double CompletionPercentage { get; set; }    
        public double NewFileCount { get; set; }
        public double OldFileCount { get; set; }
    }
}
