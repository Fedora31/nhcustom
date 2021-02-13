using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace nhcustom
{
    public class Pattern
    {

        private string _pattern;
        private string _src;


        public Pattern(string pattern, string src)
        {
            this._pattern = pattern;
            this._src = src;
        }


        public List<string> Retrieve()
        {
            string dbLine;
            StreamReader database;
            Regex regPattern = new Regex(_pattern);
            List<string> foundPatterns = new List<string>();

            using (database = new StreamReader(_src))
            {
                while ((dbLine = database.ReadLine()) != null)
                {
                    Match dbScan = regPattern.Match(dbLine);
                    if (dbScan.Success)
                    {
                        foundPatterns.Add(dbLine);
                    }
                }
            }
            return foundPatterns;
        }
    }
}
