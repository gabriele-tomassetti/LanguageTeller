using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    // This utility class contains some currently unused
    // extension methods to initialize lists.    
    public static class Utility
    {
        public static string ToRealString(this StringBuilder sb)
        {
            byte[] bytes = new byte[sb.Length];
            for (int a = 0; a < bytes.Length; a++)
                bytes[a] = (byte) sb[a];

            return Encoding.UTF8.GetString(bytes);            
        }

        public static int Size(this String s)
        {
            return Encoding.UTF8.GetBytes(s).Count();
        }

        public static List<T> Repeat<T>(T t, long size)
        {
            List<T> data = new List<T>();            
            for (long a = 0; a < size; a++)
            {
                data.Add(t);
            }

            return data;
        }

        public static List<T> Repeat<T>(long size)
        {
            List<T> data = new List<T>();
            for (long a = 0; a < size; a++)
            {
                data.Add(default(T));
            }

            return data;
        }        
    }
}
