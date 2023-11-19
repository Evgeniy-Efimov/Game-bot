using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Managers
{
    //Log exceptions and messages to file log
    public static class LogManager
    {
        public static void LogMessageToFile(string log, string fileName = "")
        {
            string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectoryPath)) Directory.CreateDirectory(logDirectoryPath);
            string fileNameWithExt = (string.IsNullOrWhiteSpace(fileName) ? $"{DateTime.Now.ToString("yyyy'-'MM'-'dd")}" : fileName) + ".txt";
            string pathFile = Path.Combine(logDirectoryPath, fileNameWithExt);
            if (!File.Exists(pathFile)) File.Create(pathFile).Close();
            log = "\r\n ---------------- " + DateTime.Now + " ---------------- \r\n" + log + "\r\n";
            File.AppendAllText(pathFile, log);
        }
        public static void LogException(Exception ex, string message = null, bool isInner = false)
        {
            var log = $"{(isInner ? "Inner exception" : "Exception")}: {ex.Message}";
            if (!string.IsNullOrWhiteSpace(message))
            {
                log = $"{message}: \r\n{log}";
            }
            LogMessageToFile(log);
            if (ex.InnerException != null)
            {
                LogException(ex.InnerException, isInner: true);
            }
        }
    }
}
