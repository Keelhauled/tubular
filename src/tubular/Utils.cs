using System.Diagnostics;
using System.IO;

namespace Tubular
{
    public static class Utils
    {
        public static void StartRedirectedProcess(string filepath, string args)
        {
            if(!File.Exists(filepath))
                throw new FileNotFoundException(filepath);
            
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filepath,
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
    }
}
