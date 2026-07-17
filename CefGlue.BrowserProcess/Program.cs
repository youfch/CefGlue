using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xilium.CefGlue.BrowserProcess.Helpers;
using Xilium.CefGlue.Common.Shared;

namespace Xilium.CefGlue.BrowserProcess
{
    class Program
    {
        [DllImport("ole32.dll", ExactSpelling = true)]
        private static extern int CoInitializeEx(IntPtr reserved, uint dwCoInit);

        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CefGlue.BrowserProcess.log"
        );

        private static void Log(string message)
        {
            try
            {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{Environment.ProcessId}] {message}";
                File.AppendAllText(LogPath, line + Environment.NewLine);
                Console.Error.WriteLine(line);
            }
            catch
            {
                // silently ignore logging failures
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Log("=== BrowserProcess started ===");
                Log("Args: " + string.Join(" ", args.Select(a => a.Length > 80 ? a.Substring(0, 80) + "..." : a)));

                // COM must be initialized for CEF's Win32 OLE/clipboard operations
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Log("Initializing COM (COINIT_APARTMENTTHREADED)...");
                    var hr = CoInitializeEx(IntPtr.Zero, 2 /* COINIT_APARTMENTTHREADED */);
                    Log($"CoInitializeEx returned: {hr} (0x{hr:X8})");
                }

                Log("Installing NativeLibsLoader...");
                NativeLibsLoader.Install();
                Log("NativeLibsLoader installed.");

                var parentProcessId = GetArgumentValue(args, CommandLineArgs.ParentProcessId);
                if (parentProcessId != null && int.TryParse(parentProcessId, out var parentProcessIdAsInt))
                {
                    Log($"Starting parent process monitor (PID: {parentProcessIdAsInt})...");
                    ParentProcessMonitor.StartMonitoring(parentProcessIdAsInt);
                    Log("Parent process monitor started.");
                }

                Log("Loading CEF runtime...");
                CefRuntime.Load();
                Log("CEF runtime loaded.");

                var customSchemesArg = GetArgumentValue(args, CommandLineArgs.CustomScheme);
                var customSchemes = CustomScheme.FromCommandLineValue(customSchemesArg);
                Log($"Custom schemes: {customSchemesArg}");

                var mainArgs = new CefMainArgs(new[] { "BrowserProcess" }.Concat(args).ToArray());
                Log("Calling CefRuntime.ExecuteProcess...");
                var exitCode = CefRuntime.ExecuteProcess(mainArgs, new RendererCefApp(customSchemes), IntPtr.Zero);
                Log($"CefRuntime.ExecuteProcess returned exit code: {exitCode}");

                if (exitCode != -1)
                {
                    Log($"Exiting with code {exitCode}");
                    Environment.Exit(exitCode);
                }

                Log("=== BrowserProcess ended normally ===");
            }
            catch (Exception ex)
            {
                Log($"!!! UNHANDLED EXCEPTION: {ex.GetType().FullName}");
                Log($"!!! Message: {ex.Message}");
                Log($"!!! StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log($"!!! InnerException: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
                }

#if DEBUG
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try { System.Diagnostics.Debugger.Launch(); } catch { }
                }
#endif
                throw;
            }
        }

        private static string GetArgumentValue(string[] args, string argName)
        {
            var arg = args.FirstOrDefault(a => a?.StartsWith(argName + "=") == true);
            return arg?.Substring(argName.Length + 1) ?? "";
        }
    }
}