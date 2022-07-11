using System.Diagnostics;

namespace Tubular
{
    public static class Utils
    {
        public static void StartRedirectedProcess(string fileName, string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            proc.ErrorDataReceived += (_, eventArgs) => Debug.WriteLine(eventArgs.Data);
            proc.OutputDataReceived += (_, eventArgs) => Debug.WriteLine(eventArgs.Data);
            
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
        }

        public static void ShellExecute(string fileName)
        {
            Process.Start(new ProcessStartInfo(fileName){ UseShellExecute = true });
        }
    }
}
