using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenUtau.Core;

namespace OpenUtau.Classic {
    public class Subbank {
        /// <summary>
        /// Voice color, e.g., "power", "whisper". Leave unspecified for the main bank.
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Subbank prefix. Leave unspecified if none.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Subbank suffix. Leave unspecified if none.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// Tone ranges. Each range specified as "C1-C4" or "C4".
        /// </summary>
        public string[] ToneRanges { get; set; }
    }

}
