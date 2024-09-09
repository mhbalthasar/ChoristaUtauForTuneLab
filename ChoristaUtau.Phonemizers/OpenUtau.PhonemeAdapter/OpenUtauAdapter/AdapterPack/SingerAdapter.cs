using ChoristaUtauApi.UVoiceBank;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter
{
    public class ChoristaOto(Oto choristaOto,VoiceBank vb) : UOto
    {
        public override string Alias => choristaOto.Alias;
        public override string Phonetic => choristaOto.Alias;
        public override string Set => "";
        public override string Color => "";
        public override string Prefix => "";
        public override string Suffix => "";
        public override SortedSet<int> ToneSet => new SortedSet<int>([60]);
        public override string File => choristaOto.GetWavfilePath(vb.vbBasePath);
        public override string DisplayFile => File;
        public override double Offset
        {
            get => offset;
            set
            {
                offset = Math.Max(0, Math.Round(value, 3));
            }
        }
        public override double Consonant
        {
            get => consonant;
            set
            {
                consonant = Math.Max(0, Math.Round(value, 3));
            }
        }
        public override double Cutoff
        {
            get => cutoff;
            set
            {
                cutoff = Math.Round(value, 3);
            }
        }
        public override double Preutter
        {
            get => preutter;
            set
            {
                preutter = Math.Max(0, Math.Round(value, 3));
            }
        }
        public override double Overlap
        {
            get => overlap;
            set
            {
                overlap = Math.Round(value, 3);
            }
        }
        public override List<string> SearchTerms { get => new List<string>(); }

        public override event PropertyChangedEventHandler PropertyChanged;

        private Oto oto;
        private double offset;
        private double consonant;
        private double cutoff;
        private double preutter;
        private double overlap;
        
        public override string ToString() => Alias;
    }

    public class ChoristaSinger(VoiceBank choristaVB) : USinger
    {
        
        protected static readonly List<UOto> emptyOtos = new List<UOto>();

        public override string Id => choristaVB.CacheHash;
        public override string Name => choristaVB.Name;
        public override Dictionary<string, string> LocalizedNames => new Dictionary<string, string>();
        public override USingerType SingerType => USingerType.Classic;
        public override string BasePath => choristaVB.vbBasePath;
        public override string Author => "";
        public override string Voice => "";
        public override string Location => choristaVB.vbBasePath;
        public override string Web => "";
        public override string Version => "";
        public override string OtherInfo => "";
        public override IList<string> Errors => new List<string>();
        public override string Avatar => "";
        public override byte[] AvatarData => new byte[0];
        public override string Portrait => "";
        public override float PortraitOpacity => 10.0f;
        public override int PortraitHeight => 10;
        public override string Sample => "";
        public override string DefaultPhonemizer => "default";
        public override Encoding TextFileEncoding => Encoding.UTF8;
        public override IList<USubbank> Subbanks => new List<USubbank>();

        public bool Found => found;
        public bool Loaded => found && loaded;
        public bool OtoDirty
        {
            get => otoDirty;
            set
            {
                otoDirty = value;
                NotifyPropertyChanged(nameof(OtoDirty));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool found;
        protected bool loaded;
        protected bool otoDirty;

        private string name;

        public override void EnsureLoaded() { }
        public override void Reload() { }
        public override void Save() { }
        public override bool TryGetOto(string phoneme, out UOto oto)
        {
            var o=choristaVB.FindSymbol(phoneme, 60);
            oto = null;
            if (o == null) return false;
            oto = new ChoristaOto(o,choristaVB);
            return true;
        }
        public override bool TryGetMappedOto(string phoneme, int tone, out UOto oto)
        {
            var o = choristaVB.FindSymbol(phoneme, tone);
            oto = null;
            if (o == null) return false;
            oto = new ChoristaOto(o, choristaVB);
            return true;
        }
        public override bool TryGetMappedOto(string phoneme, int tone, string color, out UOto oto)
        {
            var o = choristaVB.FindSymbol(phoneme, tone);
            oto = null;
            if (o == null) return false;
            oto = new ChoristaOto(o, choristaVB);
            return true;
        }

        public override IEnumerable<UOto> GetSuggestions(string text) { return emptyOtos; }
        public override byte[] LoadPortrait() => null;
        public override byte[] LoadSample() => null;
        public override string ToString() => "";
        public bool Equals(USinger other)
        {
            // Tentative: Since only the singer's Id is recorded in ustx and preferences, singers with the same Id are considered identical.
            // Singer with the same directory name in different locations may be identical.
            if (other != null && other.Id == this.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode() => Id.GetHashCode();

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Some types of singers store their data in memory when rendering.
        /// This method is called when the singer is no longer used.
        /// Note:
        /// - the voicebank may be used again even after this method is called.
        /// - this method may be called even when the singer has not been used
        /// </summary>
        public override void FreeMemory() { }
    }
}
