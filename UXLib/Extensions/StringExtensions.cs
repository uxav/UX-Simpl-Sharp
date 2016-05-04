using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace UXLib.Extensions
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string s)
        {
            return Regex.Replace(Regex.Replace(s, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static byte[] ToByteArray(this string s)
        {
            byte[] result = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                result[i] = unchecked((byte)s[i]);
            }

            return result;
        }
    }
}