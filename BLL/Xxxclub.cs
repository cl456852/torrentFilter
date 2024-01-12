using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MODEL;

namespace BLL
{
    public class Xxxclub : FilterBase
    {
        
        private Regex magnetRegex=new Regex("(magnet:.*?)\"");
        private Regex titleRegex = new Regex("og:title\" content=\"(.*?)\"");
        private Regex sizeRegex = new Regex("<li><span>Size<\\/span><span> : <\\/span><span>(.*)<\\/span><\\/li>");
        private Regex smallRegex = new Regex("<dd>480p|<dd>360p");
        public override List<RarbgTitle> GetList(string directoryStr)
        {
            List<RarbgTitle> list = new List<RarbgTitle>();
            string[] paths = Directory.GetFiles(directoryStr, "*", SearchOption.TopDirectoryOnly);
            foreach (string path in paths)
            {
                if (path.Contains("https^"))
                {
                    continue;
                }
                string content = Tool.ReadFile(path);
                RarbgTitle rarbgTitle = new RarbgTitle();
                rarbgTitle.Path = path;
                if(smallRegex.IsMatch(content))
                {
                    Tool.moveFile("sd", rarbgTitle.Path);
                    continue;
                }
                rarbgTitle.Name = titleRegex.Match(content).Groups[1].Value;
                string sizeStr = sizeRegex.Match(content).Groups[1].Value;
                rarbgTitle.Maglink = magnetRegex.Match(content).Groups[1].Value.Replace("amp;","");
                
                if (sizeStr.EndsWith("GB"))
                {
                    rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("GB", "")) * 1024);
                }
                else if(sizeStr.EndsWith("MB"))
                {
                    rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("MB", "")));
                }
                list.Add(rarbgTitle);
            }
            return list;
        }
    }
}