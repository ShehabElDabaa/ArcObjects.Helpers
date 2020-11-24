using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DEWA.GIS.Desktop.Helpers
{
    public class Helper
    {
        public static void CopyDirectoryRecursivly(string sourcePath, string destinationPath, params string[] execludedExtensions)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
                return;

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            if (execludedExtensions.Length > 0)
                execludedExtensions = execludedExtensions.Select(e => e.ToLower()).ToArray();

            var filesPaths = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Where(f => !execludedExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()));

            foreach (string filePath in filesPaths)
                File.Copy(filePath, filePath.Replace(sourcePath, destinationPath), true);
        }

        public static string ConvertFromBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            byte[] data = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(data);
        }


        public static string LoadFileText(string templateFileName)
        {
            string fullExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var fullPath = $"{Directory.GetParent(fullExePath).FullName}\\Helpers\\EmailTemplates\\{templateFileName}";
            if (!File.Exists(fullPath))
                return string.Empty;

            using (StreamReader reader = new StreamReader(fullPath))
                return reader.ReadToEnd();
        }


        public static void StopService(string serviceName, string machineName = ".")
        {
            ServiceController sc = new ServiceController
            {
                ServiceName = serviceName,
                MachineName = machineName
            };

            if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Refresh();
            }
        }

        public static void StartService(string serviceName, string machineName = ".")
        {
            ServiceController sc = new ServiceController
            {
                ServiceName = serviceName,
                MachineName = machineName
            };

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running);
                sc.Refresh();
            }
        }

    }
}
