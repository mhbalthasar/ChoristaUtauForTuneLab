using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.UVoiceBank
{
    internal class UVoiceBankPrefixAnalysiser
    {
        static string CommonPrefix(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return "";

            string minStr = strings.OrderBy(s => s.Length).First();

            for (int i = 0; i < minStr.Length; i++)
            {
                char currentChar = minStr[i];
                if (strings.Any(s => s[i] != currentChar))
                    return minStr.Substring(0, i);
            }

            return minStr;
        }

        static string CommonSuffix(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return "";

            string minStr = strings.OrderBy(s => s.Length).First();

            for (int i = 0; i < minStr.Length; i++)
            {
                char currentChar = minStr[minStr.Length - 1 - i];
                if (strings.Any(s => s[s.Length - 1 - i] != currentChar))
                    return minStr.Substring(minStr.Length - i);
            }

            return minStr;
        }

        public static PrefixPair AnalysisPrefixFromAlias(List <string> strings)
        {
            var prefix=CommonPrefix(strings);
            var suffix=CommonSuffix(strings);
            PrefixItem ret = new PrefixItem()
            {
                prefix = prefix,
                suffix = suffix,
                PitchNumber = 0
            };
            string kPair = ret.prefix.Trim() + ret.suffix.Trim();
            if (kPair.Trim() == "") kPair = "<No Prefix>";
            return new PrefixPair() { Key=kPair,PrefixItem=ret };
        }
        public static List<PrefixPair> AddPair(List<PrefixPair> target, PrefixPair item,string AppendKeyPrefix="")
        {
            int sameValueCount=target.Where(p=>p.Key==item.Key || (p.PrefixItem.prefix==item.PrefixItem.prefix && p.PrefixItem.suffix == item.PrefixItem.suffix)).Count();
            if(sameValueCount==0)
            {
                item.Key = (AppendKeyPrefix + " " + item.Key).Trim();
                target.Add(item);
            }
            return target;
        }
    }
}
