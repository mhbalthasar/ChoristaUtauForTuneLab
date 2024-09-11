using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoristaUtauApi.Utils;

namespace ChoristaUtauApi.UVoiceBank
{
    [ProtoContract]
    public class PrefixItem
    {
        [ProtoMember(1)]
        public string prefix { get; set; } = "";
        [ProtoMember(2)]
        public string suffix { get; set; } = "";
        [ProtoMember(3)]
        public int PitchNumber { get; set; } = 0;
    }
    [ProtoContract]
    public class PrefixPair
    {
        [ProtoMember(1)]
        public string Key { get; set; } = "";
        [ProtoMember(2)]
        public PrefixItem PrefixItem { get; set; }= new PrefixItem();
    }

    [ProtoContract]
    public class VoiceBank
    {
        [ProtoMember(1)]
        public string CacheHash { get; set; } = "";

        [ProtoMember(2)]
        public string Name { get; set; } = "";

        [ProtoMember(3)]
        public string DefaultLyric { get; set; } = "";

        [ProtoMember(4)]
        public List<PrefixItem> PrefixMap { get; set; } = new List<PrefixItem>();

        [ProtoMember(5)]
        public List<Oto> Otos { get; set; } = new List<Oto>();

        [ProtoMember(6)]
        public List<PrefixPair> PrefixPairs { get; set; } = new List<PrefixPair>();


        public Oto? FindSymbol(string symbol, PrefixItem? prefix=null)
        {
            if (prefix == null) prefix = new PrefixItem();
            Oto? ret = Otos.Find(p=>p.Alias==prefix.prefix+symbol+prefix.suffix);
            if (ret == null) ret = Otos.Find(p => p.Alias == symbol);
            return ret;
        }
        public Oto? FindSymbol(string symbol, int PrefixKeyNumber)
        {
            PrefixItem prefix = GetPrefixItem(PrefixKeyNumber-12);//UtauNoteNumber is Higher than Vocaloid 1 Octave.
            Oto? ret= Otos.Find(p => p.Alias == prefix.prefix + symbol + prefix.suffix);
            if (ret == null) ret = Otos.Find(p=>p.Alias==symbol);
            return ret;
        }

        public PrefixItem GetPrefixItem(int NoteNumber)
        {
            return PrefixMap.Where(p => p.PitchNumber == NoteNumber).FirstOrDefault(new PrefixItem());
        }

        public void SetPrefixItem(int NoteNumber,string prefix,string suffix)
        {
            PrefixItem? item=PrefixMap.Where(p => p.PitchNumber == NoteNumber).FirstOrDefault();
            if (item == null)
            {
                PrefixMap.Add(new PrefixItem() { PitchNumber = NoteNumber,suffix=suffix,prefix=prefix }) ;
            }else
            {
                foreach (var p in PrefixMap.Where(p => p.PitchNumber == NoteNumber)) PrefixMap.Remove(p);
                PrefixMap.Add(new PrefixItem() { PitchNumber = NoteNumber, suffix = suffix, prefix = prefix });
            }
        }
        public void SetPrefixPairs(Dictionary<string, PrefixItem> Pairs)
        {
            PrefixPairs.Clear();
            foreach (var kv in Pairs)
            {
                PrefixPairs.Add(new PrefixPair() { Key = kv.Key, PrefixItem = kv.Value });
            }
        }

        public List<string> GetPrefixPairs()
        {
            return PrefixPairs.Select(p => p.Key).ToList();
        }
        public PrefixItem? GetPrefixPairItem(string PairKey)
        {
            PrefixPair? item = PrefixPairs.Where(p => p.Key == PairKey).FirstOrDefault();
            if (item != null) return item.PrefixItem;
            return null;
        }

        public void Serialize(string TargetFile)
        {
            using (var file = File.Create(TargetFile))
            {
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
                if (FileEncoding=="")return Wav;
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
