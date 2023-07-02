using MODEL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL
{
    public class PornoMagnetUnknown
    {

        public void process(string path)
        {

            DirectoryInfo theFolder = new DirectoryInfo(path);
            string result="";
            foreach (FileInfo file in theFolder.GetFiles())
            {
                string content=Tool.ReadFile(file.FullName);
                content = content.Split(new string[] { "<h1>", "<td width=\"5%\">Добавлен</td>" }, StringSplitOptions.None)[1];
                result += content;
            }
            File.WriteAllText(Path.Combine(path,"result.htm"),result);
        }

    }
}
