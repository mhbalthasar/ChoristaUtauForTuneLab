using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using Serilog;
using YamlDotNet.Serialization;

namespace OpenUtau.Core.Ustx {
    public class URenderSettings {
        public void Validate(UTrack track) {
            if (track.Singer == null || !track.Singer.Found) {
                return;
            }
        }

        public URenderSettings Clone() {
            return new URenderSettings {
            };
        }
    }

    public class UTrack {
        public string singer;
        public string phonemizer;
        public URenderSettings RendererSettings { get; set; } = new URenderSettings();

        private USinger singer_;

        [YamlIgnore]
        public USinger Singer {
            get => singer_;
            set {
                if (singer_ != value) {
                    singer_ = value;
                    VoiceColorExp = null;
                }
            }
        }
        [YamlIgnore] public OpenUtau.Api.Phonemizer Phonemizer { get; set; } = new DefaultPhonemizer();// PhonemizerFactory.Get(typeof(DefaultPhonemizer)).Create();
        [YamlIgnore] public string PhonemizerTag => Phonemizer.Tag;
        [YamlIgnore] public int TrackNo { set; get; }
        public string TrackName { get; set; } = "New Track";
        public string TrackColor { get; set; } = "Blue";
        [YamlIgnore] public bool Muted { set; get; }
        public bool Mute { get; set; }
        public bool Solo { get; set; }
        public double Volume { set; get; }
        public double Pan { set; get; }

        public List<UExpression> TrackExpressions { get; set; } = new List<UExpression>();
        [YamlIgnore] public UExpressionDescriptor VoiceColorExp { set; get; }
        public string[] VoiceColorNames { get; set; } = new string[] { "" };

        public UTrack() {
        }
        public UTrack(UProject project) {
            int trackCount = 0;
            if (project.tracks != null && project.tracks.Count > 0) {
                trackCount = project.tracks.Max(t => int.TryParse(t.TrackName.Replace("Track", ""), out int result) ? result : 0);
                if (project.tracks.Count > trackCount) {
                    trackCount = project.tracks.Count;
                }
            }
            TrackName = "Track" + (trackCount + 1);
        }
        public UTrack(string trackName) {
            TrackName = trackName;
        }

        /**  
            <summary>
                Return false if there is no corresponding descriptor in the project
            </summary>
        */
        public bool TryGetExpDescriptor(UProject project, string abbr, out UExpressionDescriptor descriptor) {
            if (!project.expressions.TryGetValue(abbr, out descriptor)) {
                return false;
            }
            if (abbr == Format.Ustx.CLR && VoiceColorExp != null) {
                descriptor = VoiceColorExp;
            }
            return true;
        }


        /**  
            <summary>
                Return false if there is no corresponding descriptor in the project
            </summary>
        */
        public bool TryGetExpression(UProject project, string abbr, out UExpression expression) {
            if (!TryGetExpDescriptor(project, abbr, out var descriptor)) {
                expression = new UExpression(descriptor);
                return false;
            }

            var trackExp = TrackExpressions.FirstOrDefault(e => e.descriptor.abbr == abbr);
            if (trackExp != null) {
                expression = trackExp.Clone();
            } else {
                expression = new UExpression(descriptor) { value = descriptor.defaultValue };
            }
            return true;
        }

        public void Validate(ValidateOptions options, UProject project) {
            if (Singer != null && Singer.Found) {
                Singer.EnsureLoaded();
            }
            if (RendererSettings == null) {
                RendererSettings = new URenderSettings();
            }
            RendererSettings.Validate(this);
            if (project.expressions.TryGetValue(Format.Ustx.CLR, out var descriptor)) {
                if (VoiceColorExp == null && Singer != null && Singer.Found && Singer.Loaded) {
                    VoiceColorExp = descriptor.Clone();
                    var colors = Singer.Subbanks.Select(subbank => subbank.Color).ToHashSet();
                    VoiceColorExp.options = colors.OrderBy(c => c).ToArray();
                    VoiceColorExp.max = VoiceColorExp.options.Length - 1;
                }
            }
        }

        public bool ValidateVoiceColor(out string[] oldColors, out string[] newColors) {
            bool discrepancy = false;
            oldColors = VoiceColorNames.ToArray();
            newColors = new string[0];

            if (Singer != null && Singer.Found && VoiceColorExp != null && VoiceColorExp.options.Length > 0) {
                newColors = VoiceColorExp.options.ToArray();

                if (VoiceColorNames.Length > 1) {
                    if (VoiceColorNames.Length != VoiceColorExp.options.Length) {
                        discrepancy = true;
                    } else {
                        for (int i = 0; i < VoiceColorNames.Length; i++) {
                            if (VoiceColorNames[i] != VoiceColorExp.options[i]) {
                                discrepancy = true;
                                break;
                            }
                        }
                    }
                }
                VoiceColorNames = VoiceColorExp.options.ToArray();
            }
            return discrepancy;
        }

        public void BeforeSave() {
            singer = Singer?.Id;
            phonemizer = Phonemizer.GetType().FullName;
            if (Singer != null && Singer.Found && VoiceColorExp != null && VoiceColorExp.options.Length > 0) {
                VoiceColorNames = VoiceColorExp.options.ToArray();
            } else {
                VoiceColorNames = new string[] { "" };
            }
        }

        public void AfterLoad(UProject project) {
            if (RendererSettings == null) {
                RendererSettings = new URenderSettings();
            }
            TrackNo = project.tracks.IndexOf(this);
            if (!Solo && Mute) {
                Muted = true;
            }
        }
    }
}
