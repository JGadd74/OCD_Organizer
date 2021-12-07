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
        public FileOrganizer() => InitialFileCount = (Directory.EnumerateFiles(ActivePath).Count() - 1) > -1 ?
                                                      Directory.EnumerateFiles(ActivePath).Count() - 1 :
                                                      Directory.EnumerateFiles(ActivePath).Count();
        private static string SecondaryPath = DefaultPath;
        private static readonly string DefaultPath = @"C:\Users\" + Environment.UserName + @"\Downloads";
        private static string ActivePath = DefaultPath;
        readonly ExtensionsKit Ekit = new ExtensionsKit();
        private static IEnumerable<string> Files = Directory.EnumerateFiles(ActivePath);
        private const int CategoryName = 0;
        public static int InitialFileCount;
        public static Status ProgressStatus = Status.Ready;
        public int GetFileCount()
        {
            var cnt = Directory.EnumerateFiles(ActivePath).Count();
            return cnt - 1 > -1 ? cnt - 1 : cnt;
        }
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
            else if (GetFileCountFromSubDirectories() == 0 && ProgressStatus == Status.Unpacking)
            {
                ProgressStatus = Status.Finished;
                OnUnpackingFinished();
            }
        }
        public bool ThereAreFiles() => GetFileCount() > 0;
        public int GetFileCountFromSubDirectories()
        {
            int totalCount = 0;
            var LocallyCreatedDirectories = GetLocallyCreatedDirs();

            var fileCounts = from directory in LocallyCreatedDirectories
                             select Directory.EnumerateFiles(directory).Count();

            foreach (int fileCount in fileCounts) totalCount += fileCount;

            return totalCount;
        }
        public string[] GetLocallyCreatedDirs()
        {
            List<string> LocallyCreatedDirectories = new List<string>();

            foreach (var dir in Directory.EnumerateDirectories(ActivePath))
            {
                foreach (var name in Ekit.GetCategoryNames())
                {
                    var dirName = new DirectoryInfo(dir).Name;
                    if (dirName.Equals(name))
                        LocallyCreatedDirectories.Add(dir);
                }
            }
            return LocallyCreatedDirectories.ToArray();
        }
        public void Unpack() // BUG crashes when unpacking a folder from another folder
        {
            if (ProgressStatus != Status.Unpacking)
            {
                ProgressStatus = Status.Unpacking;
                OnUnpackStarted();
                InitialFileCount = GetFileCountFromSubDirectories();

                foreach (var directory in GetLocallyCreatedDirs())
                { // bug fix, check ^^directory for sub directories, don't delete if present
                    foreach (var _file in Directory.EnumerateFiles(directory))
                    {
                        if (!File.Exists(Path.Combine(ActivePath, Path.GetFileName(_file)))) 
                        {
                            File.Move(_file, Path.Combine(ActivePath, Path.GetFileName(_file)));
                        }
                        else if(File.Exists(Path.Combine(ActivePath, Path.GetFileName(_file))))
                        {
                            File.Delete(Path.Combine(ActivePath, Path.GetFileName(_file)));
                            File.Move(_file, Path.Combine(ActivePath, Path.GetFileName(_file)));
                        }
                        MonitorFileCount();
                    }
                    if(Directory.EnumerateDirectories(directory).Count() == 0) // sustainable bug fix?
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
            if (dirExists) SetNewLocation(path);
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
        public void OrganizeActivePathX() // terribly inefficient.  rewrite to also include UNKNOWN types
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
                            if (!Directory.Exists(System.IO.Path.Combine(ActivePath, category[CategoryName]))) // sustainable bug fix??
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

        public void OrganizeActivePath() //BUG** crashes when moving file to location with a duplicate file
        {
            OnOrganizingStarted(); // start tracking progress
            InitialFileCount = GetFileCount();
            ProgressStatus = Status.Organizing;

            foreach (var file in Files)
            {
                var fileMoved = false;

                foreach (var category in Ekit.ExtensionCategories)
                {
                    if (category.Contains(Path.GetExtension(file)))
                    {
                        if (!Directory.Exists(Path.Combine(ActivePath, category[CategoryName])))
                        {
                            Directory.CreateDirectory(Path.Combine(ActivePath, category[CategoryName]));
                        }

                        string fileName = Path.GetFileName(file);
                        // CREATE MESSAGE BOX WITH WARNING, LET USER DECIDE WHAT TO DO??
                        if (File.Exists(Path.Combine(ActivePath, category[CategoryName], fileName))) File.Delete(Path.Combine(ActivePath, category[CategoryName], fileName));
                        File.Move(file, Path.Combine(ActivePath, category[CategoryName], fileName));
                        fileMoved = true;
                        MonitorFileCount();
                        break;
                    }
                }
              


                if (!fileMoved)
                {
                    var unknownT = ExtensionsKit.unknownTypes[CategoryName];
                    if (!Directory.Exists(Path.Combine(ActivePath, unknownT)))
                         Directory.CreateDirectory(Path.Combine(ActivePath, unknownT));
                    string fileName = Path.GetFileName(file);
                    File.Move(file, Path.Combine(ActivePath, unknownT, fileName));
                    MonitorFileCount();
                }
            }
            MonitorFileCount();
        }
        public void CreateDummieFiles(int count)
        {
            for (int i = 0; i <= count; i++)
            {
                var name = i.ToString() + ".txt";
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
            CompletionPercentage = 100 - (100 * (NewFileCount / OldFileCount));
        }
        public double CompletionPercentage { get; set; }
        public double NewFileCount { get; set; }
        public double OldFileCount { get; set; }
    }
}