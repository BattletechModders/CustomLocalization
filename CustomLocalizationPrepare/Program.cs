using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using CustomTranslation; 

namespace CustormLocalizationPrepare {
  static class Program {
    /// <summary>           
    /// The main entry point for the application.                                                          
    /// </summary>
    [STAThread]
    static void Main() {
      Console.WriteLine("Main");
      if (File.Exists("Assembly-CSharp.dll")) {
        Console.WriteLine("Exists");
        string[] arguments = Environment.GetCommandLineArgs();
        if (arguments.Length > 1) {
          //CustomTranslation.Core.InitStandalone(arguments[1]);
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);

          Application.Run(new CustomLocalizationPrepare.LangSelectForm());
        }
      } else {
        Console.WriteLine("Not exists: ");
        string managedPath = AppDomain.CurrentDomain.BaseDirectory;
        managedPath = Path.Combine(managedPath, "..");
        managedPath = Path.Combine(managedPath, "..");
        managedPath = Path.Combine(managedPath, "BattleTech_Data");
        managedPath = Path.Combine(managedPath, "Managed");
        string exeDstPath = Path.Combine(managedPath, Path.GetFileName(Application.ExecutablePath));
        string dllDstPath = Path.Combine(managedPath, "CustomLocalization.dll");
        string dll2DstPath = Path.Combine(managedPath, "EPPlus.dll");
        string exeSrcPath = Application.ExecutablePath;
        string dllSrcPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CustomLocalization.dll");
        string dll2SrcPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "EPPlus.dll");
        string logDstPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CustomTranslationApplication.log");
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
          Console.WriteLine("Copy:");
          Console.WriteLine("From:" + dll2SrcPath);
          Console.WriteLine("To:" + dllDstPath);
          File.Copy(dll2SrcPath, dll2DstPath, true);
          Console.WriteLine("success");
          ProcessStartInfo psi = new ProcessStartInfo();
          psi.WorkingDirectory = managedPath;
          psi.FileName = exeDstPath;
          psi.Arguments = "\"" + AppDomain.CurrentDomain.BaseDirectory;
          Console.WriteLine("Starting:" + psi.FileName);
          Process p = Process.Start(psi);
          while (p.WaitForExit(100) == false) {
            try { File.Copy(logSrcPath, logDstPath, true); } catch (Exception) { }
          }
          try { File.Copy(logSrcPath, logDstPath, true); } catch (Exception) { }
          File.Delete(logSrcPath);
          File.Delete(exeDstPath);
          File.Delete(dllDstPath);
          File.Delete(dll2DstPath);
        } catch (Exception e) {
          Console.WriteLine(e.ToString());
        }
      }
    }
  }
}
