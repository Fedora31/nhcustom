using System;
using System.IO;
using System.Reflection;

namespace nhcustom
{
    public class HelpMsg
    {
        string embedFolderString = @".embed.";
        string helpFile = "help.txt";

        public void Display()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream;

            using (stream = assembly.GetManifestResourceStream(assembly.GetName().Name + embedFolderString + helpFile))
            {
                using (StreamReader helpMsg = new StreamReader(stream))
                {
                    string line;
                    while ((line = helpMsg.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            Console.Read();
        }
    }
}
