using System;
using System.Collections.Generic;
using System.IO;

namespace nhcustom
{
    public class FolderHandler
    {
        private string _inputLocation;
        private string _outputLocation;
        private List<string> _errorLog = new List<string>();
        private List<string> _pathList = new List<string>();

        //reminder: "_keepmode = true" means the parameters in the config file will be what's kept in the game, so I chose the method to copy the
        //entirety of the input folder to the output folder, then delete what's specified in the config. (if it's not in the mod, it will appear in-game.)
        private bool _keepMode = false;


        public FolderHandler(string inputLocation, string outputlocation, List<string> pathList)
        {
            this._inputLocation = inputLocation;
            this._outputLocation = outputlocation;
            this._pathList = pathList;
        }

        public bool KeepMode
        {
            set { this._keepMode = value; }
        }


        public List<string> Copy()
        {
            //copy the content of the input folder to the output folder if _keepMode is true
            if (_keepMode)
            {
                CopyAll();
                Console.WriteLine("Deleting files...");
            }

            foreach (string line in _pathList)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                Console.WriteLine(line);


                //if the line is a reference to a folder (doesn't end with ".*"):
                if (!line.EndsWith("*", StringComparison.CurrentCulture))
                {
                    if (_keepMode)
                    {
                        //delete the folder referenced by the line
                        DeleteFolderFromOutput(line);
                    }
                    else
                    {
                        //copy the folder from input to output
                        CopyFolderToOutput(line);
                    }
                }

                //but, if the line is a file (ends with ".*"):
                else
                {
                    //separate the line in 2: parentDirs, which are the parent folders (or the path to the file), and pattern, the file (minus the "*").
                    string entry = line.Remove(line.IndexOf(".", StringComparison.CurrentCulture) + 1);
                    string parentDirs = entry.Remove(entry.LastIndexOf("/", StringComparison.CurrentCulture));
                    string pattern = entry.Substring(entry.LastIndexOf("/", StringComparison.CurrentCulture) + 1);

                    if (Directory.Exists(Path.Combine(_inputLocation, parentDirs)))
                    {
                        DirectoryInfo di;
                        if (_keepMode)
                        {
                            //if you don't change to _outputLocation, you delete files in the input folder, dummy
                            di = new DirectoryInfo(Path.Combine(_outputLocation, parentDirs));
                        }
                        else
                        {
                            di = new DirectoryInfo(Path.Combine(_inputLocation, parentDirs));
                        }
                        //search the directory di for files matching the filename in "pattern". So it shoud find multiple files
                        //with different extentions (.dx90.vtx, .mdl, .vvd...)
                        FileInfo[] fi = di.GetFiles(pattern + "*", SearchOption.TopDirectoryOnly);

                        if (fi.Length > 0)
                        {
                            if (_keepMode)
                            {
                                DeleteFiles(fi);
                            }
                            else
                            {
                                CopyFilesToOutput(fi, parentDirs);
                            }
                        }
                        else
                        {
                            _errorLog.Add("Error: " + line + ": the pattern search " + "\"" + pattern + "\"" + " retreived 0 results.");
                        }
                    }
                    else
                    {
                        _errorLog.Add("Error: " + line + ": this path doesn't exist.");
                    }
                }

            }


            return _errorLog;
        }

        private void CopyAll()
        {
            //copy the content of the input folder to the output folder
            foreach (string dir in Directory.GetDirectories(_inputLocation, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(_outputLocation, dir.Substring(_inputLocation.Length + 1)));
            }
            foreach (string fileName in Directory.GetFiles(_inputLocation, "*", SearchOption.AllDirectories))
            {
                File.Copy(fileName, Path.Combine(_outputLocation, fileName.Substring(_inputLocation.Length + 1)));
            }
        }

        private void DeleteFolderFromOutput(string line)
        {
            //get all the files in the directory, delete them, then delete the directory
            if (Directory.Exists(Path.Combine(_outputLocation, line)))
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_outputLocation, line));
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                Directory.Delete(Path.Combine(_outputLocation, line));
            }
            else
            {
                _errorLog.Add("Error: " + line + ": this path doesn't exist.");
            }
        }

        private void CopyFolderToOutput(string line)
        {
            //create the folder in the output folder and copy all the files supposed to be inside it
            if (Directory.Exists(Path.Combine(_inputLocation, line)))
            {
                Directory.CreateDirectory(Path.Combine(_outputLocation, line));
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_inputLocation, line));
                foreach (FileInfo file in di.GetFiles())
                {
                    if (!File.Exists(Path.Combine(_outputLocation, line, file.Name)))
                    {
                        file.CopyTo(Path.Combine(_outputLocation, line, file.Name));
                    }
                }
            }
            else
            {
                _errorLog.Add("Error: " + line + ": this path doesn't exist.");
            }
        }

        private void DeleteFiles(FileInfo[] fi)
        {
            foreach (FileInfo file in fi)
            {
                file.Delete();
            }
        }

        private void CopyFilesToOutput(FileInfo[] fi, string path)
        {
            //create the path in the output folder
            Directory.CreateDirectory(Path.Combine(_outputLocation, path));

            //for every file in fi, copy them to the correct folder with the correct filename
            foreach (FileInfo file in fi)
            {
                if (!File.Exists(Path.Combine(_outputLocation, path, file.Name)))
                {
                    file.CopyTo(Path.Combine(_outputLocation, path, file.Name));
                }
            }
        }
    }
}
