using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program
{
    static async Task Main(string[] args)
    {
        int exitCode = 0;
        string logPath = Path.Combine(Path.GetTempPath(), "ElementInstaller.log");

        try
        {
            // Force TLS 1.2 to avoid SSL/TLS errors
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Log(logPath, "=== Element Installer Started ===");

            // Determine config folder
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string elementDir = Path.Combine(appData, "Element");
            string configPath = Path.Combine(elementDir, "config.json");

            Log(logPath, $"Config directory: {elementDir}");

            // Create directory if needed
            Directory.CreateDirectory(elementDir);
            Log(logPath, "Directory created/verified");

            // Write config file
            string configJson = @"{
  ""features"": {
    ""feature_latex_maths"": true,
    ""feature_share_history_on_invite"": true
  }
}";
            File.WriteAllText(configPath, configJson);
            Log(logPath, $"Config file written to: {configPath}");

            // Download Element installer
            Log(logPath, "Downloading Element installer...");
            string installerUrl = "https://packages.riot.im/desktop/install/win32/x64/Element%20Setup.exe";
            string installerPath = Path.Combine(Path.GetTempPath(), "ElementSetup.exe");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                byte[] installerData = await client.GetByteArrayAsync(installerUrl);
                File.WriteAllBytes(installerPath, installerData);
            }

            Log(logPath, $"Installer downloaded to: {installerPath}");

            // Run installer visibly
            Log(logPath, "Launching Element installer...");
            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            });

            Log(logPath, "=== Installation completed successfully ===");
        }
        catch (Exception ex)
        {
            Log(logPath, $"ERROR: {ex.ToString()}");
            exitCode = 1;

            MessageBox.Show($"Element Installer encountered an error:\n\n{ex.Message}\n\nCheck log for details:\n{logPath}",
                            "Installation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
        finally
        {
            Environment.Exit(exitCode);
        }
    }

    static void Log(string logPath, string message)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        File.AppendAllText(logPath, logEntry + Environment.NewLine);
    }
}
