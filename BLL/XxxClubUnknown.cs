using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class XxxClubUnknown : IUnknown
    {
        public void Process(string path)
        {
            DirectoryInfo theFolder = new DirectoryInfo(path);
            string result="";
            foreach (FileInfo file in theFolder.GetFiles())
            {
                StreamReader sr = new StreamReader(file.FullName);
                string content = sr.ReadToEnd();
                content = content.Split(new string[] { "<div style=\"min-height:830px","<hr>" }, StringSplitOptions.None)[1];
                result += content+"<br>\n";
            }

            File.WriteAllText(Path.Combine(path,"result.htm"),result);

        }
    }
}
