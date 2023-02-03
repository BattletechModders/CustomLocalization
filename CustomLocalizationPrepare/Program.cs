using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using CustomTranslation; 

namespace CustomLocalizationPrepare {
  public static class Program {
    public static string GameRootFolder { get; set; }
    public static string ManagedFolder { get; set; }
    public static string ModsFolder { get; set; }
    private static Dictionary<string, string> assemblies = new Dictionary<string, string>();
    private static Assembly ResolveAssembly(System.Object sender, ResolveEventArgs evt) {
      Assembly res = null;
      try {
        AssemblyName assemblyName = new AssemblyName(evt.Name);
        Console.WriteLine($"request resolve assembly:{assemblyName.Name}");
        if (assemblies.TryGetValue(assemblyName.Name, out string path)) {
          res = Assembly.LoadFile(path);
        }
      } catch (Exception err) {
        Console.WriteLine(err.ToString());
      }
      return res;
    }
    public static string AssemblyDirectory {
      get {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }
    private static void GatherFolders() {
      string path = AssemblyDirectory;
      while (string.IsNullOrEmpty(path) == false) {
        Console.WriteLine(path);
        if (File.Exists(Path.Combine(path, "BattleTech.exe"))) {
          GameRootFolder = path;
          ManagedFolder = Path.Combine(path, "BattleTech_Data", "Managed");
          ModsFolder = Path.Combine(path, "Mods");
          return;
        }
        path = Path.GetDirectoryName(path);
      }
      GameRootFolder = string.Empty;
    }
    [STAThread]
    static void Main() {
      GatherFolders();
      if (string.IsNullOrEmpty(GameRootFolder)) {
        MessageBox.Show("Не могу найти исполняемый файл игры");
        return;
      }
      foreach(var path in Directory.GetFiles(ManagedFolder, "*.dll", SearchOption.AllDirectories)) {
        try {
          string name = AssemblyName.GetAssemblyName(path).Name;
          assemblies[name] = path;
          Console.WriteLine($"{name}:{path}");
        } catch (Exception) {
        }
      }
      foreach (var path in Directory.GetFiles(ModsFolder, "*.dll", SearchOption.AllDirectories)) {
        try {
          string name = AssemblyName.GetAssemblyName(path).Name;
          assemblies[name] = path;
          Console.WriteLine($"{name}:{path}");
        } catch (Exception) {
        }
      }
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly;
      AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
      Application.Run(new CustomLocalizationPrepare.LangSelectForm());
    }
  }
}
