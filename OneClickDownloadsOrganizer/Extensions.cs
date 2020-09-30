using System.Collections;
using System.Collections.Generic;

using System.Linq;


namespace OneClickDownloadsOrganizer
{
    
    public class ExtensionsKit
    {
       //

        public List<string[]> ExtensionCategories = new List<string[]>() 
        { // arranged so more common categories are checked first
            compressed ,
            wordProcessor ,
            spreadsheet ,
            image ,
            video ,
            audio ,
            executable ,
            diskMedia , 
            database , 
            email , 
            font , 
            internet , 
            presentation , 
            programming , 
            system ,
        };

        
        // a common category? 
        // gets common extensions
        //returns the selected extensions category name


        public string[] GetExtensionsArray() // update to use linq
        {
            List<string> extensions = new List<string>();
            foreach(string[] category in ExtensionCategories)
            {
                foreach(string entry in category)
                {
                    if(entry[0] == '.')
                    {
                        extensions.Add(entry);
                    }
                }
            }
            
            return extensions.ToArray();
        }

        public string[] GetCategoryNames()
        {
            var nameList = from category in ExtensionCategories select category[0];
            return nameList.ToArray();
        }


        
        public static string[] audio = new string[] {"Audio Files" , ".aif", ".cda" , ".mid" , ".midi" , ".mp3" , ".mpa" , ".ogg" , ".wav" , ".wma" , ".wpl"};

        public static string[] compressed = new string[] {"Compressed Files" , ".7z" , ".arj" , ".deb" , ".pkg" , ".rar" , ".rpm" , ".tar.gz" , ".z" , ".zip"};

        public static string[] diskMedia = new string[] {"Disk Media Files" , ".bin" , ".dmg" , ".iso" , ".toast" , ".vcd"};

        public static string[] database = new string[] { "Database Files", ".csv", ".dat", ".db", ".dbf", ".log", ".mdb", ".sav", ".sql", ".tar", ".xml"};

        public static string[] email = new string[] { "Email Files", ".email", ".eml" , ".emlx" , ".msg" , ".oft" , ".ost" , ".pst" , ".vcf"};

        public static string[] executable = new string[] { "Executable Files", ".apk", ".bat" , ".bin" , ".cgi" , ".pl" , ".com" , ".exe" , ".gadget" , ".jar" , ".msi" , ".py" , ".wsf"};

        public static string[] font = new string[] { "Font Files", ".fnt", ".fon" , ".otf" , ".ttf"};

        public static string[] image = new string[] { "Image Files", ".ai", ".bmp" , ".gif" , ".ico" , ".jpeg" , ".jpg" , ".png", ".ps" , ".psd" , ".svg" , ".tif" , ".tiff"};

        public static string[] internet = new string[] { "Internet Files", ".asp", ".aspx" , ".cer" , ".cfm" , ".cgi" , ".pl" , ".css" , ".htm" , ".html" , ".js" , ".jsp" , ".part" , ".php" , ".py" , ".rss" , ".xhtml" , ".torrent"};

        public static string[] presentation = new string[] { "Presentation Files", ".key", ".odp" , ".pps" , ".ppt" , ".pptx" };

        public static string[] programming = new string[] { "Programming Files", ".c" , ".class" , ".cpp" , ".cs" , ".h" , ".java" , ".pl" , ".sh" , ".swift" , ".vb"};

        public static string[] spreadsheet = new string[] { "Spreadsheet Files", ".ods", ".xls" , ".xlsm" , ".xlsx"};

        public static string[] system = new string[] { "System Files", ".bak", ".cab" , ".cfg" , ".cpl" , ".cur" , ".dll" , ".dmp" , ".drv" , ".icns" , ".ico" , ".ini" , ".lnk" , ".msi" , ".sys" , ".tmp"};

        public static string[] video = new string[] { "Video Files", ".3g2", ".3gp" , ".avi" , ".flv" , ".h264" , ".m4v" , ".mkv" , ".mov" , ".mp4" , ".mpg" , ".mpeg" , ".rm" , ".swf" , ".vob" , ".wmv"};

        public static string[] wordProcessor = new string[] { "Word Processor Files", ".doc", ".docx" , ".odt" , ".pdf" , ".rtf" , ".tex" , ".txt" , ".wpd"};          
    }
}
