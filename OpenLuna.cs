using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

internal static class OpenLuna
{
    [STAThread]
    private static int Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        string electron = Path.Combine(baseDir, "node_modules", "electron", "dist", "electron.exe");
        string projectDir = baseDir;

        if (!File.Exists(electron))
        {
            MessageBox.Show(
                "Luna's engine is not installed yet.\n\nOpen PowerShell in this folder and run:\n\n  npm install\n\nthen try again.",
                "Open Luna",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return 1;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = electron,
                Arguments = "\"" + projectDir + "\"",
                WorkingDirectory = projectDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not start Luna:\n\n" + ex.Message, "Open Luna", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
