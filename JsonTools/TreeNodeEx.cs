using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JsonTools
{
    class TreeNodeEx:TreeNode
    {
        public string nodename;
        public string path;
        public int index;
        public bool IsHaveChild;
        public string SelectPath;
        public string value;
        public SortedDictionary <string ,string>Tsdc;
    }
}
