﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using GetSize;

namespace BLL
{
    class _141javAnalysis
    {
        Regex r = new Regex("<br />VID:.*");
        Regex sizeRegex = new Regex("<br />Size:.*");
        Regex actressRegex = new Regex("<br />Actress:.*");
        Regex imageRex = new Regex("movies/.*.jpg");

        //http://www.141jav.com/movies/MILD833.jpg
        public ArrayList alys(string content, string path)
        {
            ArrayList resList = new ArrayList();
            string[] contents = content.Split(new string[] { "<h3><p>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in contents)
            {
                if (!s.Contains("artist-container"))
                    continue;
                His his = new His();
                his.OriginalHtml = s;
                his.Vid = r.Match(s).Value.Replace("<br />VID:", "").Trim();
                //if (his.Vid.Split('-').Length > 2)
                //{
                //    his.Vid = his.Vid.Substring(0, his.Vid.LastIndexOf('-'));
                //}
                his.Vid = his.Vid.Replace("-", "");
                string size = sizeRegex.Match(s).Value.Replace("<br />Size:", "").Trim();
                string sizeDigit = size.Replace("Size:", "").Replace(".<br />", "").Replace("<br />", "");
                try
                {
                    if (sizeDigit.EndsWith("GB"))
                    {
                        sizeDigit = sizeDigit.Replace("GB", "");
                        his.Size = Convert.ToDouble(sizeDigit) * 1024;

                    }
                    else
                        his.Size = Convert.ToDouble(sizeDigit.Replace("MB", "").Replace("KB", ""));
                }
                catch (Exception e)
                {
                    Console.WriteLine("CAN NOT FIND SIZE!!");
                    Console.WriteLine(e.Message);
                }
                his.Actress = actressRegex.Match(s).Value.Replace("<br />Actress: ", "").Replace("</p>", "");
                getHtml(his, size);
                resList.Add(his);
            }
            return resList;
        }

        void getHtml(His his, string size)
        {
            string imgUrl = "http://www.141jav.com/movies/" + imageRex.Match(his.OriginalHtml).Value.Replace("movies/", "");
            
            his.Html = "<img src=\"" + imgUrl + "\"/><br>" + his.Vid + "<br>" + size + "<br>" + "<br>" + his.Actress + "<br>";
            his.Html += "<a href=\"http://www.141jav.com/file.php?n=" + his.Vid + "&q=torrage\">torrage</a><br>";
            his.Html += "<a href=\"http://www.141jav.com/file.php?n=" + his.Vid + "&q=ws\">ws</a><br>";
            his.Html += "<a href=\"http://www.141jav.com/file.php?n=" + his.Vid + "&q=zoink\">zoink</a><br>";
            his.Html += "<a href=\"http://www.141jav.com/file.php?n=" + his.Vid + "&q=torcahe\">torcahe</a><br>";

        }
    }
}
