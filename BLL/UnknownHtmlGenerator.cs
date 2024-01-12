using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace BLL
{
    public class UnknownHtmlGenerator:IUnknown
    {
        public void Process(string path)
        {
            string regex;
            if (path.Contains("PornoMagnet"))
            {
                regex = "<h1>.*?Добавлен";


            }
            else
            {
                regex = "<div class=\"detailsdiv\">.*?<div class=\"page-footer\">";
            }
            
            DirectoryInfo theFolder = new DirectoryInfo(path);
            string result = "";
            foreach (FileInfo file in theFolder.GetFiles())
            {
                StreamReader sr = new StreamReader(file.FullName);
                string content = sr.ReadToEnd();
                string poster = Regex.Match(content, regex, RegexOptions.Singleline).Value;
                result += poster + "<br>\r\n";
            }
            File.WriteAllText(Path.Combine(path, "result.htm"), result);
        }
    }
}
