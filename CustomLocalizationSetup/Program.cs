using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CustomLocalizationSetup {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      if (File.Exists("Assembly-CSharp.dll")) {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
      } else {
        Console.WriteLine("Not exists: ");
        string managedPath = AppDomain.CurrentDomain.BaseDirectory;
        managedPath = Path.Combine(managedPath, "..");
        managedPath = Path.Combine(managedPath, "..");
        managedPath = Path.Combine(managedPath, "BattleTech_Data");
        managedPath = Path.Combine(managedPath, "Managed");
        string exeDstPath = Path.Combine(managedPath, Path.GetFileName(Application.ExecutablePath));
        string dllDstPath = Path.Combine(managedPath, "CustomLocalization.dll");
        string exeSrcPath = Application.ExecutablePath;
        string dllSrcPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CustomLocalization.dll");
        string logDstPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CustomTranslation.log");
        string logSrcPath = Path.Combine(managedPath, "CustomTranslation.log");
        try {
          Console.WriteLine("Copy:");
          Console.WriteLine("From:" + exeSrcPath);
          Console.WriteLine("To:" + exeDstPath);
          File.Copy(exeSrcPath, exeDstPath, true);
          Console.WriteLine("success");
          Console.WriteLine("Copy:");
          Console.WriteLine("From:" + dllSrcPath);
          Console.WriteLine("To:" + dllDstPath);
          File.Copy(dllSrcPath, dllDstPath, true);
          Console.WriteLine("success");
          ProcessStartInfo psi = new ProcessStartInfo();
          psi.WorkingDirectory = managedPath;
          psi.FileName = exeDstPath;
          Console.WriteLine("Starting:" + psi.FileName);
          Process p = Process.Start(psi);
          while (p.WaitForExit(100) == false) {
            try { File.Copy(logSrcPath, logDstPath, true); } catch (Exception) { }
          }
          try { File.Copy(logSrcPath, logDstPath, true); } catch (Exception) { }
          File.Delete(logSrcPath);
          File.Delete(exeDstPath);
          File.Delete(dllDstPath);
        } catch (Exception e) {
          Console.WriteLine(e.ToString());
        }
      }
    }
  }
}
