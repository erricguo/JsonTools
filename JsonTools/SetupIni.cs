using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Specialized;

namespace JsonTools
{
    public class SetupIni
    {
        public string path;
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal,
        int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, 
        string key, string def, byte[] retVal, int size, string filePath);

        public void IniWriteValue(string Section, string Key, string Value, string inipath)
        {
            WritePrivateProfileString(Section, Key, Value, inipath);
        }
        public string IniReadValue(string Section, string Key, string inipath)
        {
            StringBuilder temp = new StringBuilder(65536);
            int i = GetPrivateProfileString(Section, Key, "", temp, 65536, inipath);
            return temp.ToString();
        }
        public void WriteString(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }
        public void WriteInteger(string Section, string Key, int Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), path);
        }
        public void WriteBool(string Section, string Key, bool Value)
        {
            string tmp = "";
            if (Value) tmp = "1"; else tmp = "0";            
            WritePrivateProfileString(Section, Key, tmp, path);
        }
        public void SetFileName(string xfilename)
        {
            path = xfilename;
        }
        public string ReadString(string Section, string Key, string Default)
        {
            StringBuilder temp = new StringBuilder(65536);
            int i = GetPrivateProfileString(Section, Key, Default, temp, 65536, path);
            return temp.ToString();
        }
        public int ReadInteger(string Section, string Key, int Default)
        {
            StringBuilder temp = new StringBuilder(65536);
            string s = ReadString(Section, Key, "");
            int i = Default;
            if (s != "")
            {
                i = Int32.Parse(s);
            }
            try
            {
                return i;
            }
            catch (System.Exception ex)
            {
                return Default;
            }
        }
        public bool ReadBool(string Section, string Key, bool Default)
        {
            StringBuilder temp = new StringBuilder(65536);
            int n = 0;
            if (Default) n = 1; else n=0;
            int i = ReadInteger(Section, Key, n);
            try
            {
                return (i != 0);
            }
            catch (System.Exception ex)
            {
                return Default;
            }
        }
        public void ReadSection(string Section, StringCollection Idents)
        {
            Byte[] Buffer = new Byte[65536];
            //Idents.Clear();

            int bufLen = GetPrivateProfileString(Section, null, null, Buffer, Buffer.GetUpperBound(0),
             path);
            //对Section进行解析
            GetStringsFromBuffer(Buffer, bufLen, Idents);
        }
        private void GetStringsFromBuffer(Byte[] Buffer, int bufLen, StringCollection Strings)
        {
            Strings.Clear();
            if (bufLen != 0)
            {
                int start = 0;
                for (int i = 0; i < bufLen; i++)
                {
                    if ((Buffer[i] == 0) && ((i - start) > 0))
                    {
                        String s = Encoding.GetEncoding(0).GetString(Buffer, start, i - start);
                        Strings.Add(s);
                        start = i + 1;
                    }
                }
            }
        }
        //读取指定的Section的所有Value到列表中
       public void ReadSectionValues(string Section,NameValueCollection Values)
　　　　{
　　　　　　StringCollection KeyList = new StringCollection();
　　　　　　ReadSection(Section, KeyList);
　　　　　　Values.Clear();
           int i = 0;
　　　　　　foreach (string key in KeyList)
　　　　　　{
　　　　　   if (i==28)
            {
                string k = "";
                k = "";
            }
　
          Values.Add(key, ReadString(Section, key, ""));
                i++;
　　　　　　}
     
　　　　}
　　　　
    }
    public class DBINFO
    {
        public string DBType;
        public string ServerIP;
        public string DBName;
        public string UserID;
        public string UserPass;
        public string DSCSYS;
    }
    public class MACHINEPARAM
    {
       public string BAS_CompNo;
       public string BAS_CompShortName;
       public string BAS_StoreNo;
       public string BAS_StoreShortName;
       public string BAS_POSNO;
       public string BAS_Stamp_StoreName;
       public string BAS_Stamp_Manager;
       public string BAS_Stamp_CompName;
       public string BAS_Stamp_CompAddress;
       public string BAS_Stamp_CompPhone;
       public string BAS_Stamp_CompUniID;
    }

}
