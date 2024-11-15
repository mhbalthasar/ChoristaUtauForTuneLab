using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UtaubaseForTuneLab.AudioEffect
{
    internal class SwitcherManager
    {
        public static bool IsAudioEffectEnable { get; set; } = false;
        public static void UpdateSwitchers()
        {
            try
            {
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!Directory.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"))) Directory.CreateDirectory(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"));
                if (File.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml")))//openutau
                {
                    try
                    {
                        YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                        Dictionary<object, object> ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml")));
                        if (ouMap.TryGetValue("audio_effects_enable", out object Value))
                        {
                            IsAudioEffectEnable = ((string)Value).ToLower() == "true";
                        }
                    }
                    catch {; }
                }
            }
            catch {; }
        }

        public static void SaveSwitchers()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!Directory.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"))) Directory.CreateDirectory(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"));

            Dictionary<object, object> ouMap = new Dictionary<object, object>();
            if (File.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml")));
                }
                catch {; }
            }
            if (IsAudioEffectEnable)
            {
                if (ouMap.ContainsKey("text_file_encoding"))
                    ouMap["audio_effects_enable"] = "true";
                else
                    ouMap.Add("audio_effects_enable", "true");
            }
            else
            {
                if (ouMap.ContainsKey("audio_effects_enable")) ouMap.Remove("audio_effects_enable");
            }
            try
            {
                if (ouMap.Count == 0)
                {
                    if (File.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml")))
                    {
                        File.Delete(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml"));
                    }
                }
                else
                {
                    YamlDotNet.Serialization.Serializer s = new YamlDotNet.Serialization.Serializer();
                    string content = s.Serialize(ouMap);
                    File.WriteAllText(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "switchers.yaml"), content);
                }
            }
            catch {; }
        }
    }
}
