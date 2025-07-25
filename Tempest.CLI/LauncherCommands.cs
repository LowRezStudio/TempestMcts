using System.Diagnostics;
using System.Runtime.InteropServices;
using ConsoleAppFramework;

namespace Tempest.CLI;

internal class LauncherCommands
{
    public void Launch(ConsoleAppContext context, string path, bool noDefaultArgs = false, string? platform = null, string[]? dlls = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Error.WriteLine("The \"launch\" command only works on Windows or under Wine!");
            return;
        }

        var exePath = Path.GetFullPath(path);
        var defaultArgs = !noDefaultArgs;

        if (!Path.GetExtension(exePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var gameFolder = new DirectoryInfo(exePath);

            while (gameFolder != null)
            {
                var fullName = gameFolder.FullName;

                if (Directory.Exists(Path.Join(fullName, "Binaries")) &&
                    Directory.Exists(Path.Join(fullName, "Engine")))
                {
                    break;
                }
                
                gameFolder = gameFolder.Parent;
            }

            if (gameFolder == null)
            {
                Console.Error.WriteLine("Couldn't find the Paladins game folder (containing Binaries and Engine folders)");
                return;
            }
            
            exePath = Path.Join(gameFolder.FullName, "Binaries", platform ?? "Win64", "Paladins.exe");

            if (platform == "Win32" || !Environment.Is64BitProcess || !File.Exists(exePath))
            {
                exePath = Path.Join(gameFolder.FullName, "Binaries", "Win32", "Paladins.exe");
            }

            if (!File.Exists(exePath))
            {
                Console.Error.WriteLine($"\"{exePath}\" doesn't exist");
                return;
            }
        }
        
        var process = new Process();

        process.StartInfo.FileName = exePath;
        process.StartInfo.Environment["OPENSSL_ia32cap"] = "~0x200000200000000"; // Fix for the 64bit clients not working on 10th Gen and 11th Gen Intel CPUs
        
        foreach (var arg in context.EscapedArguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }
        
        if (defaultArgs)
        {
            process.StartInfo.ArgumentList.Add("-log");
            process.StartInfo.ArgumentList.Add("-seekfreeloadingpcconsole");
            process.StartInfo.ArgumentList.Add("-pid=402");
            process.StartInfo.ArgumentList.Add("-anon");
            process.StartInfo.ArgumentList.Add("-nosteam");
            process.StartInfo.ArgumentList.Add("-eac-nop-loaded");
            process.StartInfo.ArgumentList.Add("-replayfile=");
            process.StartInfo.ArgumentList.Add("-COOKFORDEMO");
            process.StartInfo.ArgumentList.Add("-homedir=Tempest");
        }

        process.Start();
    }
}