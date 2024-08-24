using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.Utils;

namespace UtauSharpApi.UVoiceBank
{
    [ProtoContract]
    public class PrefixItem
    {
        [ProtoMember(1)]
        public string prefix { get; set; } = "";
        [ProtoMember(2)]
        public string suffix { get; set; } = "";
    }

    [ProtoContract]
    public class VoiceBank
    {
        [ProtoMember(1)]
        public string CacheHash { get; set; } = "";

        [ProtoMember(3)]
        public string Name { get; set; } = "";

        [ProtoMember(4)]
        public string DefaultLyric { get; set; } = "";

        [ProtoMember(5)]
        public List<Oto> Otos { get; set; } = new List<Oto>();

        [ProtoMember(6)]
        public PrefixItem[] PrefixMap { get; set; } = new PrefixItem[128];

        public Oto? FindSymbol(string symbol, PrefixItem? prefix=null)
        {
            if (prefix == null) prefix = new PrefixItem();
            return Otos.Find(p=>p.Alias==prefix.prefix+symbol+prefix.suffix);
        }
        public Oto? FindSymbol(string symbol, int NoteNumber)
        {
            PrefixItem? prefix = null;
            if (NoteNumber <= 127) prefix = PrefixMap[NoteNumber];
            if (prefix == null) prefix=new PrefixItem();
            return Otos.Find(p => p.Alias == prefix.prefix + symbol + prefix.suffix);
        }

        public void Serialize(string TargetFile)
        {
            using (var file = File.Create(TargetFile))
            {
                for(int i = 0; i < PrefixMap.Length; i++) if (PrefixMap[i] == null) PrefixMap[i]=new PrefixItem();
                ProtoBuf.Serializer.Serialize(file, this);
            }
        }
        public static VoiceBank? Deserialize(string FromFile)
        {
            using (var file = File.OpenRead(FromFile))
            {
                return ProtoBuf.Serializer.Deserialize<VoiceBank>(file);
            }
            return null;
        }

        public string vbBasePath { get; set; } = "";
    }

    [ProtoContract]
    public class Oto
    {
        [ProtoMember(1)]
        public string Alias { get; set; }="";
        [ProtoMember(2)]
        public string Wav { get; set; } = "";
        [ProtoMember(3)]
        public List<string> BaseDirs { get; set; } = new List<string>();

        [ProtoMember(4)]
        public double Offset { get; set; } = 0;
        [ProtoMember(5)] 
        public double Consonant { get; set; } = 0;
        [ProtoMember(6)]
        public double Cutoff { get; set; } = 0;
        [ProtoMember(7)]
        public double Preutter { get; set; } = 0;
        [ProtoMember(8)]
        public double Overlap { get; set; } = 0;
        
        [ProtoMember(9)]
        public string FileEncoding { get; set; } = "Shift-JIS";

        public string LocalWav { get
            {
                var SrcEncoding = EncodingUtils.GetEncoding(FileEncoding);
                var DstEncoding = EncodingUtils.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
                byte[] orgWav=SrcEncoding.GetBytes(Wav);
                string localWav= DstEncoding.GetString(orgWav);
                return localWav;
            } 
        }

        public string GetWavfilePath(string VoiceBankDir="")
        {
            string pdir = Path.Combine(BaseDirs.ToArray());
            if ((VoiceBankDir.Length>0))
            {
                return Path.Combine(VoiceBankDir, pdir,LocalWav);
            }
            return Path.Combine(pdir, LocalWav);
        }
        public bool isVaild(string VoiceBankDir = "") { return File.Exists(GetWavfilePath(VoiceBankDir)); }

        public override string ToString()
        {
            return Alias;
        }
        public static Oto GetR { get; private set; } = new Oto() { Alias = "R", Wav = "R" };
    }

}
