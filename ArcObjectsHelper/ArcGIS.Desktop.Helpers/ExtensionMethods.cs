using System;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Return a new string without any whitespaces and linebreaks wherever found.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Flatten(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Trim().Replace(" ", string.Empty)
                 .Replace(Environment.NewLine, string.Empty)
                 .Replace("\n", string.Empty)
                 .Replace("\r", string.Empty)
                 .Replace("\t", string.Empty);
        }
        
        /// <summary>
        /// Finds property value by reflection.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object FindPropValue(this object src, string propertyName)
        {
            return src?.GetType().GetProperty(propertyName)?.GetValue(src);
        }


        
    }
}
