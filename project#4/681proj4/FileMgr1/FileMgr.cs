/////////////////////////////////////////////////////////////////////
// FileMgr1 - provides file and directory handling for navigation   //
// ver 1.0                                                          //
// Weiheng, CSE681 - Software Modeling and Analysis, Fall 2017      //
//Source Jim Fawcett                                                //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines IFileMgr interface, FileMgrFactory, and LocalFileMgr
 * classes.  Clients use the FileMgrFactory to create an instance bound to
 * an interface reference.
 * 
 * The FileManager finds files and folders at the root path and in any
 * subdirectory in the tree rooted at that path.
 * 
 * Public interface
 * public interface IFileMgr-----provide a interface for the class
 * public class FileMgrFactory-----factory of filemgr
 * public class LocalFileMgr : IFileMgr -----inherit from the ifilemgr
 * public IEnumerable<string> getFiles()-----get list of files in the path
 * public IEnumerable<string> getDirs()-----get list of dirs in the path
 * 
 * Required Files:
* =============================================
* Environment1
* 
* 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 23 Oct 2017
 * - moved all Environment definitions into an Environment project
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Navigator
{
    public enum FileMgrType { Local, Remote }


    public interface IFileMgr
    {
        IEnumerable<string> getFiles();
        IEnumerable<string> getDirs();
        Stack<string> pathStack { get; set; }
        string currentPath { get; set; }
    }

    public class FileMgrFactory
    {
        static public IFileMgr create(FileMgrType type)
        {
            if (type == FileMgrType.Local)
                return new LocalFileMgr();
            else
                return null;  
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Concrete class for managing local files

    public class LocalFileMgr : IFileMgr
    {
        public string currentPath { get; set; } = "";
        public Stack<string> pathStack { get; set; } = new Stack<string>();

        public LocalFileMgr()
        {
            pathStack.Push(currentPath);  // stack is used to move to parent directory
        }
        //----< get names of all files in current directory >------------

        public IEnumerable<string> getFiles()
        {
            List<string> files = new List<string>();
            string path = Path.Combine(Environment1.root, currentPath);
            string absPath = Path.GetFullPath(path);
            files = Directory.GetFiles(absPath).ToList<string>();
            currentPath = "";
            for (int i = 0; i < files.Count(); ++i)
            {
              files[i] = Path.Combine(currentPath, Path.GetFileName(files[i]));
           }
            return files;
        }
        //----< get names of all subdirectories in current directory >---

        public IEnumerable<string> getDirs()
        {
            List<string> dirs = new List<string>();
            string path = Path.Combine(Environment1.root, currentPath);
            dirs = Directory.GetDirectories(path).ToList<string>();
            for (int i = 0; i < dirs.Count(); ++i)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                dirs[i] = Path.Combine(currentPath, dirName);
            }
            return dirs;
        }
    }
#if (TEST_FileMgr)

  class Program
  {
    static void Main(string[] args)
    {
        currentpath=@"../../../repostorage/";
        getFiles();
        getDirs();

    }
  }
#endif
}
