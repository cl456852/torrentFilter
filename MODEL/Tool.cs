﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BencodeLibrary;

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
                    File.Move(path , Path.Combine(targetDir, Path.GetFileName(path)).Substring(0, 240) + ".htm");
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
            if (name.Length >= 30)
            {
                name = name.Substring(0, 30);
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

        public static List<RarbgTitle> DecodeTorrent(string p)
        {
            List<RarbgTitle> rarbgTitles = new List<RarbgTitle>();
            bool hasBigFile = false;
            BDict torrentFile = null;
            try
            {
                torrentFile = BencodingUtils.DecodeFile(p) as BDict;
            }
            catch (Exception e)
            {
                Tool.moveFile("decodeErr", p);
                return rarbgTitles;
            }
            BList b;
            b = (BList)(torrentFile["info"] as BDict)["files"];



            for (int i = 0; i < b.Count; i++)
            {
                BDict bd = (BDict)b[i];
                long length = ((BInt)bd["length"]).Value;
                float size = length / 1024 / 1024;
                if (size < 30)
                    continue;
                BList list = (BList)bd["path"];
                string s = ((BString)list[list.Count - 1]).Value;
                RarbgTitle rarbgTitle = new RarbgTitle();
                rarbgTitle.Size = length / 1024 / 1024;
                rarbgTitle.Name = s;
                rarbgTitles.Add(rarbgTitle);
            }

            return rarbgTitles;
        }
    }
}
