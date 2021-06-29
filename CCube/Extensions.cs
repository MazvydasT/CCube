using System;

namespace CCube
{
    public static class Extensions
    {
        public static string ToCSV(this String value) => $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
