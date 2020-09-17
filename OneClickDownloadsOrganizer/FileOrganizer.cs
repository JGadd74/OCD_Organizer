using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Windows;

namespace OneClickDownloadsOrganizer
{
    class FileOrganizer
    {

        private static readonly string activeUser = Environment.UserName;

        private static  readonly string Main = @"C:\Users\" + activeUser + @"\Downloads";
        //private static string Other = System.IO.Path.Combine(Main, "Unknown type");


        private static readonly ExtensionsKit Ekit = new ExtensionsKit();
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


      
        public int GetFileCount()
        {
            int cnt = Directory.EnumerateFiles(Main).Count();
            return cnt - 1;
        }
        public void MonitorFileCount()
        {
            if (GetFileCount() != InitialFileCount) OnFileCountUpdated();
            if (GetFileCount() == 0) 
            {
                OnFileCountUpdated();
            }
        }

        public delegate void FileCountUpdate(object source, FileCountUpdatedEventArgs args);
        public event FileCountUpdate FileCountUpdated;
        protected virtual void OnFileCountUpdated()
        {
            FileCountUpdated?.Invoke(this, new FileCountUpdatedEventArgs(GetFileCount(), InitialFileCount));
        }


        public bool OrganizeDownloads()
        {
            InitialFileCount = Files.Count();
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
            return true;
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
            OldFileCount = oldFileCount;
            CompletionPercentage = (100 - (100 * (newFileCount / oldFileCount)));
        }
        public double CompletionPercentage { get; set; }    
        public int NewFileCount { get; set; }
        public int OldFileCount { get; set; }
    }
}
