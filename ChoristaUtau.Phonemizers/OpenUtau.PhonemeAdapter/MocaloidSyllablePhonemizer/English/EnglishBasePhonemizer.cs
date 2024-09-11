using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using NumSharp.Utilities;
using OpenUtau.Api;
using OpenUtau.Core.G2p;
using OpenUtau.Plugin.Builtin;
using Serilog;

namespace MocaloidSyllablePhonemizer.English
{
    public class MocaloidEnglishBasePhonemizer : SyllableBasedPhonemizer
    {

        private readonly string[] vowels = ("a,@,u,0,8,I,e,3,A,i,E,O,Q,6,o,1ng,9,&,x,1,Y,L,W"+ "Q@,V,{,e@,@r,@U,eI,u:,aI,i:,O:,I@,aU").Split(",");
        private readonly string[] consonants = "b,ch,d,dh,f,g,h,j,k,l,m,n,ng,p,r,s,sh,t,th,v,w,y,z,zh,dd,hh,sp,st".Split(",");

        private readonly Dictionary<string, string> dictionaryReplacements = ("aa=a;ae=@;ah=u;ao=9;aw=8;ay=I;" +
        "b=b;ch=ch;d=d;dh=dh;eh=e;er=3;ey=A;f=f;g=g;hh=h;hhy=hh;ih=i;iy=E;jh=j;k=k;l=l;m=m;n=n;ng=ng;ow=O;oy=Q;" +
        "p=p;r=r;s=s;sh=sh;t=t;th=th;uh=6;uw=o;v=v;w=w;y=y;z=z;zh=zh;dx=dd;").Split(';')
        .Select(entry => entry.Split('='))
        .Where(parts => parts.Length == 2)
        .Where(parts => parts[0] != parts[1])
        .ToDictionary(parts => parts[0], parts => parts[1]);

        protected override string[] GetVowels() => vowels;
        protected override string[] GetConsonants() => consonants;
        protected override string GetDictionaryName() => "cmudict-0_7b.txt";
        protected override IG2p LoadBaseDictionary()
        {
            var g2ps = new List<IG2p>();
            string path = Path.Combine(PluginDir, "envccv.yaml");
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(PluginDir);
                File.WriteAllBytes(path, Resources.Resource.envccv_template);
            }
            g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(path)).Build());
            g2ps.Add(new ArpabetG2p());
            return new G2pFallbacks(g2ps.ToArray());
        }
        protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => dictionaryReplacements;


        Dictionary<string, Dictionary<string, int>> vccv2voc = new Dictionary<string, Dictionary<string, int>>()
        {
            {"u",new Dictionary<string,int>(){{"V",37558},{"I",5402},{"e",4149},{"Q",3974},{"U",2290},{"eI",103},{"@U",78},{"u:",68},{"{",42},{"aI",29},{"i:",17},{"t",14},{"r",11},{"O:",10},{"l",10},{"n",7},{"s",6},{"g",4},{"O@",2},{"@r",2},{"Q@",2},{"v",1},{"b",1},{"e@",1},{"m",1},{"I@",1},{"k",1},{"z",1}}},
            {"A",new Dictionary<string,int>(){{"eI",9454},{"V",16},{"{",6},{"r",5},{"i:",5},{"Q",5},{"aI",3},{"e@",2},{"@r",1},{"e",1},{"I",1}}},
            {"a",new Dictionary<string,int>(){{"Q",13809},{"Q@",966},{"V",247},{"{",43},{"e@",21},{"i:",13},{"@U",12},{"O:",9},{"m",8},{"k",8},{"r",5},{"h",5},{"p",4},{"eI",3},{"aI",2},{"t",2},{"g",1},{"O@",1},{"@r",1},{"n",1},{"dZ",1},{"e",1},{"l",1},{"I",1},{"S",1},{"s",1}}},
            {"3",new Dictionary<string,int>(){{"@r",23234},{"r",15},{"V",6},{"O@",4},{"e@",4},{"I",3},{"Q@",3},{"Q",2},{"h",1},{"i:",1},{"aI",1},{"I@",1}}},
            {"th",new Dictionary<string,int>(){{"T",1861},{"D",19},{"t",1}}},
            {"e",new Dictionary<string,int>(){{"e@",2250},{"{",128},{"I",50},{"V",29},{"i:",21},{"eI",18},{"r",6},{"I@",4},{"s",2},{"v",2},{"aI",2},{"n",2},{"h",1},{"l",1},{"u:",1},{"Q@",1},{"U",1},{"@U",1},{"O:",1},{"z",1}}},
            {"@",new Dictionary<string,int>(){{"{",16061},{"V",40},{"r",22},{"Q",13},{"e@",12},{"eI",7},{"I",3},{"O:",1},{"I@",1}}},{"O",new Dictionary<string,int>(){{"@U",14332},{"Q",68},{"V",28},{"r",11},{"h",2},{"f",1},{"u:",1},{"O@",1},{"aU",1},{"I",1},{"O:",1}}},
            {"dd",new Dictionary<string,int>(){{"t",5318},{"d",2812},{"{",1}}},
            {"E",new Dictionary<string,int>(){{"i:",25622},{"I",33},{"I@",18},{"eI",16},{"r",13},{"e",9},{"aI",8},{"V",7},{"j",2},{"O@",1},{"w",1},{"n",1}}},
            {"i",new Dictionary<string,int>(){{"I",34565},{"I@",824},{"e",792},{"V",385},{"i:",58},{"aI",19},{"t",12},{"eI",12},{"r",9},{"O@",7},{"Q",7},{"l",4},{"s",4},{"z",3},{"u:",3},{"{",2},{"n",2},{"f",1},{"v",1},{"k",1}}},
            {"ng",new Dictionary<string,int>(){{"N",7337},{"n",3},{"I",2},{"O:",1},{"dZ",1}}},
            {"sh",new Dictionary<string,int>(){{"S",5926},{"tS",60},{"s",2},{"Q",1},{"i:",1}}},
            {"8",new Dictionary<string,int>(){{"aU",2344},{"f",1},{"Q",1}}},
            {"o",new Dictionary<string,int>(){{"u:",6782},{"V",13},{"U@",4},{"@U",2},{"U",2},{"r",2},{"@r",1},{"h",1},{"g",1},{"aU",1},{"w",1},{"j",1}}},
            {"k",new Dictionary<string,int>(){{"kh",9381},{"V",7},{"I",7},{"@U",6},{"g",5},{"tS",5},{"s",4},{"t",3},{"S",3},{"e",2},{"h",1},{"O:",1},{"Q",1},{"i:",1},{"{",1},{"u:",1}}},
            {"s",new Dictionary<string,int>(){{"z",34},{"t",33},{"n",15},{"S",11},{"V",6},{"k",6},{"g",2},{"l",1},{"@r",1},{"aI",1},{"{",1},{"O:",1},{"tS",1}}},
            {"9",new Dictionary<string,int>(){{"O:",3534},{"O@",1551},{"Q",61},{"V",29},{"f",10},{"p",7},{"@U",7},{"Q@",6},{"b",4},{"k",3},{"t",3},{"aU",3},{"h",2},{"r",2},{"@r",2},{"n",1},{"d",1},{"l",1},{"i:",1},{"m",1},{"ph",1},{"I",1}}},
            {"I",new Dictionary<string,int>(){{"aI",7991},{"i:",15},{"w",7},{"r",4},{"eI",3},{"O@",2},{"d",2},{"V",2}}},
            {"t",new Dictionary<string,int>(){{"th",2911},{"s",10},{"aI",7},{"T",3},{"eI",2},{"V",2},{"dh",1},{"i:",1},{"tS",1},{"k",1},{"kh",1},{"I",1},{"d",1}}},
            {"j",new Dictionary<string,int>(){{"dZ",4253},{"g",6},{"z",1},{"h",1},{"N",1}}},
            {"l",new Dictionary<string,int>(){{"l0",4356},{"e",8},{"i:",4},{"V",3},{"U",3},{"I",2},{"k",1},{"@U",1},{"s",1}}},
            {"p",new Dictionary<string,int>(){{"ph",5628},{"s",10},{"f",2},{"e",2},{"aI",1},{"z",1},{"V",1}}},
            {"ch",new Dictionary<string,int>(){{"tS",3243},{"h",6},{"S",4},{"t",4},{"kh",3},{"k",1},{"s",1}}},
            {"zh",new Dictionary<string,int>(){{"Z",389},{"gh",2},{"z",1},{"g",1}}},
            {"y",new Dictionary<string,int>(){{"j",3488},{"i:",13},{"dZ",3},{"V",2},{"r",2},{"z",1},{"h",1},{"eI",1},{"aI",1}}},
            {"dh",new Dictionary<string,int>(){{"D",377},{"T",3}}},
            {"6",new Dictionary<string,int>(){{"U",1073},{"U@",446},{"O@",6},{"V",5},{"u:",2},{"r",1}}},
            {"Q",new Dictionary<string,int>(){{"OI",861}}},
            {"z",new Dictionary<string,int>(){{"s",23},{"V",4},{"n",2},{"i:",1},{"I",1}}},
            {"r",new Dictionary<string,int>(){{"Q@",46},{"t",37},{"O@",34},{"b",24},{"d",23},{"k",13},{"@r",11},{"s",9},{"e@",7},{"I@",7},{"m",6},{"l",4},{"n",4},{"p",3},{"g",3},{"eI",2},{"f",2},{"O:",1},{"U@",1},{"V",1},{"z",1},{"S",1}}},
            {"dr",new Dictionary<string,int>(){{"r",23},{"dh",6},{"d",6}}},
            {"tr",new Dictionary<string,int>(){{"r",95},{"t",40},{"th",36}}},
            {"b",new Dictionary<string,int>(){{"bh",6997},{"I",22},{"e",2},{"@U",2},{"u:",1},{"s",1}}},
            {"g",new Dictionary<string,int>(){{"gh",3501},{"N",3},{"dZ",2},{"V",2},{"{",2},{"h",1},{"f",1},{"d",1},{"Z",1},{"k",1},{"z",1},{"s",1}}},
            {"f",new Dictionary<string,int>(){{"s",7},{"t",3},{"e",1},{"I",1},{"k",1},{"kh",1},{"p",1}}},
            {"n",new Dictionary<string,int>(){{"{",17},{"t",7},{"N",5},{"V",3},{"I",3},{"d",2},{"m",1},{"s",1},{"@U",1},{"i:",1},{"O:",1}}},
            {"m",new Dictionary<string,int>(){{"V",4},{"d",3},{"s",3},{"e",2},{"I",1},{"i:",1},{"z",1},{"Q",1},{"n",1}}},
            {"d",new Dictionary<string,int>(){{"dh",5290},{"eI",3},{"s",3},{"V",1},{"I",1},{"t",1}}},
            {"w",new Dictionary<string,int>(){{"h",7},{"v",2},{"f",1},{"u:",1},{"r",1}}},
            {"h",new Dictionary<string,int>(){{"t",4},{"l",2},{"aI",2},{"e",1},{"g",1},{"f",1},{"n",1}}},
            {"v",new Dictionary<string,int>(){{"f",2},{"@U",2},{"Q",1},{"i:",1}}}
        };
        private string[] ReplaceVV(string inputVV)
        {
            if (vccv2voc.ContainsKey(inputVV)) return vccv2voc[inputVV].Keys.ToArray();
            return [];
        }
        Dictionary<string, string> cache = new Dictionary<string, string>();
        private string ReplaceAndFillSymbols(string inputV1, string inputV2)
        {
            string key = inputV1 + " " + inputV2;
            if (cache.ContainsKey(key)) return cache[key];
            List<string> v1l = new List<string>() { inputV1 };
            List<string> v2l = new List<string>() { inputV2 };
            v1l.AddRange(ReplaceVV(inputV1));
            if (inputV2 != "Sil" && inputV2 != "") v2l.AddRange(ReplaceVV(inputV2));
            foreach (string v1 in v1l)
            {
                foreach (string v2 in v2l)
                {
                    string symbol = (v1 + " " + v2).Trim();
                    if (HasOto(symbol, 60))
                    {
                        if(!cache.ContainsKey(key))cache.Add(key, symbol);
                        return symbol;
                    }
                }
            }
            return "";
        }
        protected override List<string> ProcessSyllable(Syllable syllable)
        {
            string prevV = syllable.prevV;
            string[] cc = syllable.cc;
            string[] PreviousWordCc = syllable.PreviousWordCc;
            string[] CurrentWordCc = syllable.CurrentWordCc;
            string v = syllable.v;
            var lastC = cc.Length - 1;
            var lastCPrevWord = syllable.prevWordConsonantsCount;

            List<string> vowel = new List<string>();
            if (prevV != "") vowel.Add(prevV);
            vowel.AddRange(cc);
            if (v != "") vowel.Add(v);

            List<string> ret = new List<string>();
            if (vowel.Count == 1)
            {
                var sym = ReplaceAndFillSymbols(vowel[0], "");
                if (sym != "") ret.Add(sym);
            }
            else for (int i = 1; i < vowel.Count; i++)
                {
                    var sym = ReplaceAndFillSymbols(vowel[i - 1], vowel[i]);
                    if (sym != "") ret.Add(sym);
                }
            return ret;
        }

        protected override List<string> ProcessEnding(Ending ending)
        {
            string[] cc = ending.cc;
            string v = ending.prevV;
            var lastC = cc.Length - 1;

            var phonemes = new List<string>();
            // --------------------------- ENDING V ------------------------------- //
            if (ending.IsEndingV)
            {
                // try V- else no ending
                var sym = ReplaceAndFillSymbols(v, "Sil");
                if (sym != "") phonemes.Add(sym);
            }
            else if (cc.Length > 0)
            {
                var sym = ReplaceAndFillSymbols(v, cc.Last());
                if (sym != "") phonemes.Add(sym);
                sym = ReplaceAndFillSymbols(cc.Last(), "Sil");
                if (sym != "") phonemes.Add(sym);
            }
            // ---------------------------------------------------------------------------------- //

            return phonemes;
        }


        protected override string ValidateAlias(string alias)
        {
            //foreach (var consonant in new[] { "h" }) {
            //    alias = alias.Replace(consonant, "hh");
            //}
            foreach (var consonant in new[] { "6r" })
            {
                alias = alias.Replace(consonant, "3");
            }

            return alias;
        }
    }
}
