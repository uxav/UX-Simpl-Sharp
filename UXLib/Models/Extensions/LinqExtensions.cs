using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models.Extensions
{
    public static class LinqExtensions
    {
        public static SourceCollection ToSourceCollection(this IEnumerable<Source> sources)
        {
            return new SourceCollection(sources);
        }
    }
}