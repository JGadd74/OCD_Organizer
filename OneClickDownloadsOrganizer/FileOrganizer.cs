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
      
        public FileOrganizer() => InitialFileCount = Directory.EnumerateFiles(ActivePath).Count() - 1;

        private static string SecondaryPath = DefaultPath;
        private static readonly string DefaultPath = @"C:\Users\" + Environment.UserName + @"\Downloads";
        private static string ActivePath = DefaultPath;

        readonly ExtensionsKit Ekit = new ExtensionsKit();
        private static IEnumerable<string> Files = Directory.EnumerateFiles(ActivePath);
        private const int CategoryName = 0;
        public static int InitialFileCount;
        
        public static Status ProgressStatus = Status.Ready;
        //public int GetFileCount() => Directory.EnumerateFiles(ActivePath).Count();
        public int GetFileCount()
        {
            if (ActivePath == DefaultPath) return Directory.EnumerateFiles(ActivePath).Count() - 1;
            else return Directory.EnumerateFiles(ActivePath).Count();
        }
        //BUG any dir other than Downloads doesn't have secret hidden file.  Fix

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
            else if (GetFileCountFromSubDirectories(ActivePath) == 0 && ProgressStatus == Status.Unpacking)
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

            foreach (var dir in Directory.EnumerateDirectories(ActivePath))
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
                InitialFileCount = GetFileCountFromSubDirectories(ActivePath);

                foreach( var directory in GetLocallyCreatedDirs())
                {
                    foreach(var _file in Directory.EnumerateFiles(directory))
                    {
                        File.Move(_file, Path.Combine(ActivePath, Path.GetFileName(_file)));
                        MonitorFileCount();
                    }
                    Directory.Delete(directory);
                    MonitorFileCount();
                }
            }
            MonitorFileCount();
            InitialFileCount = GetFileCount();
        }

        public bool ValidateDirectory(string path)
        {
            bool dirExists = Directory.Exists(path);
            if(dirExists) SetNewLocation(path);
            return dirExists;
        }

        private void SetNewLocation(string path)
        {
            SecondaryPath = path ?? "";
        }

        public void UseDefaultLocation()
        {
            ActivePath = DefaultPath;
            Files = Directory.EnumerateFiles(ActivePath);
        }
        public void UseCustomLocation()
        {
            ActivePath = SecondaryPath;
            Files = Directory.EnumerateFiles(ActivePath);
        }

        public void OrganizeActivePath()
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
                            if (!Directory.Exists(System.IO.Path.Combine(ActivePath, category[CategoryName])))
                            {
                                Directory.CreateDirectory(System.IO.Path.Combine(ActivePath, category[CategoryName]));
                            }
                            string fileName = Path.GetFileName(file);
                            File.Move(file, Path.Combine(ActivePath, category[CategoryName], fileName));
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
                File.Create(Path.Combine(ActivePath, name));
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
