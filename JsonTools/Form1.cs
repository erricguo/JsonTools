using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Collections.Specialized;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows;
using Gma.QrCodeNet.Encoding.Windows.Render;
// 添加额外的命名空间
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace JsonTools
{

    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, uint wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern UInt32 GlobalAddAtom(String lpString);  //添加原子
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern UInt32 GlobalFindAtom(String lpString);  //查找原子
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);  //删除原子
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


 
        string nowRP02 = "";
        int tmpLen = 0;
        int index = 0;
        int tP05_RB2 = 0;
        int tP05_RB4 = 0;
        TreeNodeEx CurrentNode = null;
        public delegate void ClickEventHandler(object sender, TreeNodeMouseClickEventArgs e);
        static Computer myComputer = new Computer();
        public static string Mydocument = myComputer.FileSystem.SpecialDirectories.MyDocuments;
        public static string JsonDir = Mydocument + @"\JsonTools\";
        public static string PGNamePath = Mydocument + @"\JsonTools\PGName.ini";
        enum ary { N0100=2,N0101=10,N01011=26 }; //由左至右計算 2的N次方從0開始 N0100 = 0*1+1*2+0*4+0*8=2
        List<TextBox> tP03_tbList = new List<TextBox>();
        List<DataGridView> tP03_dgvList = new List<DataGridView>();
        List<TextBox> tP04_tbList = new List<TextBox>();
        List<DataGridView> tP04_dgvList = new List<DataGridView>();
        //20140218
        // 服务器端口
        int port;
        // 监听端口
        int tcpPort;
        // 定义变量
        private UdpClient receiveUdpClient;
        private IPEndPoint serverIPEndPoint;
        private IPAddress serverIp;
        private TreeNodeEx FoundNode = null;
        public static Dictionary<string, string> jsonKeyValues = new Dictionary<string, string>();
        int MsgCount = 0;
        int MsgDone = 0;
        //20140218
        //20140225
        public static JObject Pool = new JObject();
        int count = 0;
        int Mmax = 0;
        private TcpListener tcpListener;
        private Thread listenThread;

        MemoryStream Fstream = null;
        private static string JsonTreeStr = "";
        private static bool IsCanUpdate = false;
        private static bool IsLogToTxt = false;
        private static int LogLines = 1;
        //20140225
        public Form1()
        {
            InitializeComponent();
        }
        public static bool isDirectory(string p)//目錄是否存在
        {
            if (p == "")
            {
                return false;
            }
            return System.IO.Directory.Exists(p);
        }
        TreeNodeEx MyNode = new TreeNodeEx();
        string ss = "";
        //20140325
        int DGV03_SelectIndex = -1;
        private void Form1_Load(object sender, EventArgs e)
        {
            
            //f2.Show();
            GoRead();
            lbl_Msg.Text = "";
            this.Text = "JSonTools Ver " + GetVersion() + " @ ErricGuo ";

            if (tb_TXT.Text != "")
            {
                if (!File.Exists(tb_TXT.Text))
                {
                    MessageBox.Show("檔案不存在!!", "錯誤");
                    return;
                }
                GoRegisty("Path",tb_TXT.Text);
                btn_Load.PerformClick();
            }
            cboChanged(cbo_Name);
            cboChanged(cbo_Value);
            cboChanged(cbo_Path);
            cboChanged(chk_CheckNow);
            if (cbo_Big5.SelectedIndex == -1)
            {
                cbo_Big5.SelectedIndex = 0;
            }
            if (cbo_Sym.SelectedIndex == -1)
            {
                cbo_Sym.SelectedIndex = 3;
            }
            cboSelectChanged(cbo_Big5);
            cboSelectChanged(cbo_Sym);
            tP06_Lb01.Text = "";

            if (tP06_tbPath.Text != "")
            {
                GoRegisty("tP06_tbPath", tP06_tbPath.Text);
            }
            //TP07
            LoadPGSections();
            ThreadPool.SetMaxThreads(2, 2);
            timer_AutoUpdate.Interval = Int32.Parse(tb_Seconds.Text) *1000;
            timer_AutoUpdate.Enabled = true;
            tcpListener = new TcpListener(IPAddress.Any, 4097);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();

        }
        public static string GetVersion()
        {
            string s = "";
            try
            {
                s += System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (Exception)
            {
                s = "開發程式階段";
            }
            return s;
        }

        string nodeName = string.Empty;

        private bool GetJsonTree(string xStr, TreeNodeEx xNode, int xType)
        {
            Hashtable ht = new Hashtable();
            Hashtable ht2 = new Hashtable();
            ArrayList al = new ArrayList();
            ArrayList al2 = new ArrayList();
            SortedDictionary<string, string> sdc = null;
            JArray ja = new JArray();
            string tmp2 = "";
            if (xType == 1)
            {
                /*if (xStr.StartsWith("[") && xStr.EndsWith("]"))
                {
                    tmp2 = "[" + xStr + "]";
                }
                else*/
                {
                    tmp2 = xStr;
                }
                try
                {
                    ja = JArray.Parse(tmp2);
                }
                catch
                {

                }
                TreeNodeEx Nodeay = null;
                for (int i = 0; i < ja.Count();i++ )
                {
                    sdc = new SortedDictionary<string, string>();
                    ht = JavaScriptConvert.DeserializeObject(ja[i].ToString(), typeof(Hashtable)) as Hashtable;

                    foreach (DictionaryEntry de in ht)
                    {
                        //al.Add(de.Key.ToString());
                        //al2.Add(de.Value.ToString());
                        sdc.Add(de.Key.ToString(), de.Value.ToString());
                    }
                    ht.Clear();
                    

                    Nodeay = new TreeNodeEx();
                    Nodeay.Text = "["+i.ToString()+"]"; //節點文字
                    Nodeay.value = i.ToString(); //節點文字    
                    Nodeay.Tag = "";
                    //Nodeay.path = "";// xNode.path + ",'" + xNode.nodename + "'";
                    //Nodeay.path =  xNode.path + ",'" + xNode.nodename + "'";
                    Nodeay.path = xNode.path ;
                    //Nodeay.nodename = xNode.nodename+Nodeay.Text;
                    Nodeay.Name = Nodeay.Text;
                    Nodeay.nodename =  Nodeay.Text;
                    SetNodeSelectPath(Nodeay);
                    Nodeay.Tsdc = sdc;
                    Nodeay.index = index++;
                    xNode.Nodes.Add(Nodeay);

                    //if (ht != null)
                    if (sdc != null)
                    {
                        //foreach (DictionaryEntry de in ht)
                        foreach (KeyValuePair<string, string> de in sdc)
                        {
                            TreeNodeEx Node = new TreeNodeEx();
                            Node.Text = de.Key.ToString(); //節點文字
                            Node.value = de.Key.ToString(); //節點文字
                            //Node.path = xNode.path + ",'" + xNode.nodename + "'";
                            //Node.path = xNode.path + ",'" + Nodeay.nodename + "'";
                            Node.path = Nodeay.path;
                            //Node.nodename = xNode.nodename + Nodeay.nodename;
                            Node.Name = Node.Text;
                            Node.nodename = Node.Text;
                            SetNodeSelectPath(Node);
                            string tmp = de.Value.ToString().Trim();
                            if (tmp.EndsWith("}") && tmp.StartsWith("{"))
                            {
                                Node.index = index++;
                                Nodeay.Nodes.Add(Node);
                                Node.Tag = "";
                                GetJsonTree(de.Value.ToString(), Node, 0);
                            }
                            else if (tmp.EndsWith("]") && tmp.StartsWith("["))
                            {
                                string tmps = tmp.Substring(1, tmp.Length - 1).Trim();
                                Node.index = index++;
                                Nodeay.Nodes.Add(Node);
                                Node.Tag = "";
                                char[] ch = new char[2];
                                ch[0] = '[';
                                ch[1] = ']';
                                if (tmps.StartsWith("{"))
                                {
                                    GetJsonTree(de.Value.ToString().Trim(), Node, 1);
                                }

                            }
                            else
                            {
                                Node.Text += " : \"" + de.Value.ToString() + "\"";
                                Node.value = de.Value.ToString();
                                Node.Tag = de.Value.ToString();
                                Node.index = index++;
                                Node.ImageIndex = 2;
                                Node.SelectedImageIndex = 2; 
                                Nodeay.Nodes.Add(Node);
                            }

                        }
                    }
                    Nodeay = null;
                }
                return true;
            }
            else
            {
                try
                {
                    
                    xStr = xStr.Replace("\\\"\"","\\\"");//20141227 add by erric
                    sdc = new SortedDictionary<string, string>();
                    //ht = JavaScriptConvert.DeserializeObject(xStr, typeof(Hashtable)) as Hashtable;
                    ht = JavaScriptConvert.DeserializeObject<Hashtable>(xStr);
                    foreach (DictionaryEntry de in ht)
                    {
                        //al.Add(de.Key.ToString());
                        //al2.Add(de.Value.ToString());
                        sdc.Add(de.Key.ToString(), de.Value.ToString()); 
                    }
                    ht.Clear();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("不符合的資料型態!!","錯誤");
                    treeView1.Nodes.Clear();
                    return false;
                }

               /* foreach (KeyValuePair<string, string> k in sdc)
                {
                    ht.Add(k.Key,k.Value);
                }*/
                
               // Dictionary<string, string> dc = JavaScriptConvert.DeserializeObject(xStr, typeof(Dictionary<string, string>)) as Dictionary<string, string>;
            }

            if (ht != null)
            {
                xNode.Tsdc = sdc;
                //foreach (DictionaryEntry de in ht)
                foreach (KeyValuePair<string, string> de in sdc)
                {
                    
                    TreeNodeEx Node = new TreeNodeEx();
                    Node.Text = de.Key.ToString(); //節點文字
                    Node.value = de.Key.ToString(); //節點文字
                    Node.Name = de.Key.ToString(); //節點文字
                    Node.nodename = de.Key.ToString(); //節點文字
                    if (xNode.nodename == "ROOT") 
                     Node.path = "";
                        //Node.path = xNode.path + ",'" + xNode.nodename + "'";
                    else
                        //Node.path = xNode.path + ",'" + xNode.nodename + "'";
                    Node.path = xNode.path ;
                        //Node.path = "'" + xNode.nodename + "'";                    
                    SetNodeSelectPath(Node);

                    string tmp = de.Value.ToString().Trim();
                    if (tmp.EndsWith("}") && tmp.StartsWith("{"))
                    {
                        Node.index = index++;
                        xNode.Nodes.Add(Node);
                        Node.Tag = "";
                        GetJsonTree(de.Value.ToString(), Node,0);
                    }
                    else if (tmp.EndsWith("]") && tmp.StartsWith("["))
                    {
                        string tmps = tmp.Substring(1, tmp.Length - 1).Trim();
                        if (tmps.StartsWith("{"))
                        {
                            Node.index = index++;
                            xNode.Nodes.Add(Node);
                            Node.Tag = "";
                            char[] ch = new char[2];
                            ch[0] = '[';
                            ch[1] = ']';
                            GetJsonTree(de.Value.ToString().Trim(), Node, 1);
                        }
                        else
                        {
                            Node.index = index++;
                            Node.Text += " : \"" + de.Value.ToString() + "\"";
                            Node.value = de.Value.ToString();
                            Node.Tag = de.Value.ToString();
                            Node.ImageIndex = 2;
                            Node.SelectedImageIndex = 2;
                            xNode.Nodes.Add(Node);
                        }
                    }
                    else
                    {
                        Node.index = index++;
                        Node.Text += " : \"" + de.Value.ToString() + "\"";
                        Node.value = de.Value.ToString();
                        Node.Tag = de.Value.ToString();
                        Node.ImageIndex = 2;
                        Node.SelectedImageIndex = 2; 
                        xNode.Nodes.Add(Node);
                    }

                }
            }
            return true;  
        }

        private void SetNodeSelectPath(TreeNodeEx a)
        {
            string tmp = a.path;
            if (tmp.EndsWith(","))
            {
                tmp = tmp.Substring(0, tmp.Length - 1);
            }
            if (tmp.StartsWith(","))
            {
                tmp = tmp.Substring(1, tmp.Length - 1);
            }
            a.path = tmp + "," + a.nodename;// +"'";
            if (a.path.StartsWith(","))
            {
                a.path = a.path.Substring(1, a.path.Length - 1);
            }
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNodeEx a = (TreeNodeEx)e.Node;
            treeView1.SelectedNode = a;
            tb_VALUE.Text = a.Tag.ToString();
            tb_NAME.Text = a.nodename;
            //tb_SEARCH.Text = a.index.ToString();
            ss = "";
            //string tmp = GetTreeList(a);
            /*string tmp = a.path;
            if (tmp.EndsWith(","))
            {
                tmp = tmp.Substring(0, tmp.Length - 1);
            }                      
            if (tmp.StartsWith(","))
            {
                tmp = tmp.Substring(1, tmp.Length-1);
            }
            tb_PATH.Text = tmp + ",'"+a.nodename+"'";
            if (tb_PATH.Text.StartsWith(","))
            {
                tb_PATH.Text = tb_PATH.Text.Substring(1, tb_PATH.Text.Length - 1);
            }*/
            tb_PATH.Text = a.path;
            int count = 0;
            dataGridView1.Rows.Clear();

            if (a.Tsdc != null)
            {
                foreach (KeyValuePair<string, string> de in a.Tsdc)
                {
                  /*  if (count == 0)
                      dataGridView1.Rows[0].SetValues(de.Key.ToString(), de.Value.ToString());
                    else*/
                      dataGridView1.Rows.Add(de.Key.ToString(), de.Value.ToString());
                    count = 1;
                }
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
            CurrentNode = a;
            
        }

        private string GetTreeList(TreeNodeEx xnode)
        {
          
           if (xnode.Parent == null)
           {
               if (ss=="")
               {
                   return xnode.Text;
               }
               return ss;
            }
           else
           {
               if (xnode.Parent.Text != "ROOT")
               {
                   ss = "'" + xnode.Parent.Text + "'," + ss;
               }

               return GetTreeList((TreeNodeEx)xnode.Parent);
            }
        }

        //private TreeNodeEx FindNode(TreeNodeEx tnParent, string strValue)
        private TreeNodeEx FindNode(TreeNodeEx tnParent, int index)
        {
            if (tnParent == null) return null;

            //if (tnParent.nodename.ToUpper() == strValue) return tnParent;
            if (tnParent.index == index) return tnParent;
            else if (tnParent.Nodes.Count == 0) return null;

            TreeNodeEx tnCurrent, tnCurrentPar;

            //Init node
            tnCurrentPar = tnParent;
            tnCurrent = (TreeNodeEx)tnCurrentPar.FirstNode;

            while (tnCurrent != null && tnCurrent != tnParent)
            {
                while (tnCurrent != null)
                {
                    if (tnCurrent.index == index) return tnCurrent;
                    else if (tnCurrent.Nodes.Count > 0)
                    {
                        //Go into the deepest node in current sub-path
                        tnCurrentPar = tnCurrent;
                        tnCurrent = (TreeNodeEx)tnCurrent.FirstNode;
                    }
                    else if (tnCurrent != tnCurrentPar.LastNode)
                    {
                        //Goto next sible node
                        tnCurrent = (TreeNodeEx)tnCurrent.NextNode;
                    }
                    else
                        break;
                }

                //Go back to parent node till its has next sible node
                while (tnCurrent != tnParent && tnCurrent == tnCurrentPar.LastNode)
                {
                    tnCurrent = tnCurrentPar;
                    tnCurrentPar = (TreeNodeEx)tnCurrentPar.Parent;
                }

                //Goto next sible node
                if (tnCurrent != tnParent)
                    tnCurrent = (TreeNodeEx)tnCurrent.NextNode;
            }
            return null;
        }

        private void FindNodes(TreeNodeEx xnode, string strValue)
        {
            
            bool[] b = new bool[] { cbo_Name.Checked, cbo_Value.Checked, cbo_Path.Checked };
            for(int i=0;i<xnode.Nodes.Count;i++)
            {

                TreeNodeEx a = (TreeNodeEx)xnode.Nodes[i];
                string[] t = new string[] { a.nodename.ToUpper(), a.Tag.ToString().ToUpper(), a.path.ToString().ToUpper() };
                for (int j = 0; j < b.Length; j++)
                {
                    if (b[j])
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(t[j], CheckChars(strValue.ToUpper()), System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        //if (System.Text.RegularExpressions.Regex.IsMatch(t[j], strValue.ToUpper()))
                        {
                            string tmp = a.path;
                            if (tmp.StartsWith(","))
                            {
                                tmp = tmp.Substring(1, tmp.Length - 1);
                            }
                            dataGridView2.Rows.Add(a.index, a.nodename, a.Tag.ToString(), tmp);
                            int idx = dataGridView2.RowCount - 1;
                            if (idx % 2 == 0)
                            {
                                dataGridView2.Rows[idx].DefaultCellStyle.BackColor = Color.FromName("InactiveBorder");
                            }
                            else
                            {
                                dataGridView2.Rows[idx].DefaultCellStyle.BackColor = Color.FromName("Info");
                            }

                        }
                    }
                }
                FindNodes((TreeNodeEx)xnode.Nodes[i], strValue);
            }

        }
        private void FindNodes2(TreeNodeEx xnode, string path,string rvalue)
        {
            for (int i = 0; i < xnode.Nodes.Count; i++)
            {
                TreeNodeEx a = (TreeNodeEx)xnode.Nodes[i];
                if (System.Text.RegularExpressions.Regex.IsMatch(a.path.ToString().ToUpper(), CheckChars(path.ToUpper()), System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    string tmp = a.path;
                    if (tmp.StartsWith(","))
                    {
                        tmp = tmp.Substring(1, tmp.Length - 1);
                    }
                    a.value = rvalue;
                    a.Tag = rvalue;
                    a.Text = a.nodename + ":" + "\"" + rvalue + "\"";
                    FoundNode = a;
                    return;
                }
                FindNodes2((TreeNodeEx)xnode.Nodes[i], path, rvalue);
            }
        }

        TreeNodeEx b = null;
        private TreeNodeEx FindNodeWithIndex(TreeNodeEx xnode, string path,out int input)
        {
            input = 0;
            TreeNodeEx a = null;
            for (int i = 0; i < xnode.Nodes.Count; i++)
            {
                a = (TreeNodeEx)xnode.Nodes[i];
                if (System.Text.RegularExpressions.Regex.IsMatch(a.path.ToUpper(),CheckChars(path.ToUpper()), System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                //if (System.Text.RegularExpressions.Regex.IsMatch(a.path.ToUpper(), path.ToUpper()))
                {
                    input = 1;
                    return a;
                }
                else
                {
                    b = FindNodeWithIndex(a, path,out input);
                }
                if (b!=null && input ==1)
                {
                    return b;
                }
            }
            return null;
        }


        private void btn_Copy_Click(object sender, EventArgs e)
        {
            if (tb_PATH.Text=="")
            {
                return;
            }
            Clipboard.SetData(DataFormats.Text, tb_PATH.Text);
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            string tmp_path = "";
            if ((sender as DataGridView).RowCount > 0)
            {
                int index = Int32.Parse(dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString());
                tb_NAME.Text = dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString();
                tb_VALUE.Text = dataGridView2.Rows[e.RowIndex].Cells[2].Value.ToString();
               /* tmp_path = dataGridView2.Rows[e.RowIndex].Cells[3].Value.ToString();
                tb_PATH.Text = tmp_path + ",'" + tb_NAME.Text + "'";

                if (tb_PATH.Text.StartsWith(","))
                {
                    tb_PATH.Text = tb_PATH.Text.Substring(1, tb_PATH.Text.Length - 1);
                }*/
                tb_PATH.Text = dataGridView2.Rows[e.RowIndex].Cells[3].Value.ToString(); 
                  TreeNodeEx tnRet = null;
                  foreach (TreeNodeEx tn in treeView1.Nodes)
                  {
                      if (tn.Text.StartsWith("["))
                      {
                          continue;
                      }
                      tnRet = FindNode(tn, index);
                      if (tnRet != null) break;
                  }
                  if (tnRet != null)
                  {
                      
                      treeView1.SelectedNode = tnRet;
                      treeView1.SelectedNode.Expand();
                      treeView1.SelectedNode = tnRet;
                      treeView1.Focus();                      
                      tnRet.EnsureVisible();
                  }
            }
            //dataGridView2.Focus();
        }


        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNodeEx a = (TreeNodeEx)e.Node;
            treeView1.SelectedNode = a;
            if (a.Nodes.Count == 0)
            {
                return;
            }

                a.ImageIndex = 1;
                a.SelectedImageIndex = 1;


        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            TreeNodeEx a = (TreeNodeEx)e.Node;
            treeView1.SelectedNode = a;
            if (a.Nodes.Count == 0)
            {
                return;
            }

                a.ImageIndex = 0;
                a.SelectedImageIndex = 0;
            
        }

        private void GoRegisty(string key,string value)
        {
            //打開 子機碼 路徑。
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey("Software", true);
            ////檢查子機碼是否存在，檢查資料夾是否存在。
            if (Reg.GetSubKeyNames().Contains("JSonTools") == false)
            {
                //建立子機碼，建立資料夾。
                Reg.CreateSubKey("JSonTools");
                //寫入資料 Name,Value,"寫入類型"
                Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", key, value, RegistryValueKind.String);
            }
            else
            {
                //寫入資料 Name,Value,"寫入類型"
                Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", key, value, RegistryValueKind.String);
                //關閉 子機碼 路徑
                Reg.Close();
            }
        }

        private void GoRead()
        {
            //開啟指定的機碼目錄。
            RegistryKey oRegistryKey = Registry.LocalMachine.OpenSubKey("Software\\JSonTools");
            if (oRegistryKey != null)
            {
                //若目錄存在，則取出 key=cnstr 的值。
                if (oRegistryKey.GetValue("Path", "").ToString() != "")
                {
                    tb_TXT.Text = oRegistryKey.GetValue("Path").ToString();
                }

                if (oRegistryKey.GetValue("cbo_Name", "ERROR").ToString() == "Y")
                {
                    cbo_Name.Checked = true;
                }
                else if (oRegistryKey.GetValue("cbo_Name", "ERROR").ToString() == "N")
                {
                    cbo_Name.Checked = false;
                }
                else
                {
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", "cbo_Name", "Y", RegistryValueKind.String);
                }

                if (oRegistryKey.GetValue("cbo_Value", "ERROR").ToString() == "Y")
                {
                    cbo_Value.Checked = true;
                }
                else if (oRegistryKey.GetValue("cbo_Value", "ERROR").ToString() == "N")
                {
                    cbo_Value.Checked = false;
                }
                else
                {
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", "cbo_Value", "Y", RegistryValueKind.String);
                }

                if (oRegistryKey.GetValue("cbo_Path", "ERROR").ToString() == "Y")
                {
                    cbo_Path.Checked = true;
                }
                else if (oRegistryKey.GetValue("cbo_Path", "ERROR").ToString() == "N")
                {
                    cbo_Path.Checked = false;
                }
                else
                {
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", "cbo_Path", "Y", RegistryValueKind.String);
                }

                if (oRegistryKey.GetValue("chk_CheckNow", "ERROR").ToString() == "Y")
                {
                    chk_CheckNow.Checked = true;
                }
                else if (oRegistryKey.GetValue("chk_CheckNow", "ERROR").ToString() == "N")
                {
                    chk_CheckNow.Checked = false;
                }
                else
                {
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\JSonTools", "chk_CheckNow", "Y", RegistryValueKind.String);
                }

                if (oRegistryKey.GetValue("cbo_Big5", "").ToString() != "")
                {
                    cbo_Big5.SelectedIndex = Int32.Parse(oRegistryKey.GetValue("cbo_Big5").ToString());
                }

                if (oRegistryKey.GetValue("cbo_Sym", "").ToString() != "")
                {
                    cbo_Sym.SelectedIndex = Int32.Parse(oRegistryKey.GetValue("cbo_Sym").ToString());
                }
                //TP03
                {
                    if (oRegistryKey.GetValue("tP03_tb01", "").ToString() != "")
                    {
                        tP03_tb01.Text = oRegistryKey.GetValue("tP03_tb01").ToString();
                    }
                    if (oRegistryKey.GetValue("tP03_tb02", "").ToString() != "")
                    {
                        tP03_tb02.Text = oRegistryKey.GetValue("tP03_tb02").ToString();
                    }
                    if (oRegistryKey.GetValue("tP03_tb03", "").ToString() != "")
                    {
                        tP03_tb03.Text = oRegistryKey.GetValue("tP03_tb03").ToString();
                    }
                    if (oRegistryKey.GetValue("tP03_tb04", "").ToString() != "")
                    {
                        tP03_tb04.Text = oRegistryKey.GetValue("tP03_tb04").ToString();
                    }
                    if (oRegistryKey.GetValue("tP03_tb05", "").ToString() != "")
                    {
                        tP03_tb05.Text = oRegistryKey.GetValue("tP03_tb05").ToString();
                    }
                    if (oRegistryKey.GetValue("tP03_tb06", "").ToString() != "")
                    {
                        tP03_tb06.Text = oRegistryKey.GetValue("tP03_tb06").ToString();
                    }
                }
                //TP04
                {
                    if (oRegistryKey.GetValue("tP04_tb07", "").ToString() != "")
                    {
                        tP03_tb01.Text = oRegistryKey.GetValue("tP04_tb07").ToString();
                    }
                    if (oRegistryKey.GetValue("tP04_tb08", "").ToString() != "")
                    {
                        tP03_tb02.Text = oRegistryKey.GetValue("tP04_tb08").ToString();
                    }
                    if (oRegistryKey.GetValue("tP04_tb09", "").ToString() != "")
                    {
                        tP03_tb03.Text = oRegistryKey.GetValue("tP04_tb09").ToString();
                    }
                    if (oRegistryKey.GetValue("tP04_tb10", "").ToString() != "")
                    {
                        tP03_tb04.Text = oRegistryKey.GetValue("tP04_tb10").ToString();
                    }
                    if (oRegistryKey.GetValue("tP04_tb11", "").ToString() != "")
                    {
                        tP03_tb05.Text = oRegistryKey.GetValue("tP04_tb11").ToString();
                    }
                    if (oRegistryKey.GetValue("tP04_tb12", "").ToString() != "")
                    {
                        tP03_tb06.Text = oRegistryKey.GetValue("tP04_tb12").ToString();
                    }
                }
                //TP01_DGV
                {
                    if (oRegistryKey.GetValue("tP01_dgv", "").ToString() != "")
                    {
                        string[] tmp = Split(",", oRegistryKey.GetValue("tP01_dgv").ToString());
                        for (int i = 0; i < tmp.Length;i++ )
                        {
                            tP01_dgv.Rows.Add("",tmp[i]);
                        }                        
                    }
                }
                //TP06
                if (oRegistryKey.GetValue("tP06_tbPath", "").ToString() != "")
                {
                    tP06_tbPath.Text = oRegistryKey.GetValue("tP06_tbPath").ToString();
                }
            }
        }

        private void 版更說明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form7 f7 = new Form7();
            f7.ShowDialog();
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            string mName = (sender as Button).Name;
            FileStream myFile = null;
            StreamReader rd = null;
            string output = "";
            treeView1.Nodes.Clear();
            MyNode.Nodes.Clear();
            if (mName == "btn_Load")
            {
                if (tb_TXT.Text == "")
                {
                    MessageBox.Show("請選擇檔案!!", "提示");
                    return;
                }
                if (!File.Exists(tb_TXT.Text))
                {
                    MessageBox.Show("檔案不存在!!", "錯誤");
                    return;
                }
                myFile = File.Open(tb_TXT.Text, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                rd = new StreamReader(myFile, System.Text.Encoding.GetEncoding("Big5"));
                output = rd.ReadToEnd();
                output = output.Replace("\\", "\\\\");
                rd.Dispose();
                myFile.Dispose();
            }
            else if (mName == "btn_UpdatePool")
            {
                
                if (Pool==null)
                {
                    return;
                }
                byte[] byteArray = Encoding.Unicode.GetBytes(Pool.ToString());
                    MemoryStream stream = new MemoryStream( byteArray ); 
         
                    // convert stream to string
                    rd = new StreamReader(stream, Encoding.Unicode);
                output = rd.ReadToEnd();
                output = output.Replace("\\", "\\\\");
                rd.Dispose();
            }
            else if (mName == "btn_LoadPool")
            {
                if (tb_TXT.Text == "")
                {
                    MessageBox.Show("請選擇檔案!!", "提示");
                    return;
                }
                if (!File.Exists(tb_TXT.Text))
                {
                    MessageBox.Show("檔案不存在!!", "錯誤");
                    return;
                }
                myFile = File.Open(tb_TXT.Text, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                rd = new StreamReader(myFile, System.Text.Encoding.GetEncoding("Big5"));
                output = rd.ReadToEnd();
                output = output.Replace("\\", "\\\\");
                rd.Dispose();
                myFile.Dispose();
                Pool = JObject.Parse(output);
            }
            MyNode.Text = "ROOT"; //節點文字
            MyNode.nodename = "ROOT";
            MyNode.path = "";
            MyNode.Tag = "5";
            MyNode.index = index++;
            treeView1.Nodes.Add(MyNode);
            if (!GetJsonTree(output, MyNode, 0))
            {
                return;
            }
            
            treeView1.Nodes[0].Expand();
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
           // treeView1.TreeViewNodeSorter = new NodeSorter();
        }

        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            tb_TXT.Text = file.FileName;
            if (!File.Exists(tb_TXT.Text))
            {
                MessageBox.Show("檔案不存在!!", "錯誤");
                return;
            }
            GoRegisty("Path", tb_TXT.Text);
            btn_Load.PerformClick();
        }

        private void btn_Select_Click(object sender, EventArgs e)
        {
            //if (cbo_Name.Checked && cbo_Value.Checked && cbo_Path.Checked && tb_SEARCH.Text=="")
            if ( tb_SEARCH.Text=="")
            {

                if (MessageBox.Show("搜尋空字串將花費較久的時間，是否確定進行?", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            DateTime dtone = DateTime.Now;
            dataGridView2.Rows.Clear();
            FindNodes(MyNode, tb_SEARCH.Text.ToUpper());

            DateTime dtwo = DateTime.Now;
            TimeSpan span = dtwo.Subtract(dtone); //算法是dtone 减去 dtwo
            string seconds = string.Format((((int)span.TotalMilliseconds) / 1000.0).ToString(), "0.000") + "秒";

            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            if (dataGridView2.RowCount <= 0)
            {
                lbl_Msg.Text = "無符合資料!!         費時:" + seconds;
                lbl_Msg.ForeColor = Color.Red;
            }
            else
            {
                lbl_Msg.Text = "符合資料: " + dataGridView2.RowCount.ToString() + "筆         費時:" + seconds;
                lbl_Msg.ForeColor = Color.Green;
            }
        }

        private void cbo_CheckedChanged(object sender, EventArgs e)
        {
            cboChanged((sender as CheckBox));
        }

        private void cboChanged(CheckBox cbo)
        {
            string s = "";
            string key = cbo.Name;
            if (cbo.Checked)
            {
                s = "Y";
            }
            else
            {
                s = "N";
            }
            GoRegisty(key, s);
        }

        private void cbo_Big5_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSelectChanged(sender as ComboBox);
        }
        private void cboSelectChanged(ComboBox cbo)
        {
            string s = cbo.SelectedIndex.ToString();
            string key = cbo.Name;
            GoRegisty(key, s);
        }

        private void tb_SEARCH_TextChanged(object sender, EventArgs e)
        {
            if (CheckChineseString(tb_SEARCH.Text) < Int32.Parse(cbo_Big5.Text) &&
                (tb_SEARCH.Text.Length < Int32.Parse(cbo_Sym.Text)))
            {
                return;
            }
            
            if (!chk_CheckNow.Checked || tb_SEARCH.Text == "" )
            {
                return;
            }
            if (tb_SEARCH.Text.Length >= Int32.Parse(cbo_Sym.Text) || CheckChineseString(tb_SEARCH.Text) >= Int32.Parse(cbo_Big5.Text))
            {
                btn_Select.PerformClick();
            }
            
        }

        private int CheckChineseString(string strInputString)
        {
            int intCode = 0;
            int count = 0;
           // int intIndexNumber=0;
            for (int i = 0; i < strInputString.Length;i++)
            {
                //中文範圍（0x4e00 - 0x9fff）轉換成int（intChineseFrom - intChineseEnd）
                int intChineseFrom = Convert.ToInt32("4e00", 16);
                int intChineseEnd = Convert.ToInt32("9fff", 16);
                if (strInputString != "")
                {
                    //取得input字串中指定判斷的index字元的unicode碼
                    intCode = Char.ConvertToUtf32(strInputString, i);

                    if (intCode >= intChineseFrom && intCode <= intChineseEnd)
                    {
                        count++;
                        //return true;     //如果是範圍內的數值就回傳true
                    }
                    else
                    {
                        //return false;    //如果是範圍外的數值就回傳true
                    }
                }
            }
            return count;
        }

        private void btn_AddNewPage_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Add("123");
        }

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right )
            {
                contextMenuStrip1.Show(MousePosition);
            }  
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            btn_Lock.PerformClick();
            bool Isadd = false;
            string s = "";
            string key = "";
            if (CurrentNode.Nodes.Count == 0)
            {
                //tP01_dgv.Rows.Add(CurrentNode.index, CurrentNode.nodename, CurrentNode.Tag.ToString(), CurrentNode.path);
                tP01_dgv.Rows.Add(CurrentNode.index, CurrentNode.nodename, CurrentNode.Tag.ToString(), tb_PATH.Text);
                tP01_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;                
                for (int i = 0; i < tP01_dgv.RowCount;i++ )
                {
                    s += tP01_dgv.Rows[i].Cells[1].Value.ToString() + ",";
                }
                if (s.EndsWith(","))
                {
                    s = s.Substring(0, s.Length - 1);
                }
                //GoRegisty(tP01_dgv.Name, s); 
                showinfo(CurrentNode.nodename + "已加入頁籤:變數1");
            }
            else
            {
                for(int i=0;i<6;i++)
                {
                    if (tP03_tbList[i].Text == "")
                    {
                        Isadd = true;
                        tP03_tbList[i].Text = CurrentNode.path;
                        s = CurrentNode.path;
                        key = tP03_tbList[i].Name;
                        //tP03_tbList[i].Tag = CurrentNode.nodename;
                        showinfo(CurrentNode.nodename + "已加入頁籤:變數群組1");
                        if (CurrentNode.Tsdc != null)
                        {
                            foreach (KeyValuePair<string, string> de in CurrentNode.Tsdc)
                            {
                                tP03_dgvList[i].Rows.Add(de.Key.ToString(), de.Value.ToString());
                            }
                            tP03_dgvList[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        }
                        break;
                    }
                }
                if (!Isadd)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (tP04_tbList[i].Text == "")
                        {
                            Isadd = true;
                            tP04_tbList[i].Text = CurrentNode.path;
                            s = CurrentNode.path;
                            key = tP03_tbList[i].Name;
                            showinfo(CurrentNode.nodename + "已加入頁籤:變數群組2");
                            //tP03_tbList[i].Tag = CurrentNode.nodename;
                            if (CurrentNode.Tsdc != null)
                            {
                                foreach (KeyValuePair<string, string> de in CurrentNode.Tsdc)
                                {
                                    tP04_dgvList[i].Rows.Add(de.Key.ToString(), de.Value.ToString());
                                }
                                tP04_dgvList[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                            }
                            break;
                        }
                    }
                }
                if (!Isadd)
                {
                    MessageBox.Show("監看的變數群組已達上限，請先刪除後再加入!", "錯誤");
                }
                else
                {                   
                    //GoRegisty(key, s);
                }
            }            
        }

        private void InitPages()
        {
            //TP03
            {
                tP03_cboDel.SelectedIndex = 6;
                tP03_tbList.Add(tP03_tb01);
                tP03_tbList.Add(tP03_tb02);
                tP03_tbList.Add(tP03_tb03);
                tP03_tbList.Add(tP03_tb04);
                tP03_tbList.Add(tP03_tb05);
                tP03_tbList.Add(tP03_tb06);
                tP03_dgvList.Add(tP03_dgv01);
                tP03_dgvList.Add(tP03_dgv02);
                tP03_dgvList.Add(tP03_dgv03);
                tP03_dgvList.Add(tP03_dgv04);
                tP03_dgvList.Add(tP03_dgv05);
                tP03_dgvList.Add(tP03_dgv06);
            }
            //TP04
            {
                tP04_cboDel.SelectedIndex = 6;
                tP04_tbList.Add(tP04_tb07);
                tP04_tbList.Add(tP04_tb08);
                tP04_tbList.Add(tP04_tb09);
                tP04_tbList.Add(tP04_tb10);
                tP04_tbList.Add(tP04_tb11);
                tP04_tbList.Add(tP04_tb12);
                tP04_dgvList.Add(tP04_dgv07);
                tP04_dgvList.Add(tP04_dgv08);
                tP04_dgvList.Add(tP04_dgv09);
                tP04_dgvList.Add(tP04_dgv10);
                tP04_dgvList.Add(tP04_dgv11);
                tP04_dgvList.Add(tP04_dgv12);
            }
        }

        private void tP03_btnReFresh_Click(object sender, EventArgs e)
        {
            if (tP03_tbList.Count <= 0) return;
            int input = 0;
            for (int i = 0; i < 6; i++)
            {
                if (tP03_tbList[i].Text == "")
                {
                    continue;
                }
                else
                {
                    TreeNodeEx a = FindNodeWithIndex(MyNode, tP03_tbList[i].Text,out input);
                    if (a != null)
                    {
                        if (a.Tsdc != null)
                        {
                            tP03_dgvList[i].Rows.Clear();
                            foreach (KeyValuePair<string, string> de in a.Tsdc)
                            {
                                tP03_dgvList[i].Rows.Add(de.Key.ToString(), de.Value.ToString());
                            }
                            tP03_dgvList[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        }
                    }
                }
            }  
        }

        private void tP04_btnReFresh_Click(object sender, EventArgs e)
        {
            int input = 0;
            for (int i = 0; i < 6; i++)
            {
                if (tP04_tbList[i].Text == "")
                {
                    continue;
                }
                else
                {
                    TreeNodeEx a = FindNodeWithIndex(MyNode, tP04_tbList[i].Text, out input);
                    if (a != null)
                    {
                        if (a.Tsdc != null)
                        {
                            tP04_dgvList[i].Rows.Clear();
                            foreach (KeyValuePair<string, string> de in a.Tsdc)
                            {
                                tP04_dgvList[i].Rows.Add(de.Key.ToString(), de.Value.ToString());
                            }
                            tP04_dgvList[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        }
                    }
                }
            }  
        }
        private void tP01_btnReFresh_Click(object sender, EventArgs e)
        {
            int input = 0;
            for (int i = 0; i < tP01_dgv.RowCount; i++)
            {
                if (tP01_dgv.Rows[i].Cells[3].Value==null)
                {
                    continue;
                }
                TreeNodeEx a = FindNodeWithIndex(MyNode, tP01_dgv.Rows[i].Cells[3].Value.ToString(), out input);
                if (a != null)
                {
                    tP01_dgv.Rows[i].SetValues(a.index,a.nodename,a.Tag.ToString(),a.path);
                    tP01_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }
                
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            InitPages();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( tabControl1.SelectedIndex == 2)
            {
                if (treeView1.Nodes.Count != 0)
                {
                    tP01_btnReFresh.PerformClick();
                }
            }
            if (tabControl1.SelectedIndex == 3)
            {
                if (treeView1.Nodes.Count != 0)
                {
                    tP03_btnReFresh.PerformClick();
                }
            }
            if (tabControl1.SelectedIndex == 4)
            {
                if (treeView1.Nodes.Count != 0)
                {
                    tP04_btnReFresh.PerformClick();
                }
            }
        }

        private string CheckChars(string a)
        {
            List<int> tmplist = new List<int>();

            string tmp = a;
            for (int i = 0; i < a.Length;i++ )
            {
                if (a[i]=='[')
                {
                    tmplist.Add(i);
                }
                if (a[i] == ']')
                {
                    tmplist.Add(i);
                }
            }

            for (int i = tmplist.Count-1; i >= 0; i--)
            {
                tmp = tmp.Insert(tmplist[i], "\\");
            }
            return tmp;
        }

        private void tP03_btn0X_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "")
            {
                foreach (TextBox tb in tP03_tbList)
                {
                    tb.Text = "";
                    tb.Tag = "";
                    GoRegisty(tb.Name, "");
                }
                foreach (DataGridView d in tP03_dgvList)
                {
                    d.Rows.Clear();
                    d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }
            }
            else
            {
                int idx = Int32.Parse((sender as Button).Text) - 1;
                if (tP03_tbList[idx].Text != "")
                {
                    tP03_tbList[idx].Text = "";
                    tP03_tbList[idx].Tag = "";
                    tP03_dgvList[idx].Rows.Clear();
                    tP03_dgvList[idx].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    GoRegisty(tP03_tbList[idx].Name, "");
                }
            }
        }
        private void tP04_btn0X_Click(object sender, EventArgs e)
        {
            if (tP04_cboDel.Text == "ALL")
            {
                foreach (TextBox tb in tP04_tbList)
                {
                    tb.Text = "";
                    tb.Tag = "";
                    GoRegisty(tb.Name, "");
                }
                foreach (DataGridView d in tP04_dgvList)
                {
                    d.Rows.Clear();
                    d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }
            }
            else
            {
                int idx = Int32.Parse((sender as Button).Text) - 7;
                if (tP04_tbList[idx].Text != "")
                {
                    tP04_tbList[idx].Text = "";
                    tP04_tbList[idx].Tag = "";
                    tP04_dgvList[idx].Rows.Clear();
                    tP04_dgvList[idx].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    GoRegisty(tP04_tbList[idx].Name, "");
                }
            }
        }

        int idx_tP01_dgv = -1;
        string tP01_CellValue = "";

        private void tP01_btn0X_Click(object sender, EventArgs e)
        {
            if (tP01_dgv.RowCount <=0)
            {
                return;
            }
            tP01_dgv.Rows.Clear();
            GoRegisty(tP01_dgv.Name, "");
        }

        private void tP01_dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            idx_tP01_dgv = e.RowIndex;
            tP01_CellValue = tP01_dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        private void 刪除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (idx_tP01_dgv != -1)
            {
                tP01_dgv.Rows.RemoveAt(idx_tP01_dgv);
            }
            string s = "";
            for (int i = 0; i < tP01_dgv.RowCount; i++)
            {
                s += tP01_dgv.Rows[i].Cells[1].Value.ToString() + ",";
            }
            if (s.EndsWith(","))
            {
                s = s.Substring(0, s.Length - 1);
            }
            GoRegisty(tP01_dgv.Name, s);
        }

        private void tP01_dgv_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right && idx_tP01_dgv != -1)
            {
                contextMenuStrip2.Show(MousePosition);
            }  
        }

        private void 複製ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (idx_tP01_dgv != -1)
            {
                Clipboard.SetData(DataFormats.Text, tP01_CellValue); 
            }
        }
        public static string[] Split(string separator, string Text)
        {

            string[] sp = new string[] { separator };

            return Text.Split(sp, StringSplitOptions.RemoveEmptyEntries);

        }

        [DllImport("user32.dll", EntryPoint = "LockWindowUpdate")]
        public static extern int LockWindowUpdate(int hwndLock);

        private void showinfo(string s)
        {
           try
            {
                LockWindowUpdate(this.Handle.ToInt32());
                //Form2 f2 = new Form2();
                //f2.SetText(s,this.Size,this.Location);
                //f2.Show();
              /* f2.Left = this.Left + this.Width / 2 - f2.width / 2;
                f2.Top = this.Top + this.Height / 2 - f2.height / 2;*/
                //this.Focus();  
               
            }
            finally
            {
                LockWindowUpdate(0);
            }

        }

        private void tP05_tbInput_TextChanged(object sender, EventArgs e)
        {
            if (tP05_tbInput01.TextLength <= 4) return;                        
            tP05_Btn01.PerformClick();
        }

        private void tP05_RB_S_CheckedChanged(object sender, EventArgs e)
        {
            if (tP05_RB2 == 0)
            {                
                string Result = tP05_Result01.Text;
                if (Result == "") return;
                string tmp = (sender as RadioButton).Tag.ToString();
                nowRP02 = tmp;
                string Retmp = Result.Substring(8, Result.Length - 8);
                Retmp = tmp + Retmp;
                tP05_Result01.Text = Retmp;
            }

            if (tP05_Result01.Text == "")
            {
                return;
            }
            Clipboard.SetData(DataFormats.Text, tP05_Result01.Text);

        }

        private void tP05_Btn01_Click(object sender, EventArgs e)
        {
            tP05_Result01.Text = GetValue();
            if (tP05_Result01.Text == "")
            {
                return;
            }
            Clipboard.SetData(DataFormats.Text, tP05_Result01.Text);
        }

        ArrayList strlist = new ArrayList();
        ArrayList intlist = new ArrayList();
        private string GetValue()
        {
            strlist.Clear();
            intlist.Clear();
            if (tP05_tbInput01.Text == "") return "";

            string Result = "";
            bool isusearray = false;
            if (tP05_RB2 == 0 || tP05_RB2 == 1)
            {
                string tmp_s = tP05_tbInput01.Text;
                if (tmp_s.ToUpper().StartsWith("PUB"))
                {
                    tmp_s = tmp_s.Substring(4, tmp_s.Length - 4);
                    tP05_RB3_2.Checked = true;
                }
                string[] tmp = Split(".", tmp_s);

                char[] c = null;
                string subname = "";
                
                if (tmp[0].ToUpper() == "GPARAM")
                {
                    tP05_RB3_1.Checked = true;
                    for (int k = 1; k < tmp.Length;k++ )
                    {
                        c = tmp[k].ToCharArray();
                        if (k == 1)
                        {
                            for (int i = 0; i < c.Length; i++)
                            {
                                if (c[i] == '_')
                                {
                                    if (tmp[1].ToUpper().EndsWith("_A") || tmp[1].ToUpper().EndsWith("_B") ||
                                         tmp[1].ToUpper().EndsWith("_C") || tmp[1].ToUpper().EndsWith("_D") ||
                                         tmp[1].ToUpper().EndsWith("_E") || tmp[1].ToUpper().EndsWith("_F"))
                                    {
                                        subname = "";
                                    }
                                    break;
                                }
                                subname += c[i];                                
                            }

                            if (subname != "")
                            {
                                strlist.Add("'" + subname + "'");
                                intlist.Add(0);
                                strlist.Add("'" + tmp[k] + "'");
                                intlist.Add(0);
                            }
                            else
                            {
                                strlist.Add("'" + tmp[k] + "'");
                                intlist.Add(0);
                            }
                        }
                        else
                        {
                            string s = "";
                            c = tmp[k].ToCharArray();
                            if (!Chkarray(c, out s))
                            {
                                s = "'" + tmp[k] + "'";                       
                            }
                            string[] m = Split(",", s);
                            foreach (string l in m)
                            {
                                strlist.Add(l);
                            }                            
                        }
                    }
                    
                }
                else //矩陣 gPayType[mPayTypeIdx].Invoiced
                {
                    for (int k = 0; k < tmp.Length; k++)
                    {
                        if (k==0)
                        {
                            string tmp0 = tmp[k].ToUpper();
                            if (tmp0.EndsWith("INFO") || tmp0 == "FUSER" || tmp0 == "GCREDITCARD"  || 
                                tmp0 == "GCURRFUNC"   || tmp0 == "GPAYTYPE" ||tmp0 == "PAYMENT" )
                            {
                                tP05_RB3_5.Checked = true;
                            }
                            else if (tmp0.StartsWith("GCURRPOSTB") ||
                                     tmp0.StartsWith("GPAYMENT") ||
                                     tmp0 == "GPOSTA" ||
                                     tmp0.StartsWith("GPOSTB") ||
                                     tmp0.StartsWith("GPOSTC") )
                            {
                                tP05_RB3_3.Checked = true;
                            }
                        }
                        string s = "";
                        c = tmp[k].ToCharArray();

                        if (!Chkarray(c, out s))
                        {
                            s = "'" + tmp[k] + "'";
                        }
                        string[] m = Split(",", s);
                        foreach (string l in m)
                        {
                            strlist.Add(l);
                        }
                    }
                }
                if (tP05_RB2 == 0)
                {
                    if (intlist.Count == 4)
                    {
                        //if ((int)(intlist[1]) != 1 || (int)(intlist[3]) != 1)
                        if (!ChkIntArray(intlist))
                        {
                            isusearray = true;
                            for (int q = 0; q < intlist.Count; q++)
                            {
                                if ((int)intlist[q] == 1)
                                {
                                    strlist[q] = "IntToStr(" + strlist[q] + ")";
                                }
                            }
                        }
                    }
                    else if (intlist.Count == 5)
                    {
                        //if ((int)(intlist[1]) != 1 || (int)(intlist[3]) != 1 || (int)(intlist[4]) != 1)
                        if (!ChkIntArray(intlist))
                        {
                            isusearray = true;
                            for (int q = 0; q < intlist.Count; q++)
                            {
                                if ((int)intlist[q] == 1)
                                {
                                    strlist[q] = "IntToStr(" + strlist[q] + ")";
                                }
                            }
                        }
                    }
                    else if (intlist.Count > 5)
                    {
                        isusearray = true;
                        for (int q = 0; q < intlist.Count; q++)
                        {
                            if ((int)intlist[q] == 1)
                            {
                                strlist[q] = "IntToStr(" + strlist[q] + ")";
                            }
                        }
                    }
                }

                string ss = "[";
                string ss2 = "]";
                if (!isusearray)
                {
                    ss = ""; ss2 = "";
                }

                string dpnNode = "";
                if (tP05_RB3_1.Checked)
                {
                    dpnNode = tP05_RB3_1.Text + ",";
                    tmpLen = tP05_RB3_1.Text.Length;
                }
                else if (tP05_RB3_2.Checked)
                {
                    dpnNode = tP05_RB3_2.Text + ",";
                    tmpLen = tP05_RB3_2.Text.Length;
                }
                else if (tP05_RB3_3.Checked)
                {
                    dpnNode = tP05_RB3_3.Text + ",";
                    tmpLen = tP05_RB3_3.Text.Length;
                }
                else if (tP05_RB3_4.Checked)
                {
                    dpnNode = tP05_RB3_4.Text + ",";
                    tmpLen = tP05_RB3_4.Text.Length;
                }
                else if (tP05_RB3_5.Checked)
                {
                    dpnNode = tP05_RB3_5.Text + ",";
                    tmpLen = tP05_RB3_5.Text.Length;
                }
                else
                {
                    dpnNode = tP05_RB3_1.Text + ",";
                    tmpLen = tP05_RB3_1.Text.Length;
                }


                if (tP05_RB2 == 0)
                {
                    if (tP05_RB_S.Checked)
                    {
                        Result = "GetPub_S(" + dpnNode + ss;
                        nowRP02 = "GetPub_S";
                    }
                    else if (tP05_RB_I.Checked)
                    {
                        Result = "GetPub_I(" + dpnNode + ss;
                        nowRP02 = "GetPub_I";
                    }
                    else if (tP05_RB_E.Checked)
                    {
                        Result = "GetPub_E(" + dpnNode + ss;
                        nowRP02 = "GetPub_E";
                    }
                    else if (tP05_RB_B.Checked)
                    {
                        Result = "GetPub_B(" + dpnNode + ss;
                        nowRP02 = "GetPub_B";
                    }
                    else if (tP05_RB_D.Checked)
                    {
                        Result = "GetPub_D(" + dpnNode + ss;
                        nowRP02 = "GetPub_D";
                    }
                    else
                    {
                        Result = "GetPub_S(" + dpnNode + ss;
                        nowRP02 = "GetPub_S";
                    }
                }
                else if (tP05_RB2 == 1)
                {
                    Result = "SetPubValue("+ dpnNode +"[";
                    for (int q = 0; q < intlist.Count; q++)
                    {
                        if ((int)intlist[q] == 1)
                        {
                            strlist[q] = "IntToStr(" + strlist[q] + ")";
                        }
                    }
                }

                for (int j = 0; j < strlist.Count; j++)
                {
                    {
                        if (j != strlist.Count - 1)
                            Result += strlist[j] + ",";
                        else
                        {
                            if (tP05_RB2 == 0)
                            {
                                Result += strlist[j] + ss2 + ")";
                            }
                            else if (tP05_RB2 == 1)
                            {
                                Result += strlist[j] + "],)";
                            }
                        }
                    }
                }
            }
            return Result;
        }

        private void tP05_tbInput_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tP05_tbInput01.SelectAll();
        }

        private void tb_SEARCH_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tb_SEARCH.SelectAll();
        }
        private void tP05_Btn02_Click(object sender, EventArgs e)
        {
            tP05_tbInput01.Text = "";
            tP05_Result01.Text = "";
            tP05_tbMEMO.Text = "";
            tP05_RB_S.Checked = true;
            tP05_RB2_GP.Checked = true;
            tP05_RB2 = 0;
        }

        private void tP05_RB2_GP_CheckedChanged(object sender, EventArgs e)
        {
            tP05_RB2 = Int32.Parse((sender as RadioButton).Tag.ToString());
            tP05_Btn01.PerformClick();
        }

        private void tP05_RB4_GF_CheckedChanged(object sender, EventArgs e)
        {
            tP05_RB4 = Int32.Parse((sender as RadioButton).Tag.ToString()); 
        }

        private void tP05_Btn04_Click(object sender, EventArgs e)
        {
            tP05_tbInput02.Text = "";
            tP05_Result02.Text = "";            
            tP05_RB3_1.Checked = true;
            tP05_RB4_GF.Checked = true;
            tP05_RB4 = 0;
        }

        private void tP05_tbInput02_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tP05_tbInput02.SelectAll();
        }

        private void tP05_tbInput02_TextChanged(object sender, EventArgs e)
        {
            tP05_Btn03.PerformClick();
        }

        private void tP05_Btn03_Click(object sender, EventArgs e)
        {
            tP05_Result02.Text = GetValue2((sender as RadioButton).Tag.ToString());
            if (tP05_Result02.Text == "")
            {
                return;
            }
            Clipboard.SetData(DataFormats.Text, tP05_Result02.Text);
        }

        private string GetValue2(string tagstring)
        {
            if (tP05_tbInput02.Text == "") return "";

            string Result = tagstring;
            
            if (tP05_RB4 == 0 )
            {                
                
                string tmp_s = tP05_tbInput02.Text;

            }
            else
            {

            }
            return Result;
        }
        private bool Chkarray(char[] c,out string s)
        {
            int vars = 0;
            ArrayList arr = new ArrayList();
            string arrayname = "";
            string subname2 = "";
            string tmp = "";
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '[')
                {
                    vars=1;
                    continue;
                }

                if (vars == 1)
                {
                    if (c[i] != ']')
                        arrayname += c[i];
                    else
                    {
                        arr.Add(arrayname);
                        arrayname = "";
                        vars = 2;
                    }

                }
                if (vars == 0)
                {
                    subname2 += c[i];
                }
            }
            if (arr.Count > 0)
            {
                tmp = "'" + subname2 + "'";
                intlist.Add(0);
                for (int i = 0; i < arr.Count;i++ )
                {
                    tmp += "," + arr[i].ToString();
                    intlist.Add(1);
                }
                s = tmp;
                return true;
            }
            else
            {
                tmp = "'" + subname2 + "'";
                intlist.Add(0);
                s = tmp;
                return false;
            }
        }
        public bool ChkIntArray(ArrayList al)
        {
            double sum = 0;
            double value = 0;
            double basevalue = 2.0;
            double power = 0.0;
            for (int i = 0; i < al.Count;i++ )
            {
                power = i;
                value = Math.Pow(basevalue, power);
                sum += (int)al[i] * value;
            }
            sum = (int)sum;

            if (sum == (int)ary.N0100 ||
                sum == (int)ary.N0101 ||
                sum == (int)ary.N01011 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void tP06_Path_Click(object sender, EventArgs e)
        {
            string path ="";
            if (tP06_tbPath.Text != "")
            {
                folderBrowserDialog1.SelectedPath = tP06_tbPath.Text;
            }
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog1.SelectedPath;
            }
            tP06_tbPath.Text = path;
            GoRegisty("tP06_tbPath", tP06_tbPath.Text);
        }

        private void tP06_BtnCRO_Click(object sender, EventArgs e)
        {
            GoRegisty("tP06_tbPath", tP06_tbPath.Text);
            if (tP06_tbPath.Text == "")
            {
                return;
            }
            if (isDirectory(tP06_tbPath.Text))
            {
                string p = "";
                if (!tP06_tbPath.Text.EndsWith("\\"))
                {
                    p = tP06_tbPath.Text+"\\";
                    //p = tP06_tbPath.Text.Substring(0, tP06_tbPath.Text.Length - 1);
                }

                p = @""+p+"*.*"+@"";
                Execute("echo y| attrib " + p +" /S /D -R",0);
                tP06_Lb01.Text = "資料屬性變更完成!";
                tP06_timer1.Enabled = true;
            }
            else
            {
                tP06_Lb01.Text = "無此目錄!!";
                tP06_timer1.Enabled = true;
                //MessageBox.Show("無此目錄!!", "錯誤");
            }
        }

        public static string Execute(string command, int seconds)
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程  
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束  
                        }
                        else
                        {
                            process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒  
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        private void tP06_timer1_Tick(object sender, EventArgs e)
        {
            tP06_Lb01.Text = "";
            tP06_timer1.Enabled = false;
        }

        private void tP05_RB3_1_CheckedChanged(object sender, EventArgs e)
        {
            if (tP05_RB2 == 0)
            {
                string Result = tP05_Result01.Text;
                if (Result == "") return;
                //int tmpLen = (sender as RadioButton).Text.Length;
                string tmp = (sender as RadioButton).Text;
                string Retmp = nowRP02+"("+tmp+ Result.Substring(9+tmpLen, Result.Length - (9+tmpLen));
                //Retmp = tmp + Retmp;
                tP05_Result01.Text = Retmp;
            }
            tmpLen = (sender as RadioButton).Text.Length;
            Clipboard.SetData(DataFormats.Text, tP05_Result01.Text);
        }

        // Create a node sorter that implements the IComparer interface.
        public class NodeSorter : IComparer
        {
            // Compare the length of the strings, or the strings
            // themselves, if they are the same length.
            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;

                // Compare the length of the strings, returning the difference.
                if (tx.Text.Length != ty.Text.Length)
                    return tx.Text.Length - ty.Text.Length;

                // If they are the same length, call Compare.
                return string.Compare(tx.Text, ty.Text);
            }
        }

        public void LoadPGSections()
        {
            LoadPGNameInfo();
        }
        List<string> tp07_lb01_List = new List<string>();
        NameValueCollection keylist34 = new NameValueCollection();
        NameValueCollection keylistGP2 = new NameValueCollection();
        public bool LoadPGNameInfo()
        {
            string filename = PGNamePath;            
            SetupIni ini = new SetupIni();
            ini.SetFileName(filename);
            if (!File.Exists(filename))
            {
                WriteDefaultPGName(filename);
            }
            StringCollection sectionlist = new StringCollection();
            StringCollection sectionlist2 = new StringCollection();
            ini.ReadSectionValues("34", keylist34);
            ini.ReadSectionValues("GP2", keylistGP2);
            ini.ReadSection("34", sectionlist);
            ini.ReadSection("GP2", sectionlist2);
            tp07_lb01.Items.Clear();
            try
            {
                for (int i = 0; i < sectionlist.Count; i++)
                {
                    tp07_lb01.Items.Add(sectionlist[i]);
                    tp07_lb01_List.Add(sectionlist[i]);
                }
                tp07_lb01.Font = new System.Drawing.Font("微軟正黑體", 14, FontStyle.Bold);               
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        private void tp07_tb01_EditValueChanged(object sender, EventArgs e)
        {
            //tp07_lb01.SelectedIndex = -1;
            tp07_lb01.Items.Clear();
            if (tp07_tb01.Text == "")
            {
                for (int i = 0; i < tp07_lb01_List.Count; i++)
                {
                    tp07_lb01.Items.Add(tp07_lb01_List[i]);
                }
                tp07_lb01.SelectedIndex = -1;    
                return;
            }
            for (int i = 0; i < tp07_lb01_List.Count; i++)
            {
                int j = tp07_lb01_List[i].ToString().IndexOf(tp07_tb01.Text);
                if (j>=0)
                {
                    tp07_lb01.Items.Add(tp07_lb01_List[i]);
                }
            }
            for (int i = 0; i < keylist34.Count; i++)
            {
                int j = keylist34[i].ToString().IndexOf(tp07_tb01.Text);
                if (j >= 0)
                {
                    tp07_lb01.Items.Add(keylist34[i]);
                }
            }
            for (int i = 0; i < keylistGP2.Count; i++)
            {
                int j = keylistGP2[i].ToString().IndexOf(tp07_tb01.Text);
                if (j >= 0)
                {
                    tp07_lb01.Items.Add(keylistGP2[i]);
                }
            }
        }

        private void tp07_lb01_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tp07_lb01.SelectedIndex == -1)
            {
                tp07_tb02.Text = "";
                tp07_tb04.Text = "";
                tp07_tb03.Text = "";
                tp07_tb05.Text = "";

                return;
            }
            string filename = PGNamePath;
            string SectoinName = "34";
            SetupIni ini = new SetupIni();
            ini.SetFileName(filename);
            tp07_tb02.Text = ini.ReadString("34", tp07_lb01.Text, "");
            tp07_tb04.Text = tp07_lb01.Text;
            tp07_tb03.Text = ini.ReadString("GP2", tp07_lb01.Text, "");
            tp07_tb05.Text = tp07_lb01.Text;

            if (tp07_tb02.Text == "")
            {
                for (int i = 0; i < keylist34.Count; i++)
                {
                    if (keylist34[i] == tp07_tb04.Text)
                    {
                        tp07_tb02.Text = keylist34.Keys[i];
                        tp07_tb04.Text = keylist34[i];//ini.ReadString("34", tp07_tb02.Text, ""); ;
                    }
                }
            }

            if (tp07_tb03.Text == "")
            {
                for (int i = 0; i < keylistGP2.Count; i++)
                {
                    if (keylistGP2.GetValues(i)[0] == tp07_tb05.Text)
                    {
                        tp07_tb03.Text = keylistGP2.Keys[i];
                        tp07_tb05.Text = keylistGP2[i];// ini.ReadString("GP2", tp07_tb03.Text, ""); ;
                    }
                }
            }

            if (tp07_tb02.Text == "" &&  tp07_tb03.Text != "")
            {
               tp07_tb04.Text = ini.ReadString("34", tp07_tb03.Text, "");
               if (tp07_tb04.Text != "")
               {
                   for (int i = 0; i < keylist34.Count; i++)
                   {
                       if (keylist34[i] == tp07_tb04.Text)
                       {
                           tp07_tb02.Text = keylist34.Keys[i];
                       }
                   }
               }
            }

            if (tp07_tb03.Text == "" && tp07_tb02.Text != "")
            {
                tp07_tb05.Text = ini.ReadString("GP2", tp07_tb02.Text, "");
                if (tp07_tb05.Text != "")
                {
                    for (int i = 0; i < keylistGP2.Count; i++)
                    {
                        if (keylistGP2[i] == tp07_tb05.Text)
                        {
                            tp07_tb03.Text = keylistGP2.Keys[i];
                        }
                    }
                }
            }
        }
        private void WriteDefaultPGName(string fileName)
        {
            if (!Directory.Exists(JsonDir))
            {
                Directory.CreateDirectory(JsonDir); 
            }
            File.Create(fileName).Close();

            try
            {
                StreamWriter w = new StreamWriter(@fileName, false, System.Text.Encoding.Default);
                string[] sr = Split("\r\n", Properties.Resources.PGName);
                for (int i = 0; i < sr.Length; i++)
                {
                    w.WriteLine(sr[i]);
                }
                w.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("存檔失敗!!\r\n" + ex.Message);
            }
        }
        //QR
        private void QRA()
        {/*

            qrControl.Text = "QrCode.Net";

            BitMatrix qrMatrix = qrControl.GetQrMatrix(); //Qr bit matrix for input string "QrCode.Net".

            qrControl.Lock();  //Lock class.
            qrControl.ErrorCorrectLevel = ErrorCorrectionLevel.M;  //It won't encode and repaint.
            qrControl.Text = textEdit1.Text;
            qrMatrix = qrControl.GetQrMatrix(); //Qr bit matrix for input string "QrCode.Net".            
            qrControl.UnLock(); //Unlock class, re-encode and repaint. 

            qrControl.Freeze(); //Freeze class.
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            QRA();

        }

        private void TEST1_Click(object sender, EventArgs e)
        {
           /* // 创建接收套接字
            serverIp = IPAddress.Parse("127.0.0.1");
            serverIPEndPoint = new IPEndPoint(serverIp, int.Parse("4096"));            
            receiveUdpClient = new UdpClient(serverIPEndPoint);
            // 启动接收线程
            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();*/
            //Pool = new JObject();
           // timer_AutoUpdate.Interval = Int32.Parse(tb_Seconds.Text) *1000;
           // timer_AutoUpdate.Enabled = true;
            tcpListener = new TcpListener(IPAddress.Any, 4097);

            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();           
        }
        private void ListenForClients()
        {
            this.tcpListener.Start();            
            int m = 0;
            int c = 0;
            while (true)
            {
                while (!tcpListener.Pending())
                {                   
                    Thread.Sleep(1000);
                }
                ConnectionThread cli = new ConnectionThread(tcpListener, richTextBox1,btn_LoadPool);
            }
        }
        private void TEST2_Click(object sender, EventArgs e)
        {
           /* string s = "1,NSD,NSD_PIG";
            string value = "False";
            AddNode(s, value);
            string[] s = Split("\"", CurrentNode.Text);
            CurrentNode.Tag = tb_RValue.Text;
            CurrentNode.Text = CurrentNode.nodename + ":\"" + CurrentNode.Tag + "\"";
            tb_VALUE.Text = tb_RValue.Text;*/
            
        }

        private void AddNode(string path, string value)
        {
            FoundNode = null;
            FindNodes2(MyNode, path, value);
            if (FoundNode == null)
            {
                string[] s = path.Split(',');
                AddNode2(s,value);
            }
        }

        private void AddNode2(string[] snode, string value)        
        {
            int i=0;
            TreeNodeEx a = MyNode;
            string s = "";
            s = snode[i]; 
            while (i<snode.Length)
            {               
                if (a.Nodes.ContainsKey(s))
                {
                    a = (TreeNodeEx)a.Nodes[s];
                }
                else
                {
                    TreeNodeEx b = new TreeNodeEx();
                    b.nodename = snode[i];
                    b.Name = snode[i];
                    b.index = index++;
                    for (int j=0;j<=i;j++)
                    {
                        b.path += snode[j];
                        if (j != i)
                        {
                            b.path +=  ",";
                        }
                    }
                    
                    if (i != snode.Length - 1) //節點
                    {
                        b.IsHaveChild = true;
                        b.Tag = "";
                        b.Text = b.nodename;
                    }
                    else
                    {
                        b.IsHaveChild = false;
                        b.Tag = value;
                        b.Text += b.nodename + " : \"" + value + "\"";
                        b.value = value;
                        b.ImageIndex = 2;
                        b.SelectedImageIndex = 2;
                    }                   
                    a.Nodes.Add(b);
                    a = (TreeNodeEx)a.Nodes[b.Name];
                }
                i++;
                if (i < snode.Length)
                {
                    s = snode[i];
                }
            }             
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(MyNode);
            treeView1.Nodes[0].Expand();
        }
        public static void ParseJsonProperties(JObject jObject, string paramName)
        {
            IEnumerable<JProperty> jObject_Properties = jObject.Properties();

            // Build list of valid property and object types 

            JsonTokenType[] validPropertyValueTypes = { JsonTokenType.String, JsonTokenType.Integer, JsonTokenType.Float, JsonTokenType.Boolean, JsonTokenType.Null, JsonTokenType.Date };
            List<JsonTokenType> propertyTypes = new List<JsonTokenType>(validPropertyValueTypes);

            JsonTokenType[] validObjectTypes = { JsonTokenType.String, JsonTokenType.Array, JsonTokenType.Object };
            List<JsonTokenType> objectTypes = new List<JsonTokenType>(validObjectTypes);

            string currentParamName = paramName; //Need to track where we are.

            foreach (JProperty property in jObject_Properties)
            {
                paramName = currentParamName;

                try
                {
                    if (propertyTypes.Contains(property.Value.Type))
                    {
                        string stmp = "";
                        if (paramName == "")
                            stmp = "";
                        else
                            stmp = paramName + ",";

                        ParseJsonKeyValue(property, stmp + property.Name.ToString());
                    }
                    else if (objectTypes.Contains(property.Value.Type))
                    {
                        //Arrays ex. { names: ["first": "John", "last" : "doe"]}
                        if (property.Value.Type == JsonTokenType.Array && property.Value.HasValues)
                        {
                            ParseJsonArray(property, paramName);
                        }

                        //Objects ex. { name: "john"}
                        if (property.Value.Type == JsonTokenType.Object)
                        {
                            JObject jo = new JObject();
                            jo = JObject.Parse(property.Value.ToString());
                            paramName = property.Name.ToString();

                            jsonKeyValues.Add(paramName, "");//property.Value.ToString());

                            if (jo.HasValues)
                            {
                                ParseJsonProperties(jo, paramName);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            } // End of ForEach

            paramName = currentParamName;

        }
        public static void ParseJsonKeyValue(JProperty item, string paramName)
        {
            jsonKeyValues.Add(paramName, item.Value.ToString());
        }
        public static void ParseJsonArray(JProperty item, string paramName)
        {
            JArray jArray = (JArray)item.Value;

            if (paramName == "")
                paramName = item.Name.ToString();
            else            
                paramName = paramName + "," + item.Name.ToString();
            jsonKeyValues.Add(paramName, item.Value.ToString());

            string currentParamName = paramName; //Need track where we are

            try
            {
                for (int i = 0; i < jArray.Count(); i++)
                {
                    paramName = currentParamName;
                    
                    paramName = i.ToString();
                    jsonKeyValues.Add(paramName, jArray.Values<object>().ElementAt(i).ToString());

                    JObject jo = new JObject();
                    jo = JObject.Parse(jArray[i].ToString());
                    IEnumerable<JProperty> jArrayEnum = jo.Properties();

                    foreach (JProperty jaItem in jArrayEnum)
                    {
                        // Prior to JSON.NET VER 5.0, there was no Path property on JTokens. So we had to track the path on our own.
                        var paramNameWithJaItem = jaItem.Name.ToString();

                        var itemValue = jaItem.Value.ToString();
                        if (itemValue.Length > 0)
                        {
                            switch (itemValue.Substring(0, 1))
                            {
                                case "[":
                                    //Recusion call to itself
                                    ParseJsonArray(jaItem, paramNameWithJaItem);
                                    break;
                                case "{":
                                    //Create a new JObject and parse
                                    JObject joObject = new JObject();
                                    joObject = JObject.Parse(itemValue);

                                    //For this value, reparse from the top
                                    ParseJsonProperties(joObject, paramNameWithJaItem);
                                    break;
                                default:
                                    ParseJsonKeyValue(jaItem, paramNameWithJaItem);
                                    break;
                            }
                        }
                    }
                } //end for loop

                paramName = currentParamName;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        class ConnectionThread
        {
            public TcpListener threadListener;
            private static int connections = 0;
            private static int connThread = 0;
            private static int countMsg = 0;
            public RichTextBox rtb = null;
            public Button btn = null;
            private byte[] data = new byte[2] { 79, 75 };
            public MemoryStream MStream = null;
            public bool IsDone = false;      
            //public ConnectionThread(TcpListener lis, RichTextBox xrtb, JObject varPool, Button xbtn)
            public ConnectionThread(TcpListener lis, RichTextBox xrtb, Button xbtn)
            {
                threadListener = lis;
                rtb = xrtb;
                btn = xbtn;            
                //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClientComm), varPool);
                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClientComm), Pool);
            }

            public void HandleClientComm(object state)
            {
                TcpClient tcpClient = null;
                NetworkStream clientStream = null;
                try
                {
                    tcpClient = threadListener.AcceptTcpClient();
                    clientStream = tcpClient.GetStream();
                }
                catch (System.Exception ex)
                {
                    
                }                
                 
                connThread++;
                int bytesRead;
                while (true)
                {
                    bytesRead = 0;
                    string output = "";
                    try
                    {

                        if (clientStream.CanRead)
                        {
                            byte[] message = new byte[8192];
                            do
                            {
                                bytesRead = clientStream.Read(message, 0, message.Length);
                                ASCIIEncoding encoder = new ASCIIEncoding();
                                output = encoder.GetString(message, 0, bytesRead);
                            } while (clientStream.DataAvailable);
                            clientStream.Write(data, 0, data.Length);
                            if (output.Trim() != "")
                            {
                                JObject r = JObject.Parse(output);
                                string mOptions = "";
                                if (r["Options"] != null)
                                {
                                    mOptions = (string)r["Options"];
                                    if (mOptions == "GetPool")
                                    {
                                        if (r["Value"] != null)
                                        {
                                            if ((string)r.Property("Value").Value == "OK")
                                            {
                                                GetPool(btn);
                                            }
                                        }
                                    }
                                    else
                                        //ShowListView(r, rtb, (JObject)state); 
                                        ShowListView(r, rtb); 
                                }                              
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                if (tcpClient !=null)
                {
                    tcpClient.Close();
                }               
                connThread--;
            }
            private delegate void GetPoolDelegate(Button btn);
            private void GetPool(Button btn)
            {
                if (btn.InvokeRequired)
                {
                    GetPoolDelegate removedelegate = GetPool;
                    btn.Invoke(removedelegate, btn);
                }
                else
                {
                    btn.PerformClick();
                    IsCanUpdate = true;
                }
            }
            //private delegate void ShowListViewDelegate(JObject output, RichTextBox xrtb, JObject varPool);
            //private void ShowListView(JObject output, RichTextBox xrtb, JObject varPool)
            private delegate void ShowListViewDelegate(JObject output, RichTextBox xrtb);
            private void ShowListView(JObject output, RichTextBox xrtb)
            {
                connections++;
                if (xrtb.InvokeRequired)
                {
                    ShowListViewDelegate removedelegate = ShowListView;
                    //xrtb.Invoke(removedelegate, output, xrtb, varPool);                   
                    xrtb.Invoke(removedelegate, output, xrtb);                   
                }
                else
                {
                    try
                    {
                        JArray mStratum = null;
                        string mOptions = "";
                        JToken mValue = null;                       
                        JObject r = output;
                        string stratum = "";
                        string[] mtmp = null;
                        if (r["Options"] != null)
                        {
                            mOptions = (string)r["Options"];
                        }
                        if (mOptions == "SetStratumValue")
                        {
                            if (r["Stratum"] != null)
                            {
                                mStratum = (JArray)r["Stratum"];
                            }
                            if (r["Value"] != null)
                            {
                                mValue = r.Property("Value").Value;
                            }
                            mtmp = new string[mStratum.Count()];

                            for (int i = 0; i < mStratum.Count(); i++)
                            {
                                mtmp[i] = ((JValue)mStratum[i]).Value.ToString();
                            }
                            
                            //SetStratumValue(varPool, mtmp, mValue);                                                        
                            SetStratumValue(Pool, mtmp, mValue);                                                        
                        }
                        else if (mOptions == "DeleteByName")
                        {
                            if (r["Stratum"] != null)
                            {
                                mStratum = (JArray)r["Stratum"];
                                stratum = (string)((JValue)mStratum[0]).Value.ToString();
                                //DeleteByName(varPool, ((JValue)mStratum[0]).Value.ToString());                               
                                DeleteByName(Pool, ((JValue)mStratum[0]).Value.ToString());                               
                            }                            
                        }
                        else if (mOptions == "DeleteByStratum")
                        {
                            if (r["Stratum"] != null)
                            {
                                mStratum = (JArray)r["Stratum"];
                                mtmp = new string[mStratum.Count()];
                                for (int i = 0; i < mStratum.Count(); i++)
                                {
                                    mtmp[i] = ((JValue)mStratum[i]).Value.ToString();
                                }
                                //DeleteByStratum(varPool, mtmp);  
                                DeleteByStratum(Pool, mtmp);  
                            }
                        }
                        else if (mOptions == "SendAllToMoitor")
                        {

                        }
                        else if (mOptions == "GetPool")
                        {
                            if (r["Value"] != null)
                            {
                                mValue = r.Property("Value").Value;
                                if (mValue.ToString() == "OK")
                                {

                                }
                            }
                        }

                        //string rstr = r.ToString().Replace("\r\n", "");
                        //countMsg++;
                        //Console.WriteLine(countMsg.ToString());
                        //if (countMsg % 50 == 0)
                        if (rtb.Lines.Count() >= 14)
                        {
                            rtb.Text = "";
                        }
                        //rtb.Text += "[" + countMsg.ToString() + "]" + rstr +"\r\n";
                        //if (IsLogToTxt)
                        {
                            if (stratum == "")
                            {
                                for (int i = 0; i < mStratum.Count(); i++)
                                {
                                    stratum += "'" + (string)((JValue)mStratum[i]).Value.ToString() + "',";
                                }
                            }                            
                            string mstr = "";
                            stratum = stratum.Substring(0, stratum.Length - 1);
                            mstr = " {'" + mOptions.Substring(0, 1);
                            mstr += "',[" + stratum + "],'";
                            mstr += (string)((JValue)mValue).Value.ToString() + "'}";
                            countMsg++;
                            rtb.Text += "[" + countMsg.ToString() + "]" + mstr + "\r\n";
                            
                            if (IsLogToTxt)
                            WriteTotxt(mstr);
                        }
                        
                        IsCanUpdate = true;
                    }
                    catch (System.Exception ex)
                    {
                        WriteTotxt(ex.Message);
                    }
                }
            }
            private void DeleteByName(JObject Owner, string name)
            {
                bool mBool = false;
                JProperty mobj2;
                mBool = ExistOfName(Owner, name,out mobj2);
                if (mBool)
                {
                    Owner[mobj2.Name].Remove();
                }
            }
            private void DeleteByStratum(JObject Owner, string[] Stratum)
            {
                bool mBool = false;
                bool GODel = false;
                JProperty mobj2 = null;
                JProperty mobj3 = null;
                JObject mobj = Owner;


                for (int i = 0; i < Stratum.Length;i++ )
                {
                    mBool = ExistOfName(mobj, Stratum[i], out mobj2);
                    if (mBool)
                    {
                        GODel = true;
                        mobj3 = mobj.Property(Stratum[i]);
                        mobj = (JObject)mobj3.Value;
                        //mobj = (JObject)mobj.Property(Stratum[i]).Value;                        
                    }
                    else
                    {
                        return;
                    }
                }

                if (GODel)
                {
                    mobj3.Remove();
                }
            }
            private void SetStratumValue(JObject Owner, string[] Stratum, JToken Value)
            {
                int max;
                JObject mobj, mobj1;
                JProperty mobj2;
                bool mBool = false;
                JProperty mPar;
                if (Stratum.Length == 0) return;

                mobj = Owner;
                max = Stratum.Length - 1;
                for (int i = 0; i < max; i++)
                {
                    mBool = ExistOfName(mobj, Stratum[i],out mobj2);
                    if (!mBool)
                    {
                        mobj1 = new JObject();
                        mobj.Add(new JProperty(Stratum[i], mobj1));
                        mobj = mobj1;
                    }
                    else
                    {
                        mPar = new JProperty(mobj.Property(Stratum[i]).Name,mobj.Property(Stratum[i]).Value);
                        if (mPar.Value.Type == JsonTokenType.Object)
                        {
                            mobj = (JObject)mobj.Property(Stratum[i]).Value;
                            //mobj = (JObject)mPar.Value;
                        }
                        else
                        {
                            mobj1 = new JObject();
                            mPar.Value = mobj1;
                            mobj = mobj1;
                        }
                    }
                }

                mBool = ExistOfName(mobj, Stratum[max], out mobj2);
                if (!mBool)
                {
                    mobj.Add(new JProperty(Stratum[max], Value));
                    mBool = false;
                }
                else
                {
                    mobj.Property(Stratum[max]).Value = Value;
                }

            }
            private bool ExistOfName(JObject Owner, string Name, out JProperty item)
            {
                bool result = false;
                item = null;
                IEnumerable<JProperty> jObject_Properties = Owner.Properties();                
                foreach (JProperty j in jObject_Properties)
                {
                    if (j.Name == Name)
                    {
                        item = j;
                        result = true;
                        break;
                    }
                }
                return result;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SendMsgToPOS(0);
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
            if (listenThread != null)
            {
                listenThread.Abort();
            }            
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void cboListen_CheckedChanged(object sender, EventArgs e)
        {
            int i = 0;
            if (cboListen.Checked)
            {
                i = 1;
            }
            SendMsgToPOS(i);
        }
        private void tb_GetJsonAll_Click(object sender, EventArgs e)
        {
            SendMsgToPOS(2);
        }
        private void SendMsgToPOS(int Msg)
        {
            IntPtr hwnd = FindWindow("DSC_POSMainAppName", "POS");
            if (hwnd != IntPtr.Zero)
            {
                IntPtr childHwnd = FindWindowEx(hwnd, IntPtr.Zero, "TfamPOSVI01", null); //获得按钮的句柄
                if (childHwnd != IntPtr.Zero)
                {
                    SendMessage(hwnd, 2592, Msg, 0); //发送点击按钮的消息
                }
                else
                {
                    MessageBox.Show("没有找到POSVI01S");
                }
            }
            else
            {
                MessageBox.Show("没有找到POSMAINGP");
            }
        }
        private bool SendMsgToPOS(uint Msg,int type)
        {
            IntPtr hwnd = FindWindow("DSC_POSMainAppName", "POS");
            if (hwnd != IntPtr.Zero)
            {
                IntPtr childHwnd = FindWindowEx(hwnd, IntPtr.Zero, "TfamPOSVI01", null); //获得按钮的句柄
                if (childHwnd != IntPtr.Zero)
                {
                    SendMessage(hwnd, 2592, Msg, type); //发送点击按钮的消息
                    return true;
                }
                else
                {
                    MessageBox.Show("没有找到POSVI01S");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("没有找到POSMAINGP");
                return false;
            }
        }
        private void cboAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            //timer_AutoUpdate.Enabled = cboAutoUpdate.Checked;
            timer_AutoUpdate.Interval = Int32.Parse(tb_Seconds.Text)*1000;
        }
        private void timer_AutoUpdate_Tick(object sender, EventArgs e)
        {
            if (cboAutoUpdate.Checked)
            {
                if (IsCanUpdate)
                {
                    IsCanUpdate = false;
                    btn_UpdatePool.PerformClick();
                    tP01_btnReFresh.PerformClick();
                }
            }            
        }
        private static void WriteTotxt(string str)
        {            
            // 建立檔案串流（@ 可取消跳脫字元 escape sequence）
            StreamWriter sw = new StreamWriter(@JsonDir + string.Format("{0:yyyyMMdd}", DateTime.Now)+".txt",true);
            str = "[" + string.Format("{0:00000}", LogLines++) + "]" + str;
            sw.WriteLine(str);            // 寫入文字
            sw.Close();                   // 關閉串流
        }

        private void chkLogToTxt_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void btn_DelTxt_Click(object sender, EventArgs e)
        {

        }
        private void btn_Opentxt_Click(object sender, EventArgs e)
        {

        }
        private void ShowMsg(string msg)
        {
            lbl_Msg.Text = msg;
            lbl_Msg.ForeColor = Color.Red;

        }

        private void tm_Msg_Tick(object sender, EventArgs e)
        {
            lbl_Msg.Text = "";
        }

        private void btn_Lock_Click(object sender, EventArgs e)
        {
            if (!Checktp02_IsValueExist(tb_PATH.Text))
            {
                uint s = GlobalAddAtom(tb_PATH.Text);
                if (SendMsgToPOS(s,9))
                {
                    AddRowToDGV03(tb_PATH.Text);
                }                
            }                        
        }
        private bool Checktp02_IsValueExist(string s)
        {
            bool IsExist = false;
            string[] tmp = s.Split(',');
            
            for (int j = 0; j < dgv03.RowCount;j++ )            
            {
                if (tmp.Length == Int32.Parse(dgv03.Rows[j].Cells[0].Value.ToString()))
                {
                    for (int i = 0; i < tmp.Length;i++ )
                    {
                        if (tmp[i] == dgv03.Rows[j].Cells[i + 1].Value.ToString())
                        {
                            if (i==tmp.Length-1)
                            {
                                IsExist = true;
                                return IsExist;
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }                
            }
            return IsExist;
        }
        private void AddRowToDGV03(string s)
        {
            string[] tmp = s.Split(',');
            string[] tmp1 = new string[tmp.Length + 1];
            tmp1[0] = tmp.Length.ToString();
            for (int i = 1; i < tmp1.Length; i++)
            {
                tmp1[i] = tmp[i - 1];
            }
            dgv03.Rows.Add(tmp1);
            dgv03.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; 
        }
        private void tp02_btnDel_Click(object sender, EventArgs e)
        {
            if (DGV03_SelectIndex == -1)
            {
                return;
            }            
            string tmp = "";
            int count = Int32.Parse(dgv03.Rows[DGV03_SelectIndex].Cells[0].Value.ToString());
            for (int i = 1; i < count; i++)
            {
                tmp += dgv03.Rows[DGV03_SelectIndex].Cells[i].Value.ToString() + ",";
            }
            tmp = tmp.Substring(0, tmp.Length - 1);
            uint s = GlobalAddAtom(tb_PATH.Text);
            if (SendMsgToPOS((uint)DGV03_SelectIndex, 8))
            {
                dgv03.Rows.RemoveAt(DGV03_SelectIndex);
            }                        
        }
        private void dgv03_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DGV03_SelectIndex = e.RowIndex;
        }

        private void btn_ClearLog_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void btn_InitPool_Click(object sender, EventArgs e)
        {
            Pool = JObject.Parse("{}");
            btn_UpdatePool.PerformClick();
        }

        private void tp02_btnEmpty_Click(object sender, EventArgs e)
        {
            if (SendMsgToPOS((uint)DGV03_SelectIndex, 7))
            {
                dgv03.Rows.Clear();                
            }   
        }
    }


}
