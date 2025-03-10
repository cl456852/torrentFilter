﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections;
using MODEL;
using System.Text.RegularExpressions;
using BencodeLibrary;
using DB;
using System.Security.Cryptography;

namespace BLL
{
    public class FileBLL
    {
        
        Dictionary<string, HisTorrent> dic;
        Dictionary<string, HisTorrent> fileDic;
        HashSet<String> md5Set = new HashSet<string>();
        Dictionary<string,HisTorrent> torrentNameDic = new Dictionary<string,HisTorrent>();
        DateTime startTime;
        

        public void process(string directoryStr,bool ifCheckHis)
        {
            string md5;
            if (md5Set.Count == 0)
            {
                getList();
            }
            startTime = DateTime.Now;
            String[] path = Directory.GetFiles(directoryStr, "*", SearchOption.TopDirectoryOnly);
            foreach (String p in path)
            {
                if (p.EndsWith(".torrent"))
                {
                    if (Path.GetFileNameWithoutExtension(p).ToLower().Contains(".sd."))
                    {
                        Tool.moveFile("SD", p); 
                        continue;
                    }
                    md5 = GetMd5(p);
                    if (md5Set.Contains(md5))
                    {
                        Tool.moveFile("Md5Duplicate",p);
                        continue;
                    }
                    if (!Path.GetFileNameWithoutExtension(p).Replace("rarbg.to", "").Contains("."))
                    {
                        Tool.moveFile("invalid", p);
                    }
                    BDict torrentFile = null;
                    bool hasBigFile = false;
                    try
                    {
                        torrentFile = BencodingUtils.DecodeFile(p) as BDict;
                    }
                    catch (Exception e)
                    {
                        Tool.moveFile("decodeErr", p);
                    }
                    if (torrentFile != null)
                    {
                        bool flag = true;
                        BList b;
                        List<HisTorrent> listTorrent = new List<HisTorrent>();
                        if ((torrentFile["info"] as BDict).ContainsKey("files"))
                        {

                            b = (BList)(torrentFile["info"] as BDict)["files"];



                            for (int i = 0; i < b.Count; i++)
                            {
                                BDict bd = (BDict)b[i];
                                long length = ((BInt)bd["length"]).Value;
                                if (length > 25 * 1024 * 1024)
                                    hasBigFile = true;
                                BList list = (BList)bd["path"];
                                string s = ((BString)list[list.Count - 1]).Value;
                                HisTorrent trt = new HisTorrent();
                                trt.CreateTime = DateTime.Now;
                                trt.Path = p;
                                trt.Size = length;
                                if (s.LastIndexOf('.') > 0)
                                {
                                    trt.File = s.Substring(0, s.LastIndexOf('.'));
                                    trt.Ext = s.Substring(s.LastIndexOf('.'));
                                }
                                else
                                {
                                    trt.File = s;
                                }
                                listTorrent.Add(trt);
                                if (!check(trt,ifCheckHis))
                                {
                                    flag = false;
                                    break;
                                }

                            }
                        }
                        else
                        {
                            hasBigFile = true;
                            HisTorrent trt = new HisTorrent();
                            trt.CreateTime = DateTime.Now;
                            trt.Path = p;
                            trt.Size = ((BInt)(torrentFile["info"] as BDict)["length"]).Value;
                            string name = ((BString)(torrentFile["info"] as BDict)["name"]).Value;
                            if (name.LastIndexOf('.') > 0)
                            {
                                trt.File = name.Substring(0, name.LastIndexOf('.'));
                                trt.Ext = name.Substring(name.LastIndexOf('.'));
                            }
                            else
                            {
                                trt.File = name;
                            }
                            listTorrent.Add(trt);
                            if (!check(trt,ifCheckHis))
                            {
                                flag = false;
                            }
                        }
                        if (!hasBigFile)
                        {
                            Tool.moveFile("noBigFile", p);
                        }
                        if (flag && hasBigFile)
                        {
                            foreach (HisTorrent his in listTorrent)
                            {
                                if (his.Size > 60 * 1024 * 1024)
                                {
                                    if (ifCheckHis)
                                    {
                                        his.Md5 = md5;
                                        DBHelper.insertTorrent(his);
                                    }
                                    try
                                    {
                                        md5Set.Add(md5);
                                        dic.Add(Tool.filterName(his.File), his);
                                        torrentNameDic.Add(his.FilteredFileName, his);
                                    }catch(ArgumentException e)
                                    {
                                        dic.Remove(Tool.filterName(his.File));
                                        dic.Add(Tool.filterName(his.File), his);
                                        torrentNameDic.Remove(his.FilteredFileName);
                                        torrentNameDic.Add(his.FilteredFileName, his);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Tool.moveFile("decodeErr", p);
                    }
                }

            }




        }

        bool check(HisTorrent trt, bool ifCheckHis)
        {
            bool flag = true;
            if (trt.Size > 15 * 1024 * 1024&&!trt.File.ToLower().Contains("sample"))
            {
                if (fileDic.ContainsKey(Tool.filterName( trt.File)) )
                {
                    Tool.moveFile("duplicate", trt.Path);
                    return false;
                }
                if (ifCheckHis)
                {
                    HisTorrent t;
                    try
                    {
                        t = dic[Tool.filterName(trt.File)];

                    }
                    catch (KeyNotFoundException e)
                    {
                        try
                        {
                            t = torrentNameDic[trt.FilteredFileName];
                        }
                        catch (KeyNotFoundException ex)
                        {
                            t = null;
                        }
                    }
                    if (t != null)
                    {

                        if (t.Size >= trt.Size || t.CreateTime < startTime)
                        {
                            Tool.moveFile("duplicate", trt.Path);
                            flag = false;
                        }
                        else
                        {
                            Tool.moveFile("duplicate", t.Path);
                        }
                    }
                    return flag;
                }
                else
                    return true;
            }
            else
                return true;
        }




        void getList()
        {
            dic = DBHelper.getList(Tool.filterString);
            fileDic = DBHelper.getFileList(Tool.filterString);
            foreach (HisTorrent his in dic.Values)
            {
                if(his.Md5!=null)
                    md5Set.Add(his.Md5);
                try
                {
                    torrentNameDic.Add(his.FilteredFileName, his);
                }
                catch
                {
                    if (his.Size > torrentNameDic[his.FilteredFileName].Size)
                    {
                        torrentNameDic.Remove(his.FilteredFileName);
                        torrentNameDic.Add(his.FilteredFileName, his);
                    }
                }
            }
        }



        public static string GetMd5(string pathName)
        {
            string strResult = "";
            string strHashData = "";
            byte[] arrbytHashValue;

            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();

            try
            {
                oFileStream = new System.IO.FileStream(pathName.Replace("\"", ""), System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream); //计算指定Stream 对象的哈希值

                oFileStream.Close();

                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”

                strHashData = System.BitConverter.ToString(arrbytHashValue);

                //替换-
                strHashData = strHashData.Replace("-", "");

                strResult = strHashData;
            }

            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return strResult;
        }

    }
}

