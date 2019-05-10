using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace JsonTools
{
    public partial class Form7 : Form
    {
        List<string> VerInfo = new List<string>();
        Dictionary<string, string> DateInfo = new Dictionary<string, string>();
        Dictionary<string, string[]> DicVerInfo = new Dictionary<string, string[]>();
        public Form7()
        {
            InitializeComponent();
        }
        //副程式
        private void Init()
        {
            VerInfo = LoadVerInfo();
            SetVerInfo(VerInfo);
            GetVerInfo();
        }
        private void SetVerInfo(List<string> xValue)
        {
            string ver = "";
            List<string> tmp = new List<string>();
            for (int i = 0; i < xValue.Count;i++ )
            {
                string[] sp = Split("\r\n", xValue[i]);
                for (int j = 0; j < sp.Length; )
                {
                    if (sp[j].StartsWith("##"))
                    {
                        tmp.Clear();
                        string[] spt = Split("##", sp[j]);
                        cbo_Ver.Items.Add(spt[0]);
                        ver = spt[0];
                        j++;
                    }
                    else if (sp[j].StartsWith("**"))
                    {
                        string[] spt = Split("**", sp[j]);
                        DateInfo.Add(ver,spt[0]);
                        j++;
                    }
                    else 
                    {
                        tmp.Add(sp[j]);
                        j++;
                        if (j==sp.Length)
                        {
                            string[] aa = tmp.ToArray();
                            DicVerInfo.Add(ver, aa);
                            ver = "";          
                        }
                    }
                }
            }
        }
        private void GetVerInfo()
        {
            if (cbo_Ver.Items.Count > 0 )
            {
                if(cbo_Ver.SelectedIndex == -1)
                {
                    cbo_Ver.SelectedIndex = cbo_Ver.Items.Count-1;
                }
                string s = cbo_Ver.SelectedItem.ToString();
                tb_Date.Text = DateInfo[s];
                rtb01.Lines = DicVerInfo[s];

            }
            rtb01.Focus();
        }
        //--------------------------------------------------
        private void Form7_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void cbo_Ver_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetVerInfo();
        }

        public static List<string> LoadVerInfo()
        {
            List<string> tmp = new List<string>();
            string[] sr = Split("##",Properties.Resources.JSonInfo );
            for (int i = 0; i < sr.Length; i++)
            {
                tmp.Add("##" + sr[i]);
            }
            return tmp;
        }
        public static string[] Split(string separator, string Text)
        {

            string[] sp = new string[] { separator };

            return Text.Split(sp, StringSplitOptions.RemoveEmptyEntries);

        }
    }
}
