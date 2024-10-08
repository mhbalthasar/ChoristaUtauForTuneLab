﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TuneLab.Base.Properties.PropertyPath;
using TuneLab.Base.Structures;

namespace UtaubaseForTuneLab.Utils
{
    internal static class ClassHelper
    {
        public static OrderedMap<TKey, TValue> Combine<TKey,TValue>(this OrderedMap<TKey, TValue> map, IReadOnlyOrderedMap<TKey, TValue> src,bool append=true)
        {
            int index = 0;
            foreach(var pair in src)
            {
                if(append)
                    map.Add(pair.Key, pair.Value);
                else
                    map.Insert(index,pair.Key,pair.Value);
                index++;
            }
            return map;
        }

        public static List<T> InsertList<T>(this List<T> list,T item,int index = 0)
        {
            list.Insert(index, item);
            return list;
        }
        public static List<T> AppendList<T>(this List<T> list, T item)
        {
            list.Append(item);
            return list;
        }

        public static List <List <T>> DivideList<T>(this List <T> list, int count)
        {
            List<List<T>> ret= new List<List<T>>();
            for (int i = 0; i < list.Count; i++)
            {
                if(i<count)
                {
                    ret.Add(new List<T> { list[i] });
                }
                else
                {
                    var gi = i % count;
                    ret[gi].Add(list[i]);
                }
            }
            return ret;
        }
    }
}
