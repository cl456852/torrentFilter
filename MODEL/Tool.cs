using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MODEL
{
    public class Tool
    {
        public static string filterString = "3d,2k,1k,_avc_hd,_avc,_hd,480p,720p,1080p,1440p,2160p,_,480,720,1080,2160,[,],.,2000,4000,8000,12000,6000,1500, ,540,qhd,fullhd,-,high,low,sd,$$,rarbg,com,ktr,xxx,%22,%20,YAPG,PROPER,6500,4500,3000,1200,[,],.com,mp4,4k,KLEENEX,GAGViD";
        public static string filterName(string fileName)
        {
            fileName = fileName.ToLower();
            string[] strs = Tool.filterString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in strs)
            {
                fileName = fileName.Replace(s.ToLower(), "");
            }
            return fileName;
        }

        public static void moveFile(string folderName, string path)
        {
            if (File.Exists(path))
            {
                string targetDir = Path.Combine(Path.GetDirectoryName(path), folderName);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                try
                {
                    File.Move(path, Path.Combine(targetDir, Path.GetFileName(path)));
                    if (File.Exists(path + ".htm"))
                    {
                        File.Move(path + ".htm", Path.Combine(targetDir, Path.GetFileName(path)) + ".htm");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("path too long    " + path);
                    File.Move(path, Path.Combine(targetDir, Path.GetFileName(path)).Substring(0, 240) + ".torrent");
                    File.Move(path + ".htm", Path.Combine(targetDir, Path.GetFileName(path)).Substring(0, 240) + ".htm");
                    Console.WriteLine("path too long    " + Path.Combine(targetDir, Path.GetFileName(path)).Substring(0, 240) + ".torrent");
                }
                Console.WriteLine(folderName + " " + path);
            }

        }
    }
}
