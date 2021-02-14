using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace nhcustom
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            string configLocation = @"./config.txt";
            string databaseLocation = @"./database.txt";
            string outputLocation = @"./output";
            string inputLocation = @"./input";

            List<string> importantItems = new List<string>
            {
                configLocation,
                databaseLocation,
                inputLocation,
            };

            HelpMsg help;

            FolderHandler copier;

            StreamReader config;
            StreamReader database;

            bool isExept;
            bool isGreedy;
            bool keepMode = false;
            bool foundFlag = false;
            bool confError = false;
            bool inputStructure = false;

            string keepFlag = "(KEEP)";
            string removeFlag = "(REMOVE)";

            List<string> classList = new List<string>();
            List<string> equipList = new List<string>();
            List<string> hatList = new List<string>();
            List<string> updateList = new List<string>();
            List<string> workList = new List<string>();
            List<string> metaList = new List<string>();
            List<string> pathList = new List<string>();
            List<string> errorLog = new List<string>();


            Pattern patterns;

            string currPattern;

            string dbClassR = @"CLASS=\(CLASSREGEX\)";
            string dbClassGreedyR = @"CLASS=(\(|\(.*,\s)CLASSREGEX(,\s.*|\))";

            string dbHatsR = @"HAT=\(HATREGEX\)";

            string dbBodyR = @"EQUIP=\(BODYREGEX\)";
            string dbBodyGreedyR = @"EQUIP=(\(|\(.*,\s)BODYREGEX(,\s.*|\))";

            string dbUpdatesR = @"UPDATE=\(UPDATEREGEX\)";


            //regex to find the name of updates in the database
            Regex updatePattern = new Regex(@"UPDATE=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find all the existing bodyparts in the database
            Regex bodyPattern = new Regex(@"EQUIP=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find all the classes in the database
            Regex classPattern = new Regex(@"CLASS=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find all the cosmetics in the database
            Regex hatPattern = new Regex(@"HAT=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find all the dates in the database
            Regex datePattern = new Regex(@"DATE=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find the paths in a string coming from the database
            Regex pathPattern = new Regex(@"PATH=\(([^\)]*)\)", RegexOptions.IgnoreCase);

            //regex to find dates in the config file
            Regex dateConfig = new Regex(@"(\d{4})(-\d{2})?(-\d{2})?(/)?(\d{4})?(-\d{2})?(-\d{2})?$");


            foreach (string arg in args)
            {
                if (arg == "--help")
                {
                    help = new HelpMsg();
                    help.Display();
                    Environment.Exit(0);
                }
            }

            //check if the necessary files and folders are here
            foreach (string item in importantItems)
            {
                if (File.Exists(item) || Directory.Exists(item))
                {
                    if (item == inputLocation)
                    {
                        string[] di = Directory.GetDirectories(inputLocation);
                        foreach (string dir in di)
                        {
                            if (dir.Equals(Path.Combine(inputLocation, "models")))
                            {
                                inputStructure = true;
                            }
                        }
                        if (!inputStructure)
                        {
                            Console.WriteLine("Error: \"" + item + "\" is empty or doesn't have the proper structure." +
                            " You need to have a decompiled version of" +
                            " no-hats-mod/no-hats-bgum in this folder.");
                            Console.WriteLine("The filepath should be like this: ./input/models/...");
                            Console.Read();
                            Environment.Exit(1);
                        }
                    }
                    continue;
                }
                else
                {
                    Console.WriteLine("Error: \"" + item + "\" hasn't been found in the current directory. Exiting.");
                    Console.Read();
                    Environment.Exit(1);
                }
            }



            Console.WriteLine("nhcustom 0.3 - no hats bgum modifier");
            Console.WriteLine("Launch with \"--help\" to see the documentation.\n");
            Console.WriteLine("Reading the configuration file...\n");


            //go through each line of the database
            using (database = new StreamReader(databaseLocation))
            {
                string dbLine;
                while ((dbLine = database.ReadLine()) != null)
                {
                    //find all the classes referenced in the line and add them to "classList"
                    Match classSearch = classPattern.Match(dbLine);
                    GrepLine(classSearch, classList, true);


                    //find every updates in the line and add them to "updateList"
                    Match updateSearch = updatePattern.Match(dbLine);
                    GrepLine(updateSearch, updateList, false);


                    //find every hats in the line and add them to "hatList"
                    Match hatSearch = hatPattern.Match(dbLine);
                    GrepLine(hatSearch, hatList, false);


                    //find every equip regions in the line and add them to "equipList"
                    Match equipSearch = bodyPattern.Match(dbLine);
                    GrepLine(equipSearch, equipList, true);

                }
            }

            using (config = new StreamReader(configLocation))
            {
                string confLine;

                while ((confLine = config.ReadLine()) != null)
                {
                    isExept = false;
                    isGreedy = false;
                    Match dateMatch = dateConfig.Match(confLine);
                    List<string> foundPatterns;

                    //these next lines of code get rid of comments and prepare the line and the booleans in general
                    if (confLine.Contains("#"))
                    {
                        confLine = confLine.Remove(confLine.IndexOf("#", StringComparison.CurrentCulture));
                    }
                    confLine = confLine.Trim(' ');
                    //this happens after removing the comments, because some comments might take an entire line
                    if (confLine.Equals(""))
                    {
                        continue;
                    }
                    if (confLine.Equals(keepFlag) || confLine.Equals(removeFlag))
                    {
                        if (foundFlag == false)
                        {
                            if (confLine.Equals(keepFlag))
                            {
                                keepMode = true;
                                WriteMsg("(KEEP) flag found: entries in the config file will be what's kept in the game.\n", ConsoleColor.Magenta, true);
                            }
                            else
                            {
                                WriteMsg("(REMOVE) flag found: entries in the config file will be what's removed in the game.\n", ConsoleColor.Blue, true);
                            }
                            foundFlag = true;
                        }
                        else
                        {
                            WriteMsg("A (REMOVE) or (KEEP) flag has been ignored as it's been already set.\n", ConsoleColor.DarkGray, true);
                        }
                        continue;
                    }



                    //write the config line before removing special characters
                    Console.Write(confLine + " ");


                    if (confLine.StartsWith("!", StringComparison.CurrentCulture))
                    {
                        confLine = confLine.Replace("!", "");
                        isExept = true;
                    }
                    if (confLine.EndsWith("*", StringComparison.CurrentCulture))
                    {
                        confLine = confLine.Replace("*", "");
                        isGreedy = true;
                    }


                    //match the line with the content of the created lists

                    if (classList.Contains(confLine))
                    {
                        WriteMsg("This is a class.", ConsoleColor.Yellow, false);
                        if (isGreedy)
                        {
                            currPattern = dbClassGreedyR.Replace("CLASSREGEX", confLine);
                        }
                        else
                        {
                            currPattern = dbClassR.Replace("CLASSREGEX", confLine);
                        }
                        //create a new Pattern class and send it the regex string and the location of the database
                        patterns = new Pattern(currPattern, databaseLocation);

                        //retrieve the results of the Pattern class and send it to the ModWorkList method that will
                        //add or remove the found entries in the Worklist depending if the config line is an exception or not
                        foundPatterns = new List<string>(patterns.Retrieve());
                        ModWorkList(foundPatterns, ref workList, isExept);
                    }

                    else if (hatList.Contains(confLine))
                    {
                        WriteMsg("This is a cosmetic.", ConsoleColor.DarkYellow, false);
                        currPattern = dbHatsR.Replace("HATREGEX", confLine);

                        patterns = new Pattern(currPattern, databaseLocation);

                        foundPatterns = new List<string>(patterns.Retrieve());
                        ModWorkList(foundPatterns, ref workList, isExept);
                    }

                    else if (updateList.Contains(confLine))
                    {
                        WriteMsg("This is an update.", ConsoleColor.Magenta, false);
                        currPattern = dbUpdatesR.Replace("UPDATEREGEX", confLine);

                        patterns = new Pattern(currPattern, databaseLocation);

                        foundPatterns = new List<string>(patterns.Retrieve());
                        ModWorkList(foundPatterns, ref workList, isExept);
                    }

                    else if (equipList.Contains(confLine))
                    {
                        WriteMsg("This is an equip region.", ConsoleColor.Green, false);

                        if (isGreedy)
                        {
                            currPattern = dbBodyGreedyR.Replace("BODYREGEX", confLine);
                        }
                        else
                        {
                            currPattern = dbBodyR.Replace("BODYREGEX", confLine);
                        }

                        patterns = new Pattern(currPattern, databaseLocation);

                        foundPatterns = new List<string>(patterns.Retrieve());
                        ModWorkList(foundPatterns, ref workList, isExept);
                    }

                    else if (dateMatch.Success)
                    {
                        string date = BuildDate(dateMatch);

                        WriteMsg("This is a date. ", ConsoleColor.DarkMagenta, false);
                        WriteMsg(Convert.ToString(date), ConsoleColor.DarkMagenta, true);
                        WriteMsg(" (YYYY-mm-dd)", ConsoleColor.DarkMagenta, false);


                        string[] splitDate = date.Split('/');
                        DateTime confDate1 = DateTime.Parse(splitDate[0]);
                        DateTime confDate2 = DateTime.Parse(splitDate[1]);


                        string dbLine;
                        using (database = new StreamReader(databaseLocation))
                        {
                            while ((dbLine = database.ReadLine()) != null)
                            {
                                Match d = datePattern.Match(dbLine);
                                if (d.Success)
                                {
                                    DateTime dbDate = DateTime.Parse(d.Groups[1].Value);

                                    //if the date found in the database is between the 2 dates built by the BuildDate method...
                                    if (dbDate >= confDate1 && dbDate <= confDate2)
                                    {
                                        if (!workList.Contains(dbLine))
                                        {
                                            if (!isExept)
                                                workList.Add(dbLine);
                                        }
                                        else if (isExept)
                                        {
                                            workList.Remove(dbLine);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        WriteMsg("Unknown parameter!", ConsoleColor.Red, false);
                        confError = true;
                    }

                    if (isExept)
                    {
                        WriteMsg(" Exception.", ConsoleColor.Gray, false);
                    }

                    if (isGreedy)
                    {
                        WriteMsg(" Asterisk.", ConsoleColor.Gray, false);
                    }

                    Console.WriteLine();

                }
            }

            if (!foundFlag)
            {
                WriteMsg("A (REMOVE) or (KEEP) flag hasn't been found, the program will default to the (REMOVE) flag.\n", ConsoleColor.DarkYellow, true);
            }

            if (confError)
            {
                Console.WriteLine("\nThere were some errors in the config file. Press Enter to continue anyway...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\nPress Enter to start modifying the mod...");
                Console.ReadLine();
            }


            Console.WriteLine("\nFormatting paths...");
            //these lines below check if a path exist for each lines in workList and replace a couple of words
            foreach (string line in workList)
            {
                if (line.Contains("PATH=()"))
                {
                    WriteMsg("Warning: The database doesn't contain a path for: " + hatPattern.Match(line).Groups[1].Value + ".\n", ConsoleColor.Yellow, false);
                    continue;
                }

                List<string> tempClasses = new List<string>();
                if (line.Contains("(CLASS)"))
                {
                    Match c = classPattern.Match(line);

                    string classResult = c.Groups[1].Value;
                    string[] split = classResult.Split(',');

                    foreach (string piece in split)
                    {
                        string trim = piece.Trim(' ');
                        tempClasses.Add(trim);
                    }
                    if (tempClasses.Contains("All classes"))
                    {
                        tempClasses.Remove("All classes");
                        foreach (string className in classList)
                        {
                            //There is an empty string in the classList. Where does it come from?
                            if (className == "")
                            {
                                continue;
                            }

                            tempClasses.Add(className);
                        }
                        tempClasses.Remove("All classes");
                    }
                    if (tempClasses.Contains("Demoman"))
                    {
                        tempClasses.Remove("Demoman");
                        tempClasses.Add("demo");
                    }
                    foreach (string className in tempClasses)
                    {
                        string tmpPath = line.Replace("(CLASS)", className.ToLower());
                        if (!metaList.Contains(tmpPath))
                        {
                            metaList.Add(tmpPath);
                        }
                    }
                }
                else
                {
                    if (!metaList.Contains(line))
                    {
                        metaList.Add(line);
                    }
                }

            }

            //these lines extract the path(s) of each line in metaList and add them in the pathList.
            foreach (string modPath in metaList)
            {
                Match p = pathPattern.Match(modPath);

                string pathResult = p.Groups[1].Value;
                string[] pathSplit = pathResult.Split(',');
                foreach (string piece in pathSplit)
                {
                    string trim = piece.Trim(' ');
                    if (!pathList.Contains(trim))
                    {
                        pathList.Add(trim);
                    }
                }
            }

            Console.WriteLine("Cleaning the output directory...");
            if (Directory.Exists(outputLocation))
            {
                DirectoryInfo outputInfo = new DirectoryInfo(outputLocation);
                foreach (FileInfo file in outputInfo.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in outputInfo.EnumerateDirectories())
                {
                    dir.Delete(true);
                }

            }
            else
            {
                Directory.CreateDirectory("./output");
            }


            copier = new FolderHandler(inputLocation, outputLocation, pathList);

            if (keepMode)
            {
                Console.WriteLine("Copying files (this may take a while)...");
                copier.KeepMode = true;
            }
            else
            {
                Console.WriteLine("Copying files...");
            }

            errorLog.AddRange(copier.Copy());

            foreach (string line in errorLog)
            {
                WriteMsg(line + "\n", ConsoleColor.Red, false);
            }

            WriteMsg("Done.", ConsoleColor.Green, false);
            Console.ReadLine();
        }


        //method that add info to the Match it receives if it's incomplete to be a date (yes, this is hideous)
        private static string BuildDate(Match d)
        {
            string date = Convert.ToString(d.Groups[1]);

            //if it doesn't contain a month, add a default month and a day (because the day will be absent if the month already is)
            if (d.Groups[2].Value == "")
            {
                date += "-01-01";
            }
            else
            {
                date += d.Groups[2];

                //if it doesn't contain a day, add a default day
                if (d.Groups[3].Value == "")
                {
                    date += "-01";
                }
                else
                {
                    date += d.Groups[3];
                }
            }

            //if it doesn't contain a slash, add the same year from the 1st date and add a default month and day
            if (d.Groups[4].Value == "")
            {
                date += "/" + d.Groups[1] + "-12-31";
            }
            else
            {
                date += d.Groups[4];

                //if the 2nd date doesn't contain a year, add the same year from the first date and a default month and day.
                if (d.Groups[5].Value == "")
                {
                    date += d.Groups[1] + "-12-31";
                }
                else
                {
                    date += d.Groups[5];

                    //if the 2nd date doesn't contain a month, add a default month and day.
                    if (d.Groups[6].Value == "")
                    {
                        date += "-12-31";
                    }
                    else
                    {
                        date += d.Groups[6];

                        //if the 2nd date doesn't contain a day, add a default one.
                        if (d.Groups[7].Value == "")
                        {
                            date += "-31";
                        }
                        else
                        {
                            date += d.Groups[7];
                        }
                    }
                }
            }
            return date;
        }



        private static void WriteMsg(string msg, ConsoleColor color, bool bgColor)
        {
            if (bgColor)
            {
                Console.BackgroundColor = color;
            }
            else
            {
                Console.ForegroundColor = color;
            }
            Console.Write(msg);

            Console.ResetColor();
        }



        private static void ModWorkList(List<string> foundPatterns, ref List<string> workList, bool isExept)
        {
            if (!isExept)
            {
                foreach (string entry in foundPatterns)
                {
                    if (!workList.Contains(entry))
                    {
                        workList.Add(entry);
                    }
                }
            }
            else
            {
                foreach (string entry in foundPatterns)
                {
                    workList.Remove(entry);
                }
            }
        }


        private static void GrepLine(Match search, List<string> list, bool trimNeeded)
        {
            //add the found match(es) to the specified list
            if (search.Success)
            {
                if (!trimNeeded)
                {
                    string result = search.Groups[1].Value;
                    if (!list.Contains(result))
                    {
                        list.Add(result);
                    }
                }
                else
                {
                    string[] splittedResult = search.Groups[1].Value.Split(',');
                    foreach (string entry in splittedResult)
                    {
                        string trim = entry.Trim(' ');
                        if (!list.Contains(trim))
                        {
                            list.Add(trim);
                        }
                    }
                }
            }
        }
    }
}
