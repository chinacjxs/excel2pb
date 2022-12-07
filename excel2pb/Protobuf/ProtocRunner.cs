using System.Diagnostics;
using System.Text;

namespace excel2pb
{
    /// <summary>
    /// Google Protoc
    /// </summary>
    public class ProtocRunner : Singleton<ProtocRunner>
    {
        readonly string protoc = "protoc.exe";

        readonly StringBuilder sb = new StringBuilder();

        readonly ProcessStartInfo startInfo = new ProcessStartInfo();

        ProtocRunner()
        {
            startInfo.FileName = protoc;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }

        public void Run(string format,params object[] args)
        {
            sb.Clear();
            sb.Append(" ");
            sb.AppendFormat(format, args);

            startInfo.Arguments = sb.ToString();

            using (Process process = Process.Start(startInfo))
            {
                string str = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(str))
                    System.Console.WriteLine(str);

                str = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(str))
                    Utility.Exception(null,str);
            }
        }
    }
}
