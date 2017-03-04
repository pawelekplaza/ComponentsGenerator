using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComponentsGenerator.Utils
{
    public static class Working
    {
        public static string GetToStringFormat(int maxNumber)
        {
            var asString = maxNumber.ToString();
            string result = "";
            foreach (var value in asString)
                result += "0";

            return result;
        }

        public static string GetSubDirPath(string parentPath, string wholePath)
        {
            if (IsSubdirectory(parentPath, wholePath))
            {
                var startIndex = parentPath.Length - (Path.GetFileName(parentPath).Length);
                if (startIndex >= wholePath.Length)
                    return "";
                return wholePath.Substring(startIndex);
            }

            var dirPath = Path.GetDirectoryName(wholePath);
            dirPath = dirPath.Substring(dirPath.LastIndexOf("\\") + 1);
            return dirPath.Length > 0 ? $"{ dirPath }\\{ Path.GetFileName(wholePath) }" : "";
        }

        public static bool IsSubdirectory(string parentDir, string childDir)
        {
            return childDir.Contains(parentDir);
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, "Components Generator", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static string RemoveBlanks(string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if (char.IsWhiteSpace(c))
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
