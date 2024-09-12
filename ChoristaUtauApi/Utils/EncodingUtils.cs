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
            try
            {
                if (EncodingName.ToUpper() == "UTF8") return Encoding.UTF8;
                if (EncodingName.ToUpper() == "UTF-8") return Encoding.UTF8;
                if (EncodingName.ToUpper() == "ASCII") return Encoding.ASCII;
                if (EncodingName.ToUpper() == "UNICODE") return Encoding.Unicode;
                var det = CodePagesEncodingProvider.Instance.GetEncoding(EncodingName);
                if (det == null) det = CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS");
                if (det == null) return Encoding.Default;
                return det;
            }
            catch { return Encoding.Default; }
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
                if (EncodingItem == Encoding.UTF8) return "UTF-8";
                else if (EncodingItem == Encoding.ASCII) return "ASCII";
                else if (EncodingItem == Encoding.Unicode) return "Unicode";
                else if (EncodingItem == System.Text.CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS")) return "Shift-JIS";
                else return EncodingItem.WebName;
            }
            catch { return EncodingItem.WebName; }
        }
    }
}
