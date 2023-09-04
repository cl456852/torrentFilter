using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DAL;
using DB;
using MODEL;

namespace BLL
{


    public class PornoMagnet : FilterBase
    {

        private Regex magnetRegex=new Regex("magnet:.*?\"");
        private Regex titleRegex = new Regex("<h1>.*?<\\/h1>");
        private Regex sizeRegex = new Regex("Размер<\\/td><td>.*?<\\/td><\\/tr>");

        public override List<RarbgTitle> GetList(string directoryStr)
        {
            
            string[] paths = Directory.GetFiles(directoryStr, "*", SearchOption.TopDirectoryOnly);
            List<RarbgTitle> list = new List<RarbgTitle>();
            foreach(string path in paths)
            {
                if (path.Contains("https^"))
                {
                    continue;
                }
                string content = Tool.ReadFile(path);
                RarbgTitle rarbgTitle = new RarbgTitle();
                rarbgTitle.Path = path;
                rarbgTitle.Name = titleRegex.Match(content).Value.Replace("<h1>","").Replace("</h1>","");
                string sizeStr = sizeRegex.Match(content).Value.Replace("Размер</td><td>", "").Replace("</td></tr>","");
                rarbgTitle.Maglink = magnetRegex.Match(content).Value.Replace("\"", "").Replace("amp;","");
                
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