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
using DB;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UI1;

namespace UI1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Regex sizeRegex = new Regex("Size:<\\/td><td class=\"lista\">.*?<\\/td>");
        Regex titleRegex = new Regex("class=\"black\">.*?<\\/h1>");
        Regex magRegex = new Regex("magnet:.*?\"");
        Dictionary<string, RarbgTitle> titleDic;
        Dictionary<string, RarbgTitle> titleDicNew=new Dictionary<string, RarbgTitle>();

        FileBLL fb = new FileBLL();
        private void Form1_Load(object sender, EventArgs e)
        {
            Console.SetOut(new ConsoleWriter(this)); 
            Dictionary<string, string> items = new Dictionary<string, string>
            {
                { "Xxxclub", "XXXclub" },
                { "PornoMagnet", "PornoMagnet" },
                { "TorrentFilter", "TorrentFilter" },
                { "Btdig", "Btdig" }
                
            };

            comboBox1.DataSource = new BindingSource(items, null);
            comboBox1.DisplayMember = "Value";  // 显示值
            comboBox1.ValueMember = "Key";      // 存储的键
        }

        private void Insert_Click(object sender, EventArgs e)
        {
            fb.process(textBox1.Text,true);

        }
        
        string dataClicked="" ;
        bool flag = false;
        
        
        private void button4_Click(object sender, EventArgs e)
        {
            UnknownHtmlGenerator unknownHtmlGenerator = new UnknownHtmlGenerator();
            unknownHtmlGenerator.Process(textBox1.Text);
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
        public void AppendLog(string message)
        {
            if (textBoxLog.InvokeRequired)
            {
                textBoxLog.Invoke(new Action(() => textBoxLog.AppendText(message + Environment.NewLine)));
            }
            else
            {
                textBoxLog.AppendText(message + Environment.NewLine);
            }
        }

        private async void button1_Click_2(object sender, EventArgs e)
        {
            FilterBase filter;
            
            try
            {
                button1.Enabled = false;
                Type type = Type.GetType("BLL."+comboBox1.SelectedValue+", BLL");
                filter= (FilterBase)Activator.CreateInstance(type);
                
                await Task.Run(() => filter.Process(textBox1.Text));
            }            
            catch (Exception ex)
            {
                AppendLog("发生错误: " + ex.Message);
            }
            finally
            {
                button1.Enabled = true;
            }
        }
    }
}

public class ConsoleWriter : TextWriter
{
    private readonly Form1 form;

    public ConsoleWriter(Form1 form)
    {
        this.form = form;
    }

    public override void WriteLine(string value)
    {
        form.AppendLog(value);
    }

    public override void Write(string value)
    {
        form.AppendLog(value);
    }

    public override Encoding Encoding => Encoding.UTF8;
}


