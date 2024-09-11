using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.Utils
{
    public class EncodingUtils
    {
        public static Encoding GetEncoding(string EncodingName)
        {
            if (EncodingName == "UTF8") return Encoding.UTF8;
            if (EncodingName == "ASCII") return Encoding.ASCII;
            if (EncodingName == "Unicode") return Encoding.Unicode;
            return CodePagesEncodingProvider.Instance.GetEncoding(EncodingName);
        }
        public static Encoding GetEncoding(int CodePage)
        {
            try
            {
                return System.Text.CodePagesEncodingProvider.Instance.GetEncoding(CodePage);
            }
            catch
            {
                return Encoding.Default;
            }
        }
        public static string GetEncodingName(Encoding EncodingItem)
        {
            try{
                if (EncodingItem == Encoding.UTF8) return "UTF8";
                else if (EncodingItem == Encoding.ASCII) return "ASCII";
                else if (EncodingItem == Encoding.Unicode) return "Unicode";
                else if (EncodingItem == System.Text.CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS")) return "Shift-JIS";
                else return EncodingItem.WebName;
            }
            catch { return EncodingItem.WebName; }
        }
    }
}
