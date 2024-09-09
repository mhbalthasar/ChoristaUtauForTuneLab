

using ChoristaUtauApi.UPhonemizer.Presamp;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;

internal class Program
{
    private static void Main(string[] args)
    {
        /*UVoiceBank uVoiceBank = new UVoiceBank();
        uVoiceBank.FileEncodingName = "GBK";
        uVoiceBank.OtoSets.Add(new OtoSet());
        uVoiceBank.OtoSets[0].Name = "F#";
        uVoiceBank.OtoSets[0].Otos.Add(new Oto());
        uVoiceBank.OtoSets[0].Otos[0].Cutoff = 100;
        uVoiceBank.OtoSets[0].Otos[0].Alias = "BB";
        uVoiceBank.OtoSets[0].Otos[0].Offset = 13;
        uVoiceBank.Serialize("Test.bin");

        UVoiceBank rr = UVoiceBank.Deserialize("Test.bin");*/
        /*var vb = UVoiceBankLoader.LoadVoiceBank("F:\\DF\\DFZZ_U");
        string preFile = "F:\\DF\\DFZZ_U\\presamp.ini";
        var p=Presamp.ParsePresamp(preFile);
        var m=new PresampSpliter(p,vb);
        UMidiPart up = new UMidiPart();
        var ap=m.SplitCVVC(
            new ChoristaUtauApi.UNote.UMidiNote(up) { Lyric="chang",DurationMSec=120},
            new ChoristaUtauApi.UNote.UMidiNote(up) { Lyric = "cheng", DurationMSec = 120 },
            new ChoristaUtauApi.UNote.UMidiNote(up) { Lyric="wai",DurationMSec=120}
);*/
        var c = "p";
    }
}