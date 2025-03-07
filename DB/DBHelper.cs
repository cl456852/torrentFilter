using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using MODEL;
namespace DB
{
    public class DBHelper
    {
        static string seachHisSql = "select * from his1 where LOWER(vid)=LOWER('{0}') and ABS((size-{1})/{1})<0.05 ";
        static string insertHisSql = "insert into his1 values('{0}',{1},'{2}',{3},'{4}',getdate())";
        static string checkTorrentSql = "select count(*) from his where LOWER([file])='{0}' and size> 104857600";
        static string insertTorrentSql = "insert into his values('{0}',{1},'{2}',getdate(),'{3}','{4}')";
        static string insertRarbgTitleSql = "insert into rarbgTitle values('{0}',{1}, '{2}','{3}', GETDATE())";
        public static string connstr = @"server=DESKTOP-OO2BIU5;uid=sa;pwd=iamjack'scolon;database=cd";
        static string checkFilesSql = "select count(*) from files where filename='{0}' and length>60";

        static string checkUnknownTorrentsSql = "select count(*) from files where directory like '{0}%'";
        public static SqlConnection conn = new SqlConnection(connstr);
        static string checkFilesSql1 = "  select * from his where ";
        public static SqlDataReader SearchSql(string sql)
        {
            conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql, conn);
            SqlDataReader sda = cmd.ExecuteReader();

            //conn.Close();
            
            return sda;
        }

        public static void insertTorrent(HisTorrent his)
        {
            string sql = string.Format(insertTorrentSql, his.File.Replace("'","''"), his.Size, his.Path.Replace("'","''"), his.Ext,his.Md5 );
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                sc.ExecuteNonQuery();
            }
        }
        public static int checkUnknownTorrents(string s)
        {
            int res = 0;
            string sql = string.Format(checkUnknownTorrentsSql,s);
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                res = Convert.ToInt32(sc.ExecuteScalar());
            }
            return res;
        }

        public static Dictionary<string, HisTorrent> getList(string filterStr)
        {
            string[] strs = filterStr.Split(',');

            string filterStr1 = " REPLACE(LOWER([file]),'" + strs[0] + "','') ";
            for (int i = 1; i < strs.Length; i++)
            {
         
                filterStr1 = "REPLACE(" + filterStr1 + ",'" + strs[i] + "','') ";
            }
            Dictionary<string, HisTorrent> dic = new Dictionary<string, HisTorrent>();

            string sql = "select " + filterStr1 + " as [file] ,createtime,[path],[size],ext,md5 from his where size>100*1024*1024 and DATEDIFF(M,createtime,GETDATE())<=500 order by createtime";
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                sc.CommandTimeout = 1800;  
                SqlDataReader reader = sc.ExecuteReader();
                while (reader.Read())
                {
                    HisTorrent t = new HisTorrent();
                    t.File = reader["file"].ToString();
                    t.CreateTime = Convert.ToDateTime(reader["createtime"]);
                    t.Ext = reader["ext"].ToString();
                    t.Path = reader["path"].ToString();
                    t.Size = Convert.ToInt64(reader["size"].ToString());
                    t.Md5 = reader["md5"].ToString();
                    try
                    {
                        dic.Add(t.File, t);
                    }
                    catch (ArgumentException e)
                    {
                        dic.Remove(t.File);
                        dic.Add(t.File, t);
                    }
                }
                return dic;

            }
        }

        public static Dictionary<string, HisTorrent> getFileList(string filterStr)
        {
            Dictionary<string, HisTorrent> dic = new Dictionary<string, HisTorrent>();
            string sql = "select fileName from files where length>70";
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                SqlDataReader reader = sc.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        string fileName=filterName(reader[0].ToString(),filterStr);
                        dic.Add(fileName, new HisTorrent());
                    }
                    catch (ArgumentException e)
                    {
                    }
                }
                return dic;

            }
        }

        static string filterName(string fileName, string filterString)
        {
            fileName = fileName.ToLower();
            if (fileName.LastIndexOf('.') > 0)
            {
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            string[] strs = filterString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in strs)
            {
                fileName = fileName.Replace(s, "");
            }
            return fileName;
        }
        
        public static Dictionary<string, RarbgTitle> getFilteredRagbgTitle(string filterStr)
        {
            Dictionary<string, RarbgTitle> dic = new Dictionary<string, RarbgTitle>();
            string sql = "select * from rarbgTitle";
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                SqlDataReader reader = sc.ExecuteReader();
                while (reader.Read())
                {
                 
                    float size = (float)Convert.ToDouble(reader["size"]);
                    string name = reader["name"].ToString();
                    string magLink = reader["magLink"].ToString();
                    RarbgTitle rarbgTitle = new RarbgTitle(Convert.ToInt32 (reader["id"].ToString()),name, magLink, size);
                    if(!dic.ContainsKey(rarbgTitle.FilteredName))
                    { 
                        dic.Add(rarbgTitle.FilteredName, rarbgTitle);
                    }
                    else
                    {
                        if(dic[rarbgTitle.FilteredName].Size<rarbgTitle.Size)
                            dic[rarbgTitle.FilteredName] = rarbgTitle;
                    }

                    


                }
                return dic;

            }
        }

        public static void InsertRarbgTitle( RarbgTitle rarbgTitle)
        {
            string sql = string.Format(insertRarbgTitleSql, rarbgTitle.Name.Replace("'","''"), rarbgTitle.Size,rarbgTitle.Maglink.Replace("'","''"), rarbgTitle.Path.Replace("'","''") );
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand sc = new SqlCommand(sql, conn);
                sc.ExecuteNonQuery();
            }
        }
        
        
        public static List<MyFileInfo> GetAllFile()
        {
            string sql = "select fileName, length, directory from files where (len(fileName)>35 or len(directory)>35) and length>50";
            List<MyFileInfo> myFileInfoList=new List<MyFileInfo>();
            SqlDataReader sdr= DBHelper.SearchSql(sql);

            while (sdr.Read())
            {
                MyFileInfo myFileInfo = new MyFileInfo();
                myFileInfo.FileName = sdr["fileName"].ToString();
                myFileInfo.Length = Convert.ToDouble(sdr["length"]);
                myFileInfo.Directory = sdr["directory"].ToString();
                myFileInfoList.Add(myFileInfo);
            }
            sdr.Close();
            return myFileInfoList;
        }

        public static List<HisTorrent> GetAllHis()
        {
            string sql = "select [file], size/1024/2024 as size from his where len([file])>35 and size>50";

            List<HisTorrent> hisList = new List<HisTorrent>();
            SqlDataReader sdr= DBHelper.SearchSql(sql);

            while (sdr.Read())
            {
                HisTorrent hisTorrent = new HisTorrent();
                hisTorrent.Size = Convert.ToDouble(sdr["size"]);
                hisTorrent.File = sdr["file"].ToString();
                hisList.Add(hisTorrent);
            }
            sdr.Close();
            return hisList;
        }

        public static List<RarbgTitle> GetAllRarbgTitle()
        {
            string sql = "select name, size from rarbgTitle";
            List<RarbgTitle> list = new List<RarbgTitle>();
            SqlDataReader sdr= DBHelper.SearchSql(sql);
            while (sdr.Read())
            {
                RarbgTitle rarbgTitle = new RarbgTitle();
                rarbgTitle.Name = sdr["name"].ToString();
                rarbgTitle.Size = (float)Convert.ToDouble(sdr["size"]);
                list.Add(rarbgTitle);
            }

            return list;
        }
    }
}
