using SettingBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SettingBuilder_win64
{
    public partial class EncodingSetting : Form
    {
        Dictionary<string, Tuple<string, string>> TPK;
        MainForm parent;
        List<string> EncodingNames = new List<string>();
        public EncodingSetting(Dictionary<string, Tuple<string, string>> tpk,MainForm parent)
        {
            this.parent = parent;
            TPK = tpk;
            InitializeComponent();
            //txtEncoding.
            EncodingNames = CodePagesEncodingProvider.Instance.GetEncodings().Select(p => p.Name.ToUpper()).Order().ToList();
            EncodingNames.InsertRange(0, ["AutoDetect", "UTF8", "UTF16", "Unicode"]);
        }

        private void EncodingSetting_Load(object sender, EventArgs e)
        {
            singerList.Items.Clear();
            singerList.Items.AddRange(TPK.Values.Select(p => (object)p.Item1).ToArray());
            txtEncoding.Items.Clear();
            txtEncoding.Items.AddRange(EncodingNames.Select(p => (object)p).ToArray());
            singerList.SelectedIndex = 0;
        }

        private void singerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            object av = singerList.SelectedItems[0];
            if (av != null)
            {
                string cstr = (string)av;
                KeyValuePair<string, Tuple<string, string>>? cr = null;
                try
                {
                    cr=TPK.Where(p => p.Value.Item1 == cstr).First();
                }
                catch {; }
                if (cr.HasValue)
                {
                    string path = cr.Value.Key;
                    txtPath.Text = path;
                    txtPath.ReadOnly = true;
                    txtName.ReadOnly = true;
                    string rln = cr.Value.Value.Item1;
                    string enc = cr.Value.Value.Item2
                        .Replace("shift-jis", "shift_jis");
                    if (enc.Length == 0) txtEncoding.SelectedIndex = 0;
                    else if (enc.ToLower() == "utf8" || enc.ToLower() == "utf-8") txtEncoding.SelectedIndex = 1;
                    else if (enc.ToLower() == "ascii") txtEncoding.SelectedIndex = 0;
                    else if (enc.ToLower() == "unicode") txtEncoding.SelectedIndex = 3;
                    else
                    {
                        var cp = txtEncoding.Items.IndexOf(enc.ToUpper());
                        if (cp == -1) txtEncoding.Text = enc.ToUpper();
                        else 
                            txtEncoding.SelectedIndex = cp;
                    }
                    parent.GetVBOverlayName(txtPath.Text, out string? VN);
                    if (VN != null) overlayName.Text = VN; else overlayName.Text = "";
                    txtEncoding_SelectedIndexChanged(null, null);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.Invoke(() => { button1.Enabled = false; });
                this.Invoke(() =>
                {
                    SaveItem();
                    int curIndex = singerList.SelectedIndex;
                    TPK = parent.GetVoiceBanks(); 
                    singerList.Items.Clear();
                    singerList.Items.AddRange(TPK.Values.Select(p => (object)p.Item1).ToArray());
                    singerList.SelectedIndex = curIndex < TPK.Count ? curIndex : 0;
                });

                this.Invoke(() => { button1.Enabled = true; });
            });
        }

        private void txtEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            parent.GetVBNameWithEncoding(txtPath.Text, txtEncoding.Text, out string? enN, out string? VN);
            if (VN != null)
            {
                txtName.Text = VN;
            }else
            {
                txtName.Text = "";
            }
        }

        void SaveItem()
        {
            string VoiceBankPath = txtPath.Text;
            Dictionary<object, object> ouMap = new Dictionary<object, object>();
            if (File.Exists(Path.Combine(VoiceBankPath, "character.yaml")))//openutau
            {
                try
                {
                    YamlDotNet.Serialization.Deserializer d = new YamlDotNet.Serialization.Deserializer();
                    ouMap = (Dictionary<object, object>)d.Deserialize(File.ReadAllText(Path.Combine(VoiceBankPath, "character.yaml")));
                }
                catch {; }
            }
            if (txtEncoding.SelectedIndex != 0 && txtEncoding.Text.ToLower() != "autodetect")
            {
                string enc = txtEncoding.Text.ToLower();
                if (enc == "shift_jis") enc = "shift-jis";
                if (ouMap.ContainsKey("text_file_encoding"))
                    ouMap["text_file_encoding"] = enc;
                else
                    ouMap.Add("text_file_encoding", enc);
            }
            else
            {
                if (ouMap.ContainsKey("text_file_encoding")) ouMap.Remove("text_file_encoding");
            }
            if (overlayName.Text.Trim() != "")
            {
                string tnn = overlayName.Text.Trim();
                if (ouMap.ContainsKey("name"))
                    ouMap["name"] = tnn;
                else
                    ouMap.Add("name", tnn);
            }
            else
            {
                if (ouMap.ContainsKey("name")) ouMap.Remove("name");
            }
            try
            {
                if (ouMap.Count == 0)
                {
                    if(File.Exists(Path.Combine(VoiceBankPath, "character.yaml")))
                    {
                        File.Delete(Path.Combine(VoiceBankPath, "character.yaml"));
                    }
                }
                else
                {
                    YamlDotNet.Serialization.Serializer s = new YamlDotNet.Serialization.Serializer();
                    using (FileStream fs = new FileStream(Path.Combine(VoiceBankPath, "character.yaml"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (TextWriter tw = new StreamWriter(fs))
                        {
                            s.Serialize(tw, ouMap);
                        }
                    };
                }
            }
            catch {; }
        }
    }
}
