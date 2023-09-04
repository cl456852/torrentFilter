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
    public class Btdig : FilterBase
    {
        private Regex magnetRegex = new Regex("(magnet:.*?)\"");
        private Regex titleRegex = new Regex("style=\"padding-left:0em\">([^<]+)<\\/div>");
        private Regex sizeRegex = new Regex("<span class=\"torrent_size\" style=\"color:#666;padding-left:10px\">(.*?)<\\/span>");
        public override List<RarbgTitle> GetList(string directoryStr)
        {
            string[] paths = Directory.GetFiles(directoryStr, "*", SearchOption.TopDirectoryOnly);
            List<RarbgTitle> list = new List<RarbgTitle>();
            foreach (string path in paths)
            {
                string html = Tool.ReadFile(path).Replace("<b style=\"color:red; background-color:yellow\">","").Replace("</b>","");
                string[] items = html.Split(new string[] { "one_result" }, StringSplitOptions.None);
                foreach (string item in items)
                {
                    if (item.Contains("<html><head><title>"))
                        continue;
                    RarbgTitle rarbgTitle = new RarbgTitle();
                    rarbgTitle.Maglink = magnetRegex.Match(item).Groups[1].Value.Replace("amp;","").Replace("&","^&").TrimStart();
                    rarbgTitle.Name = titleRegex.Match(item).Groups[1].Value.TrimStart();
                    string sizeStr = sizeRegex.Match(item).Groups[1].Value;

                    if (sizeStr.EndsWith("GB"))
                    {
                        rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("GB", "").Trim()) * 1024);
                    }
                    else if (sizeStr.EndsWith("MB"))
                    {
                        rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("MB", "").Trim()));
                    }
                    list.Add(rarbgTitle);
                }
               
            }
            return list;

        }


    }
}
