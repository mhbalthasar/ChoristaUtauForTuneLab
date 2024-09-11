using System.ComponentModel.Design.Serialization;

namespace SettingBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        List<string> l_staticDirs = new List<string>();
        List<string> l_searchDirs = new List<string>();
        string settingFilePath = "";
        void Reload()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            l_staticDirs.Clear();
            l_staticDirs.Add(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedb"));
            l_staticDirs.Add(Path.Combine(UserProfile, "utauvbs"));

            l_searchDirs.Clear();
            if (!Directory.Exists(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"))) Directory.CreateDirectory(Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau"));
            settingFilePath = Path.Combine(UserProfile, ".TuneLab", "ChoristaUtau", "voicedirs.txt");
            if (File.Exists(settingFilePath))
                l_searchDirs.AddRange(File.ReadAllLines(settingFilePath).Select(p => p.Trim()).Where(p => p.Length > 0 && Directory.Exists(p)));
            searchDirs.Items.Clear();
            foreach (string dir in l_staticDirs)
            {
                searchDirs.Items.Add(dir);
            }
            foreach (string dir in l_searchDirs)
            {
                searchDirs.Items.Add(dir);
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            CalcSize();
            Reload();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowPinnedPlaces = true;
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var itm = fbd.SelectedPath;
                if (!Directory.Exists(itm)) return;
                if (l_staticDirs.Contains(itm)) return;
                if (l_searchDirs.Contains(itm)) return;
                l_searchDirs.Add(itm);
                if (l_searchDirs.Contains(itm))
                {
                    searchDirs.Items.Add(itm);
                    SaveChange();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int idx = searchDirs.SelectedIndex;
            if (idx < 0) return;
            if (idx < searchDirs.Items.Count)
            {
                string itm = (string)searchDirs.Items[idx];
                if (l_staticDirs.Contains(itm)) return;
                if (!l_searchDirs.Contains(itm)) return;
                l_searchDirs.Remove(itm);
                if (!l_searchDirs.Contains(itm))
                {
                    searchDirs.Items.Remove(itm);
                    SaveChange();
                }
            }
        }

        void SaveChange()
        {
            File.WriteAllLines(settingFilePath, l_searchDirs);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Reload();
        }
        public static long GetFolderSize(string folderPath)
        {
            // ����ļ����Ƿ����
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"�ļ��в�����: {folderPath}");
            }

            // ��ȡ�ļ����е������ļ������ļ���
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            long totalSize = 0;

            // ���������ļ����ۼ��ļ���С
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

            return totalSize;
        }
        private void CalcSize()
        {
            if (lab_size.Text == "Size Calcing...") return;
            string tmpPath = Path.Combine(Path.GetTempPath(), "ChoristaUtau");
            Task.Run(() =>
            {
                this.Invoke(() =>
                {
                    lab_size.Text = "Size Calcing...";
                });
                long size = 0;
                try
                {
                    size = GetFolderSize(tmpPath);
                }
                catch {; }
                long sizeMB = size / 1024 / 1024;
                this.Invoke(() =>
                {
                    lab_size.Text = string.Format("{0} MB", sizeMB);
                });
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), "ChoristaUtau");
            Task.Run(() =>
            {
                this.Invoke(() =>
                {
                    button4.Enabled = false;
                });
                try
                {
                    DeleteDirectory(tmpPath);
                }
                catch {; }
                CalcSize();
                this.Invoke(() =>
                {
                    button4.Enabled = true;
                });
            });
        }

        public static void DeleteDirectory(string targetDir)
        {
            // ���Ŀ¼�Ƿ����
            if (!Directory.Exists(targetDir))
            {
                throw new DirectoryNotFoundException($"Ŀ¼������: {targetDir}");
            }

            // ��ȡĿ¼�е������ļ�����Ŀ¼
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            // ɾ�������ļ�
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal); // ȷ���ļ�����ֻ����
                File.Delete(file);
            }

            // �ݹ�ɾ��������Ŀ¼
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            // ɾ����Ŀ¼
            Directory.Delete(targetDir, false);
        }

        private List<string> FindVB(string DirPath, int SearchDeeply = 10)
        {
            List<string> ret = new List<string>();
            if (!Directory.Exists(DirPath)) return ret;
            if (File.Exists(Path.Combine(DirPath, "character.txt")))
            {
                ret.Add(DirPath);
                return ret;
            }
            if (SearchDeeply == 0) return ret;
            foreach (string subDir in Directory.GetDirectories(DirPath))
            {
                ret.AddRange(FindVB(subDir, SearchDeeply - 1));
            }
            return ret;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.Invoke(() => { button5.Enabled = false; button6.Enabled = false; });
                List<string> allPath = new List<string>();
                allPath.AddRange(l_staticDirs);
                allPath.AddRange(l_searchDirs);

                List<string> VBPaths = new List<string>();
                foreach (string path in allPath) VBPaths.AddRange(FindVB(path, 10));

                foreach (string str in VBPaths)
                {
                    string tmpFile = Path.Combine(str, "uvoicebank.protobuf");
                    if (File.Exists(tmpFile)) try
                        {
                            File.Delete(tmpFile);
                        }
                        catch { }
                }
                this.Invoke(() => { button5.Enabled = true; button6.Enabled = true; });
            });
        }

        private void button6_Click(object sender, EventArgs e)
        {

            Task.Run(() =>
            {
                this.Invoke(() => { button5.Enabled = false; button6.Enabled = false; });
                List<string> allPath = new List<string>();
                allPath.AddRange(l_staticDirs);
                allPath.AddRange(l_searchDirs);

                List<string> VBPaths = new List<string>();
                foreach (string path in allPath) VBPaths.AddRange(FindVB(path, 10));

                foreach (string str in VBPaths)
                {
                    string tmpFile = Path.Combine(str, "phonemizer.txt");
                    if (File.Exists(tmpFile)) try
                        {
                            File.Delete(tmpFile);
                        }
                        catch { }
                }
                this.Invoke(() => { button5.Enabled = true; button6.Enabled = true; });
            });
        }
    }
}
