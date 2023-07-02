using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static string Cmd(string cmd)
        {
            //string cmd = "python main.py \"magnet:?xt=urn:btih:4dc36dab7989ebe8ccbf1dc7b31595b2592d5162&dn=Moonstalker.1989.BRRip.x264-ION10&tr=http://tracker.trackerfix.com:80/announce&tr=udp://9.rarbg.me:2990&tr=udp://9.rarbg.to:2730&tr=udp://tracker.slowcheetah.org:14710&tr=udp://tracker.tallpenguin.org:15740\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {cmd}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        public static string FilterFilePre(string name)
        {
            name = name.ToLower().Replace(".", "").Replace("-", "").Replace("_", "").Replace(" ", "");
            if (name.Length >= 25)
            {
                name = name.Substring(0, 25);
            }

            return name;

        }

        public static string ReadFile(string path)
        {
            StreamReader sr = new StreamReader(path);
            string s= sr.ReadToEnd();
            sr.Close();
            return s;
        }
    }
}
