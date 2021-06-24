using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCube
{
    public static class Extensions
    {
        public static string ToCSV(this String value) => $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
