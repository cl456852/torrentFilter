using System;
using System.Collections.Generic;
using DB;
using MODEL;

namespace BLL
{
    public abstract class FilterBase
    {
        Dictionary<string, HisTorrent> dicHis = new Dictionary<string, HisTorrent>();
        Dictionary<string, MyFileInfo> dicFile = new Dictionary<string, MyFileInfo>();
        Dictionary<string, RarbgTitle> dicRarbgTitle = new Dictionary<string, RarbgTitle>();
        Dictionary<string, RarbgTitle> dicSelf = new Dictionary<string, RarbgTitle>();

        public void Process(String path)
        {
            Init();
            List<RarbgTitle> list = GetList(path);
            foreach (RarbgTitle rarbgTitle in list)
            {
                CheckSelf(rarbgTitle);
            }

            foreach (var item in dicSelf)
            {


                if (!CheckFile(item.Value))
                {
                    continue;
                }

                if (!CheckHis(item.Value))
                {
                    continue;
                }

                if (!CheckRarbgTitle(item.Value))
                {
                    continue;
                }
                Console.WriteLine("OK   " + item.Value.Name);
                if (!String.IsNullOrEmpty(item.Value.Maglink)) 
                    Console.WriteLine( Tool.Cmd("python main.py \"" + item.Value.Maglink+"\""));
                DBHelper.InsertRarbgTitle( item.Value);
            }


            
        }

        protected void Init()
        {
            List<MyFileInfo> list = DBHelper.GetAllFile();
            foreach (MyFileInfo myFileInfo in list)
            {
                string name = Tool.FilterFilePre(myFileInfo.FileName);
                string dir = Tool.FilterFilePre(myFileInfo.Directory);
                if (!dicFile.ContainsKey(dir))
                {
                    dicFile.Add(dir, myFileInfo);
                }
                else if (dicFile[dir].Length < myFileInfo.Length)
                {
                    dicFile[dir] = myFileInfo;
                }

                if (!dicFile.ContainsKey(name))
                {
                    dicFile.Add(name, myFileInfo);
                }
                else if (dicFile[name].Length < myFileInfo.Length)
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
                    dicHis.Add(name, hisTorrent);
                }
                else if (dicHis[name].Size < hisTorrent.Size)
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

        private void CheckSelf(RarbgTitle rarbgTitle)
        {
            if(rarbgTitle.Path.Contains("rq.mp4"))
            {
                Tool.moveFile("SD", rarbgTitle.Path);
                return;
            }
            if (!dicSelf.ContainsKey(rarbgTitle.FilteredName))
            {
                dicSelf.Add(rarbgTitle.FilteredName, rarbgTitle);
            }
            else if (dicSelf[rarbgTitle.FilteredName].Size < rarbgTitle.Size)
            {
                if(!String.IsNullOrEmpty(dicSelf[rarbgTitle.FilteredName].Path))
                    Tool.moveFile("selfDuplicate", dicSelf[rarbgTitle.FilteredName].Path);
                dicSelf[rarbgTitle.FilteredName] = rarbgTitle;
            }
            else
            {
                if(!String.IsNullOrEmpty(rarbgTitle.Path))
                    Tool.moveFile("selfDuplicate", rarbgTitle.Path);
            }
        }

        private bool CheckFile(RarbgTitle rarbgTitle)
        {
            if (!dicFile.ContainsKey(rarbgTitle.FilteredName))
            {
                return true;
            }
            else if (dicFile[rarbgTitle.FilteredName].Length * 1.3 < rarbgTitle.Size)
            {
                return true;
            }
            if(!String.IsNullOrEmpty(rarbgTitle.Path))
                Tool.moveFile("fileDuplicate", rarbgTitle.Path);
            return false;
        }

        private bool CheckHis(RarbgTitle rarbgTitle)
        {
            if (!dicHis.ContainsKey(rarbgTitle.FilteredName))
            {
                return true;
            }
            else if (dicHis[rarbgTitle.FilteredName].Size * 1.3 < rarbgTitle.Size)
            {
                return true;
            }
            if(!String.IsNullOrEmpty(rarbgTitle.Path))
                Tool.moveFile("hisDuplicate", rarbgTitle.Path);
            return false;
        }

        private bool CheckRarbgTitle(RarbgTitle rarbgTitle)
        {
            if (!dicRarbgTitle.ContainsKey(rarbgTitle.FilteredName))
            {
                return true;
            }
            else if (dicRarbgTitle[rarbgTitle.FilteredName].Size * 1.3 < rarbgTitle.Size)
            {
                return true;
            }
            if(!String.IsNullOrEmpty(rarbgTitle.Path))
                Tool.moveFile("rarbgTitleDuplicate", rarbgTitle.Path);
            return false;
        }

        public abstract List<RarbgTitle> GetList(string directoryStr);
    }
}