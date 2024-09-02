using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMoresamplerForTuneLab
{
    internal class Moresample_Flags
    {
        public Dictionary<string, int> FlagDict { get; set; } = new Dictionary<string, int>();

        public void Parse(string Flags)
        {
            string fg = Flags;
            int i = 0;
            while (i < fg.Length)
            {
                if (fg[i] == 'M')
                {
                    i++;
                    if (fg[i]=='t')
                    {
                        AddValue("Mt", ref i);
                    }else if (fg[i] == 'b')
                    {
                        AddValue("Mb", ref i);
                    }
                    else if (fg[i] == 'o')
                    {
                        AddValue("Mo", ref i);
                    }
                    else if (fg[i] == 'r')
                    {
                        AddValue("Mr", ref i);
                    }
                    else if (fg[i] == 'd')
                    {
                        AddValue("Md", ref i);
                    }
                    else if (fg[i] == 'C')
                    {
                        AddValue("MC", ref i);
                    }
                    else if (fg[i] == 'G')
                    {
                        AddValue("MG", ref i);
                    }
                    else if (fg[i] == 'D')
                    {
                        AddValue("MD", ref i);
                    }
                    else if(fg[i] == 's')
                    {
                        AddValue("Ms", ref i);
                    }
                    else if(fg[i] == 'm')
                    {
                        AddValue("Mm", ref i);
                    }
                    else if(fg[i] == 'E')
                    {
                        AddValue("ME", ref i);
                    }
                    else if(fg[i] == 'e')
                    {
                        FlagDict.Add("Me", 1);
                        if(FlagDict.ContainsKey("e"))
                        {
                            FlagDict.Remove("e");
                        }
                    }
                }
                else if (fg[i] == 'g')
                {
                    AddValue("g", ref i);
                }
                else if (fg[i] == 't')
                {
                    AddValue("t", ref i);
                }
                else if (fg[i] == 'P')
                {
                    AddValue("P", ref i);
                }
                else if (fg[i] == 'A')
                {
                    AddValue("A", ref i);
                }
                else if (fg[i] == 'b')
                {
                    AddValue("B", ref i);                
                }
                else if (fg[i] == 'e')
                {
                    if(!FlagDict.ContainsKey("Me"))FlagDict.Add("e", 1);
                }
                else if (fg[i] == 'u')
                {
                    FlagDict.Add("u", 1);
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
            if (FlagDict.ContainsKey("e") && FlagDict.ContainsKey("Me")) FlagDict.Remove("e");
            foreach(var kv in FlagDict)
            {
                if(kv.Value!=0 && (kv.Key=="e" || kv.Key == "Me" || kv.Key == "u"))
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
