using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MODEL;
using BLL;
using DAL;
using DB;
using System.IO;
using System.Text.RegularExpressions;

namespace UI1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<MyFileInfo> list = new List<MyFileInfo>();

        Regex sizeRegex = new Regex("Size:<\\/td><td class=\"lista\">.*?<\\/td>");
        Regex titleRegex = new Regex("class=\"black\">.*?<\\/h1>");
        Regex magRegex = new Regex("magnet:.*?\"");
        Dictionary<string, RarbgTitle> titleDic;
        Dictionary<string, RarbgTitle> titleDicNew=new Dictionary<string, RarbgTitle>();

        FileBLL fb = new FileBLL();
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedText = "PornoMagnet";   
        }

        private void Insert_Click(object sender, EventArgs e)
        {
            fb.process(textBox1.Text,true);

        }

        private void button1_Click(object sender, EventArgs e)
        {

            using (OpenFolderDialog openFolderDlg = new OpenFolderDialog())
            {
                if (openFolderDlg.ShowDialog() == DialogResult.OK)
                {
                    this.textBox1.Text = openFolderDlg.Path;
                    Console.WriteLine(this.textBox1.Text.Replace("\\","\\\\"));
                }
            }
        }
        string dataClicked="" ;
        bool flag = false;


        private void button3_Click(object sender, EventArgs e)
        {
            DirectoryInfo theFolder = new DirectoryInfo(textBox1.Text);
            foreach (FileInfo file in theFolder.GetFiles())
            {
                string s = "[InternetShortcut]\nURL=http://rarbg.com/torrents.php?search=" + file.Name.ToLower().Replace(".torrent", "").Replace("[rarbg.com]", "") + "&category%5B%5D=4";
                SaveFile(s, file.FullName .Replace(".torrent", ".url"));
            }
        }

        public void SaveFile(string content, string fileName)
        {
            //实例化一个文件流--->与写入文件相关联
            FileStream fs = new FileStream(fileName, FileMode.Create);
            //实例化一个StreamWriter-->与fs相关联
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(content);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
           // fb.process(textBox1.Text, false);
            UnknownHtmlGenerator unknownHtmlGenerator = new UnknownHtmlGenerator();
            unknownHtmlGenerator.Process(textBox1.Text);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(titleDic==null)
                titleDic= DBHelper.getFilteredRagbgTitle(Tool.filterString);


            DirectoryInfo theFolder = new DirectoryInfo(textBox1.Text);
            foreach (FileInfo file in theFolder.GetFiles())
            {
                if(file.Name.StartsWith("https"))
                { continue; }

                StreamReader sr = new StreamReader(file.FullName);
                string content = sr.ReadToEnd();
                sr.Close();
                string name = titleRegex.Match(content).Value.Replace("class=\"black\">", "").Replace("</h1>", "");

                

                string sizeStr =  sizeRegex.Match(content).Value.Replace("Size:</td><td class=\"lista\">","").Replace("</td>","");
                float size = 0;
                if (sizeStr.EndsWith("GB"))
                {
                    size= (float)Convert.ToDouble( sizeStr.Replace("GB", ""))*1024;
                }
                else
                {
                    size = (float)Convert.ToDouble(sizeStr.Replace("MB", ""));
                }

                RarbgTitle rarbgTitle = new RarbgTitle();
                rarbgTitle.Name = name;
                rarbgTitle.Size = size;
                rarbgTitle.Path = file.FullName;
                rarbgTitle.Maglink = magRegex.Match(content).Value.Replace("\"", "").Replace("amp;","");

                //if (!RarbgTitleCheck(rarbgTitile))
                //{
                //    Console.WriteLine("Dup  " + rarbgTitile.Name);
                //    continue;
                //}
                if(!SelfCheck(rarbgTitle))
                {
                    Console.WriteLine("Dup Self  " + rarbgTitle.Name);
                    continue;
                }

                Console.WriteLine("OK   " + rarbgTitle.Name);
                

            }
            foreach(var item in titleDicNew)
            {
                Tool.moveFile("OK", Path.Combine(textBox1.Text, item.Value.Path));
                Console.WriteLine( Tool.Cmd("python main.py " + item.Value.Maglink));
                DBHelper.InsertRarbgTitle( item.Value);
            }

        }

        bool SelfCheck(RarbgTitle rarbgTitle)
        {
            if(!titleDicNew.ContainsKey(rarbgTitle.FilteredName))
            {
                titleDicNew.Add(rarbgTitle.FilteredName, rarbgTitle);
                return true;
            }
            else if(titleDicNew[rarbgTitle.FilteredName].Size<rarbgTitle.Size)
            {
                titleDicNew[rarbgTitle.FilteredName] = rarbgTitle;
                return true;
            }
            return false;

        }

        bool RarbgTitleCheck(RarbgTitle rarbgTitle)
        {
            if(!titleDic.ContainsKey(rarbgTitle.FilteredName))
            {
                return true;
            }

            if(titleDic[rarbgTitle.FilteredName].Size<rarbgTitle.Size)
            {
                return true;
            }

            return false;
        }






        

        private void button1_Click_2(object sender, EventArgs e)
        {
            FilterBase filter=null;
            if(comboBox1.Text=="PornoMagnet")
            { 
                filter = new PornoMagnet();
            } else if(comboBox1.Text=="Btdig")
            {
                filter = new Btdig();
            }
            else if(comboBox1.Text=="torrent")
            {
                filter = new TorrentFilter();
            }
            else if(comboBox1.Text=="XXXClub")
            {
                filter = new Xxxclub();
            }

            
            filter.Process(textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            IUnknown unknown= null;
            if(comboBox1.Text=="PornoMagnet")
            {
                unknown=new PornoMagnetUnknown();
            }
            else if(comboBox1.Text=="XXXClub")
            {
                unknown = new XxxClubUnknown();
            }

            unknown.Process(textBox1.Text);

        }
    }
}
