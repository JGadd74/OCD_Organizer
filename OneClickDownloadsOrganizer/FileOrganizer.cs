using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using System.Security.Policy;
using System.Xaml.Schema;

namespace OneClickDownloadsOrganizer
{
    class FileOrganizer
    {
        // start refactoring to allow for different Main locations
        public FileOrganizer() => InitialFileCount = Directory.EnumerateFiles(Main).Count() - 1;

        private static string Main = @"C:\Users\" + Environment.UserName + @"\Downloads";

        readonly ExtensionsKit Ekit = new ExtensionsKit();
        private static readonly IEnumerable<string> Files = Directory.EnumerateFiles(Main);
        private const int CategoryName = 0;
        public static int InitialFileCount;
        
        public static Status ProgressStatus = Status.Ready;
        public int GetFileCount() => Directory.EnumerateFiles(Main).Count() - 1;

        public EventHandler<FileCountUpdatedEventArgs> FileCountUpdated;
        public EventHandler<EventArgs> OrganizingStarted;
        public EventHandler<EventArgs> OrganizeFinished;
        public EventHandler<EventArgs> UnpackStarted;
        public EventHandler<EventArgs> UnpackFinished;
        protected virtual void OnFileCountUpdated() =>
            FileCountUpdated?.Invoke(this, new FileCountUpdatedEventArgs(GetFileCount(), InitialFileCount));
        protected virtual void OnOrganizingStarted() =>
            OrganizingStarted?.Invoke(this, EventArgs.Empty);
        protected virtual void OnOrganizingFinished() =>
            OrganizeFinished?.Invoke(this, EventArgs.Empty);
        protected virtual void OnUnpackStarted() =>
            UnpackStarted?.Invoke(this, EventArgs.Empty);
        protected virtual void OnUnpackingFinished() =>
            UnpackFinished?.Invoke(this, EventArgs.Empty);

        public void MonitorFileCount()
        {
            OnFileCountUpdated();
            if (GetFileCount() == 0 && ProgressStatus == Status.Organizing) 
            {
                ProgressStatus = Status.Finished;
                OnOrganizingFinished();
            }
            else if (GetFileCountFromSubDirectories(Main) == 0 && ProgressStatus == Status.Unpacking)
            {
                ProgressStatus = Status.Finished;
                OnUnpackingFinished();
            }
        }

        public bool ThereAreFiles() => GetFileCount() > 0;


        public int GetFileCountFromSubDirectories(string Dir)
        {
            int totalCount = 0;
            List<string> LocallyCreatedDirectories = GetLocallyCreatedDirs();

           
            var fileCounts = from d in LocallyCreatedDirectories
                             select Directory.EnumerateFiles(d).Count();

            foreach (int fileCount in fileCounts) totalCount += fileCount;
            
            return totalCount;
        }
        public List<string> GetLocallyCreatedDirs()
        {
            List<string> LocallyCreatedDirectories = new List<string>();

            foreach (var dir in Directory.EnumerateDirectories(Main))
            {
                foreach (var name in Ekit.GetCategoryNames())
                { 
                    var dirInfo = new DirectoryInfo(dir);
                    var dirName = dirInfo.Name;
                    if (dirName.Equals(name)) 
                        LocallyCreatedDirectories.Add(dir);
                }
            }
            return LocallyCreatedDirectories;
        }
        public void Unpack()
        { 
            if (ProgressStatus != Status.Unpacking)
            {
                ProgressStatus = Status.Unpacking;
 
                OnUnpackStarted();
                InitialFileCount = GetFileCountFromSubDirectories(Main);

                foreach( var directory in GetLocallyCreatedDirs())
                {
                    foreach(var _file in Directory.EnumerateFiles(directory))
                    {
                        File.Move(_file, Path.Combine(Main, Path.GetFileName(_file)));
                        MonitorFileCount();
                    }
                    Directory.Delete(directory);
                    MonitorFileCount();
                }
            }
            MonitorFileCount();
            InitialFileCount = GetFileCount();
        }

        public bool ValidateDirectory(string Path)
        {
            bool DoesItExist = Directory.Exists(Path);

            return DoesItExist;
        }

        public void OrganizeDownloads()
        {
            OnOrganizingStarted();
            InitialFileCount = GetFileCount();
            ProgressStatus = Status.Organizing;
          
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
        public void CreateDummieFiles(int count)
        {
            for (int i = 0; i <= count; i++)
            {
                string name = i.ToString() + ".txt";
                File.Create(Path.Combine(Main, name));
            }
            InitialFileCount = Files.Count() - 1;
        } // testing Purposes

        public enum Status
        {
            Ready,
            Organizing,
            Unpacking,
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
        }
        public double CompletionPercentage { get; set; }    
        public double NewFileCount { get; set; }
        public double OldFileCount { get; set; }
    }
}
