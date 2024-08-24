using UtauSharpApi.Utils;
using UtauSharpApi.UVoiceBank;

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

        string str=OctaveUtils.NoteNumber2Str(60);
        int num = OctaveUtils.Str2NoteNumber(str);

        VoiceBank rr = UVoiceBankLoader.LoadVoiceBank(@"F:\F\G\VocalUtau\VocalUtau\bin\Debug\voicedb\uta");// @"F:\DF\DFZZ_U");
        string fn = rr.Otos[0].GetWavfilePath(@"F:\F\G\VocalUtau\VocalUtau\bin\Debug\voicedb\uta");
        bool fb = Path.Exists(fn);
        Console.WriteLine("Hello, World!");
    }
}