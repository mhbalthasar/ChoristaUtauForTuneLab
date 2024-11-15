using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ChoristaUtau.SettingUI.Kernel.Utils;
using ImGuiNET;
using ImGuiNET.SDL2CS;
using SDL2;

namespace ChoristaUtau.SettingUI
{

    public class ImGuiWindow : ImWindowExt_CJK
    {
        private bool _showAlertWindow = false;
        private string _textPopupWindow = "";
        private bool _showConfirmWindow = false;
        private Action<object> _actionConfirmWindow = null;
        private object _objectConfirmWindow = null;

        private void Alert(string content)
        {
            _textPopupWindow = content;
            _showAlertWindow = true;
        }
        private void Confirm(string content, Action<object> action, object obj)
        {
            _textPopupWindow = content;
            _showConfirmWindow = true;
            _actionConfirmWindow = action;
            _objectConfirmWindow = obj;
        }

        static ImGuiWindowFlags win_flags = ImGuiWindowFlags.None;
        static int win_width = 740;
        static int win_height = 680;

        public ImGuiWindow() : base(Mode.Single, "", null, win_width, win_height, win_flags)
        {
            InitFont();
            Title = Worker.GuiTitle;
            BackgroundColor = new Vector4(0, 0, 0, 0);
            Task.Run(() => { Worker.UpdateVoiceDirs(); });
            Task.Run(() => { Worker.UpdateCacheSize(); });
            SwitcherManager.UpdateSwitchers();
            mAction = SubmitUI;
        }
        void InitFont()
        {
            var io = ImGui.GetIO();
            io.Fonts.Clear();
            var fontPtr = zpixFont.zpixFont.getFont(out long fontLen);
            io.Fonts.AddFontFromMemoryTTF(fontPtr, (int)fontLen, 12f, null, io.Fonts.GetGlyphRangesChineseFull());
        }
        bool SubmitUI()
        {
            if (_showAlertWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(this.Size.X / 2, this.Size.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(this.Size.X / 4, this.Size.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Alert", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, this.Size.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(this.Size.X / 4 - 50, this.Size.Y / 2 - 35));
                if (ImGui.Button("OK", new Vector2(100, 25)))
                    _showAlertWindow = false;
                ImGui.End();
            }
            else if (_showConfirmWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(this.Size.X / 2, this.Size.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(this.Size.X / 4, this.Size.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Confirm", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, this.Size.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(this.Size.X / 8 - 50, this.Size.Y / 2 - 35));
                if (ImGui.Button("Yes", new Vector2(100, 25)))
                {
                    _actionConfirmWindow(_objectConfirmWindow);
                    _showConfirmWindow = false;
                }
                ImGui.SetCursorPos(new Vector2((this.Size.X * 3 / 8) - 50, this.Size.Y / 2 - 35));
                if (ImGui.Button("No", new Vector2(100, 25)))
                    _showConfirmWindow = false;
                ImGui.End();
            }
            else
            {
                {
                    ImGui.SetCursorPos(new Vector2(10, 15));
                    ImGui.Text("Utau VoiceBank Search Dirs:");
                    ImGui.SetCursorPos(new Vector2(10, 40));
                    ImGui.PushItemWidth(this.Size.X - 155);
                    if (ImGui.ListBox("", ref Worker.VoiceDirSelectedIndex, Worker.VoiceDirList.ToArray(), Worker.VoiceDirList.Count, 15))
                    {
                        // Alert(Worker.ExtensionSelectedIndex.ToString());
                    };
                    ImGui.PopItemWidth();

                    ImGui.SetCursorPos(new Vector2(this.Size.X - 125, 40));
                    if (ImGui.Button("Add Dir", new Vector2(100, 35)))
                    {
                        Worker.AddDir();
                    }
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 125, 40 + 40 * 1));
                    if (ImGui.Button("Add Utau", new Vector2(100, 35)))
                    {
                        var v=Worker.PickUtauVoiceBank(out string error);
                        if (v == "")
                        {
                            if (error.Length > 0) Alert(error);
                        }else
                        {
                            Worker.AddDir(v);
                        }
                    }
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 125, 40 + 40 * 2));
                    if (!(Worker.VoiceDirSelectedIndex < Worker.NotVoiceDirFileDataLen && Worker.VoiceDirSelectedIndex >= 0)) if (ImGui.Button("Remove Dir", new Vector2(100, 35)))
                        {
                            if (Worker.VoiceDirSelectedIndex >= 0 && Worker.VoiceDirList.Count > Worker.VoiceDirSelectedIndex)
                                Confirm(String.Format("Did you confirm to remove the voice search directory:{0} ?", Worker.VoiceDirList[Worker.VoiceDirSelectedIndex]), Worker.RemoveDir, Worker.VoiceDirSelectedIndex - Worker.NotVoiceDirFileDataLen);
                        }
                }
                {
                    {
                        ImGui.GetWindowDrawList().AddLine(
                            new Vector2(this.Size.X - 125, 2 + 40 + 40 * 3),
                            new Vector2(this.Size.X - 25, 2 + 40 + 40 * 3),
                            ImGui.GetColorU32(new Vector4(1, 1, 1, 1)),
                            1.0f                                     // 线宽
                        );
                    }                    
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 125, 10 + 40 + 40 * 3));
                    if (ImGui.Button("Clean\nUVoiceBank", new Vector2(100, 35)))
                    {
                        Worker.CleanAllUVoiceBank();
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        ImGui.SetCursorPos(new Vector2(this.Size.X - 125, 10 + 40 + 40 * 4));
                        if (ImGui.Button("Kill All\nResamplers", new Vector2(100, 35)))
                        {
                            Worker.KillAllResamplers();
                        }
                    }
                }

                {
                    ImGui.SetCursorPos(new Vector2(10, 315));
                    ImGui.Text("Avaliable Voice Banks:");
                    ImGui.SetCursorPos(new Vector2(10, 340));
                    ImGui.PushItemWidth(this.Size.X - 400);
                    if (ImGui.ListBox(" ", ref Worker.VoiceBankSelectedIndex, Worker.VoiceBankList.ToArray(), Worker.VoiceBankList.Count, (int)Math.Round((this.Size.Y - 340)/17.0) ))
                    {
                        Worker.LoadVoiceBankData(Worker.VoiceBankSelectedIndex);
                    };
                    ImGui.PopItemWidth();
                    {
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 340 + 40 * 0));
                            ImGui.Text("Voice Bank Path:");
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 340 + 20 * 1));
                            string vbPath = Worker.GetVBInfo_Path(Worker.VoiceBankSelectedIndex, 28);
                            ImGui.InputTextMultiline(" ", ref vbPath, 512, new Vector2(200, 40 * 2), ImGuiInputTextFlags.ReadOnly);
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 265, 340 - 8));
                            if (ImGui.Button("Navigate", new Vector2(90, 25)))
                            {
                                Worker.NavigateDBPath(Worker.VoiceBankSelectedIndex);
                            }
                        }
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 325 + 40 * 3));
                            ImGui.Text("Overlayed Name:");
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 345 + 40 * 3));
                            ImGui.PushItemWidth(200);
                            ImGui.InputText("  ", ref Worker.CurrentVBInfo_OverlayedName, (uint)65535);
                            ImGui.PopItemWidth();
                        }
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 325 + 41 * 4));
                            ImGui.Text("Detected Name:");
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 345 + 41 * 4));
                            ImGui.PushItemWidth(200);
                            string det = Worker.CurrentVBInfo_DetectedName;
                            ImGui.InputText("   ", ref det, (uint)Worker.CurrentVBInfo_DetectedName.Length,ImGuiInputTextFlags.ReadOnly);
                            ImGui.PopItemWidth();
                        }
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 325 + 42 * 5));
                            ImGui.Text("Text Encoding:");
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 345 + 42 * 5));
                            ImGui.PushItemWidth(200); 
                            if (ImGui.BeginCombo("      ", Worker.GetVBInfo_EncodingName(Worker.CurrentVBInfo_EncodingIndex)))
                            {
                                var ix = Worker.CurrentVBInfo_EncodingIndex;
                                var mx = Worker.EncodingNameList.Count;
                                var cl = Worker.EncodingNameList;
                                if (!(ix >= 0 && ix < mx)) ix = 0;
                                for (int i = 0; i < mx; i++)
                                {
                                    bool isSelected = (ix == i);
                                    if (ImGui.Selectable(cl[i], isSelected))
                                    {
                                        if (ix != i)
                                        {
                                            Worker.CurrentVBInfo_EncodingIndex = i;
                                            Worker.OnVBEncodingChange(i);
                                        }
                                    }
                                    if (isSelected)
                                    {
                                        ImGui.SetItemDefaultFocus();
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            ImGui.PopItemWidth();
                        }
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 325 + 43 * 6));
                            ImGui.Text("Phonemizer:");
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 345 + 43 * 6));
                            ImGui.PushItemWidth(200);
                            if (ImGui.BeginCombo("     ", Worker.GetVBInfo_PhonemizerName(Worker.CurrentVBInfo_PhonemizerIndex)))
                            {
                                var ix = Worker.CurrentVBInfo_PhonemizerIndex;
                                var mx = Worker.PhonemizerList.Count;
                                var cl = Worker.PhonemizerList;
                                if (!(ix >= 0 && ix < mx)) ix = 0;
                                for (int i = 0; i < mx; i++)
                                {
                                    bool isSelected = (ix == i);
                                    if (ImGui.Selectable(cl[i], isSelected))
                                    {
                                        if (ix != i)
                                        {
                                            Worker.CurrentVBInfo_PhonemizerIndex = i;
                                            Worker.OnVBPhonemizerChange(i);
                                        }
                                    }
                                    if (isSelected)
                                    {
                                        ImGui.SetItemDefaultFocus();
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            ImGui.PopItemWidth();
                        }
                        {
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 375, 345 + 41 * 7));
                            if (ImGui.Button("Save Change", new Vector2(95, 30)))
                            {
                                Worker.Do_VB_SaveChange(Worker.VoiceBankSelectedIndex);
                            }
                            ImGui.SetCursorPos(new Vector2(this.Size.X - 270, 345 + 41 * 7));
                            if (ImGui.Button("Rebuild UVB", new Vector2(95, 30)))
                            {
                                Worker.Do_VB_RebuildUVB(Worker.VoiceBankSelectedIndex);
                            }
                        }
                    }
                }
                {
                    ImGui.GetWindowDrawList().AddLine(
                        new Vector2(this.Size.X - 155,320),
                        new Vector2(this.Size.X - 155,this.Size.Y-25),
                        ImGui.GetColorU32(new Vector4(1, 1, 1, 1)),
                        1.0f                                     // 线宽
                    );
                }
                {
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 315));
                    ImGui.Text("Caches:");
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 340));
                    ImGui.TextWrapped(String.Format("Space Used:\n {0} GB",Worker.CacheSize.ToString("F2")));
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 380));
                    if (ImGui.Button("Clear", new Vector2(100, 35)))
                    {
                        Worker.ClearCache();
                        Task.Run(() => { Worker.UpdateCacheSize(); });
                    }
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 420));
                    if (ImGui.Button("Refresh", new Vector2(100, 35)))
                    {
                        Task.Run(() => { Worker.UpdateVoiceDirs(); });
                        Task.Run(() => { Worker.UpdateCacheSize(); });
                    }

                }
                {
                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 450 + 40));
                    ImGui.Text("Switchers:");

                    ImGui.SetCursorPos(new Vector2(this.Size.X - 140, 450 + 60));
                    if (ImGui.Button((SwitcherManager.IsAudioEffectEnable?"Disable":"Enable")+"\nAudioEffect", new Vector2(100, 35)))
                    {
                        SwitcherManager.IsAudioEffectEnable = !SwitcherManager.IsAudioEffectEnable;
                        SwitcherManager.SaveSwitchers();
                        SwitcherManager.UpdateSwitchers();
                    }
                }
            }
            return true;
        }
    }
}