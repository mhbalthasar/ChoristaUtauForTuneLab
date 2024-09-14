using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDoppeltlerForTuneLab
{
    internal class Doppeltler_Flags
    {
        public Dictionary<string, int> FlagDict { get; set; } = new Dictionary<string, int>();

        public void Parse(string Flags)
        {
            string fg = Flags;
            int i = 0;
            while (i < fg.Length)
            {
                if (fg[i] == 'g')
                {
                    AddValue("g", ref i);
                }
                else if (fg[i] == 'b')
                {
                    AddValue("B", ref i);
                }
                else if (fg[i] == 'P')
                {
                    AddValue("P", ref i);
                }
                else if (fg[i] == 'N')
                {
                    FlagDict.Add("N", 1);
                }
                else if (fg[i] == 'a')
                {
                    AddValue("a", ref i);
                }
                else if (fg[i] == 'i')
                {
                    AddValue("i", ref i);
                }
                else if (fg[i] == 'v')
                {
                    AddValue("v", ref i);
                }
                else if (fg[i] == 't')
                {
                    AddValue("t", ref i);
                }
                i++;
            }
            void AddValue(string FlagKey,ref int i)
            {
                string val = "";
                while (i + 1 < fg.Length && (fg[i + 1] == '-' || (fg[i + 1] <= '9' && fg[i + 1] >= '0')))
                {
                    i++;
                    val = val + fg[i];
                }
                if (val.Length > 0) FlagDict.Add(FlagKey, int.Parse(val));
            }
        }
        public override string ToString()
        {
            string ret = "";
            foreach(var kv in FlagDict)
            {
                if(kv.Value!=0 && (kv.Key=="N"))
                {
                    ret += kv.Key;
                    continue;
                }
                ret += kv.Key + kv.Value.ToString();
            }
            return ret;
        }
    }
}
