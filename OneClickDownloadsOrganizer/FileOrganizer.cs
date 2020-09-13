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

        public delegate void FileCountUpdate(object source, EventArgs args);

        public event FileCountUpdate FileCountUpdated;

        protected virtual void OnFileCountUpdated()
        {
            FileCountUpdated?.Invoke(this, EventArgs.Empty);
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

        public delegate void OrganizeStart(object source, EventArgs args);
        public event OrganizeStart OrganizeStarted;
        protected virtual void OnOrganizeStarted()
        {
            OrganizeStarted?.Invoke(this, EventArgs.Empty);
        }

        public bool OrganizeDownloads()
        {
            OnOrganizeStarted();
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
}
