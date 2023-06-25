using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DAL;
using DB;
using MODEL;

namespace BLL
{


    public class PornoMagnet
    {
        Dictionary<string, HisTorrent> dicHis = new Dictionary<string, HisTorrent>();
        Dictionary<string, MyFileInfo> dicFile = new Dictionary<string, MyFileInfo>();
        private Dictionary<string, RarbgTitle> dicRarbgTitle = new Dictionary<string, RarbgTitle>();
        private Dictionary<string, RarbgTitle> dicSelf = new Dictionary<string, RarbgTitle>();

        private Regex magnetRegex=new Regex("magnet:.*?\"");
        private Regex titleRegex = new Regex("<h1>.*?<\\/h1>");
        private Regex sizeRegex = new Regex("Размер<\\/td><td>.*?<\\/td><\\/tr>");

        public void Process(string directoryStr)
        {
            Init();
            string[] paths = Directory.GetFiles(directoryStr, "*", SearchOption.TopDirectoryOnly);

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
                rarbgTitle.Maglink = magnetRegex.Match(content).Value.Replace("\"", "").Replace("amp;","").Replace("&","^&");
                
                if (sizeStr.EndsWith("GB"))
                {
                    rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("GB", "")) * 1024);
                }
                else if(sizeStr.EndsWith("MB"))
                {
                    rarbgTitle.Size = (float)(Convert.ToDouble(sizeStr.Replace("MB", "")));
                }
                checkSelf(rarbgTitle);
            }
            foreach (var item in dicSelf)
            {


                if (!checkFile(item.Value))
                {
                    continue;
                }

                if (!checkHis(item.Value))
                {
                    continue;
                }

                if (!checkRarbgTitle(item.Value))
                {
                    continue;
                }
                Console.WriteLine("OK   " + item.Value.Name);
                Console.WriteLine( Tool.Cmd("python main.py " + item.Value.Maglink));
                DBHelper.InsertRarbgTitle( item.Value);
            }
            
        }
        
        private void checkSelf(RarbgTitle rarbgTitle)
        {
            if (!dicSelf.ContainsKey(rarbgTitle.FilteredName))
            {

                dicSelf.Add(rarbgTitle.FilteredName, rarbgTitle);
            }
            else if (dicSelf[rarbgTitle.FilteredName].Size < rarbgTitle.Size)
            {
                Console.WriteLine("selfDuplicate: " + dicSelf[rarbgTitle.FilteredName].Name);
                Tool.moveFile("selfDuplicate", dicSelf[rarbgTitle.FilteredName].Path);
                dicSelf[rarbgTitle.FilteredName] = rarbgTitle;

            }
            else
            {
                Tool.moveFile("selfDuplicate", rarbgTitle.Path);
                Console.WriteLine("selfDuplicate: " + rarbgTitle.Name);
            }
        }

        private bool checkFile(RarbgTitle rarbgTitle)
        {
            if (!dicFile.ContainsKey(rarbgTitle.FilteredName))
            {
                
                return true;
            }
            else if(dicFile[rarbgTitle.FilteredName].Length*1.3<rarbgTitle.Size)
            {
                return true;
            }
            Tool.moveFile("fileDuplicate", rarbgTitle.Path);
            Console.WriteLine("Check File "+rarbgTitle.Name);
            return false;

        }

        private bool checkHis(RarbgTitle rarbgTitle)
        {
            if (!dicHis.ContainsKey(rarbgTitle.FilteredName))
            {
                
                return true;
            }
            else if(dicHis[rarbgTitle.FilteredName].Size*1.3<rarbgTitle.Size)
            {
                return true;
            }
            Tool.moveFile("hisDuplicate", rarbgTitle.Path);
            Console.WriteLine("Check His "+rarbgTitle.Name);
            return false;
        }
        
        private bool checkRarbgTitle(RarbgTitle rarbgTitle)
        {
            if (!dicRarbgTitle.ContainsKey(rarbgTitle.FilteredName))
            {
                
                return true;
            }
            else if(dicRarbgTitle[rarbgTitle.FilteredName].Size*1.3<rarbgTitle.Size)
            {
                return true;
            }
            Tool.moveFile("rarbgTitleDuplicate", rarbgTitle.Path);
            Console.WriteLine("Check RarbgTitle "+rarbgTitle.Name);
            return false;
        }
        
        
        private void Init()
        {
            List<MyFileInfo> list= DBHelper.GetAllFile();
            foreach (MyFileInfo myFileInfo in list)
            {
                string name = Tool.FilterFilePre(myFileInfo.FileName);
                string dir = Tool.FilterFilePre(myFileInfo.Directory);
                if(!dicFile.ContainsKey(dir))
                {
                    dicFile.Add(dir, myFileInfo);
                }
                else if(dicFile[dir].Length<myFileInfo.Length)
                {
                    dicFile[dir] = myFileInfo;
                }
                if (!dicFile.ContainsKey(name))
                {
                    dicFile.Add(name, myFileInfo);
                }
                else if(dicFile[name].Length<myFileInfo.Length)
                {
                    dicFile[name] = myFileInfo;
                }
            }

            List<HisTorrent> hisList = DBHelper.GetAllHis();
            foreach (HisTorrent hisTorrent in hisList)
            {
                string name = Tool.FilterFilePre(hisTorrent.File);
                if (!dicHis.ContainsKey(name))
                {
                    dicHis.Add(name,hisTorrent);
                }
                else if(dicHis[name].Size<hisTorrent.Size)
                {
                    dicHis[name] = hisTorrent;
                }
            }

            List<RarbgTitle> rarbgTitileList = DBHelper.GetAllRarbgTitle();
            foreach (RarbgTitle rarbgTitile in rarbgTitileList)
            {
                if (!dicRarbgTitle.ContainsKey(rarbgTitile.FilteredName))
                {
                    dicRarbgTitle.Add(rarbgTitile.FilteredName, rarbgTitile);
                }
                else if (dicRarbgTitle[rarbgTitile.FilteredName].Size < rarbgTitile.Size)
                {
                    dicRarbgTitle[rarbgTitile.FilteredName] = rarbgTitile;
                }
            }
        }
        
        
        
        
        
        
    }
    
    
}