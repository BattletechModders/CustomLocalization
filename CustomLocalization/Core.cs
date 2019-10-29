using BattleTech;
using Harmony;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;

namespace CustomTranslation {
  public static class Log {
    //private static string m_assemblyFile;
    private static string m_logfile;
    private static readonly Mutex mutex = new Mutex();
    public static string BaseDirectory;
    private static StringBuilder m_cache = new StringBuilder();
    private static StreamWriter m_fs = null;
    private static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        //Log.LogWrite("flush\n");
        //if (Core.translationSaver == null) {
        //  Core.translationSaver = UnityGameInstance.Instance.gameObject.AddComponent<TranslationSaver>();
        //}
        Log.flush();
      }
    }
    public static void InitLog() {
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomTranslation.log");
      File.Delete(Log.m_logfile);
      Log.m_fs = new StreamWriter(Log.m_logfile);
      Log.m_fs.AutoFlush = true;
      Log.flushThread.Start();
    }
    public static void flush() {
      if (Log.mutex.WaitOne(1000)) {
        Log.m_fs.Write(Log.m_cache.ToString());
        Log.m_fs.Flush();
        Log.m_cache.Length = 0;
        Log.mutex.ReleaseMutex();
      }
    }
    public static void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n", isCritical);
      } else {
        LogWrite(prefix + line, isCritical);
      }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      //try {
      if ((Core.Settings.debugLog) || (isCritical)) {
        if (Log.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          //File.AppendAllText(Log.m_logfile, line);
          Log.mutex.ReleaseMutex();
        }
        if (isCritical) { Log.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
      }
      //} catch (Exception) {
      //i'm sertanly don't know what to do
      //}
    }
  }
  [HarmonyPatch(typeof(SG_Stores_MultiPurchasePopup))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SG_Stores_MultiPurchasePopup_Refresh {
    public static void Postfix(SG_Stores_MultiPurchasePopup __instance, LocalizableText ___TitleText, string ___itemName) {
      Log.LogWrite("SG_Stores_MultiPurchasePopup.Refresh dirty hack: "+ ___itemName+"\n");
      ___TitleText.SetText(new Text("SELL: "+ ___itemName).ToString());
    }
  }

  [HarmonyPatch(typeof(Localize.Text))]
  [HarmonyPatch("Append")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Text_Append {
    public static void Localize(ref string text) {
      if (string.IsNullOrEmpty(text)) { return; };
      if (Core.localizationCache.ContainsKey(text)) {
        text = Core.localizationCache[text];
        return;
      }
      MatchCollection matches = Core.locRegEx.Matches(text);
      //Log.LogWrite(text + "\n");
      string original = text;
      if (matches.Count != 0) {
        for (int t = 0; t < matches.Count; ++t) {
          //Log.LogWrite(" '" + matches[t].Groups[1].Value + "':" + matches[t].Index + ":" + matches[t].Length + "\n");
        }
        StringBuilder newText = new StringBuilder();
        int pos = 0;
        for (int t = 0; t < matches.Count; ++t) {
          newText.Append(text.Substring(pos, matches[t].Index - pos));
          newText.Append(Core.getLocalizationString(matches[t].Groups[1].Value));
          pos = matches[t].Index + matches[t].Length;
        }
        if (pos < text.Length) { newText.Append(text.Substring(pos)); };
        text = newText.ToString();
      }
      Core.localizationCache.Add(original, text);
    }
    public static bool Prefix(Text __instance, ref string text, ref object[] args) {
      try {
        if (string.IsNullOrEmpty(text)) { return true; };
        Log.LogWrite("Localize.Text:" + text);
        Text_Append.Localize(ref text);
        Log.LogWrite("->"+text);
        if (args != null) {
          //Log.LogWrite(" params:" + args.Length + "\n");
          for (int t = 0; t < args.Length; ++t) {
            if (args[t] == null) { continue; };
            if (args[t].GetType() == typeof(System.String)) {
              string arg = (string)args[t];
              Log.LogWrite(" "+arg);
              Text_Append.Localize(ref arg);
              args[t] = arg;
              Log.LogWrite("->" + arg);
            }
            //Log.LogWrite("  string param:" + args[t] + ":"+args[t].GetType()+"\n");
          }
        }
        Log.LogWrite("\n");
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + " " + text + "\n",true);
      }
      return true;
    }
    public static void Postfix(Text __instance) {
      Log.LogWrite(" result:" + __instance+"\n");
    }
  }
  public class TranslateRecord {
    public string FileName { get; set; }
    public string Original { get; set; }
    public string Commentary { get; set; }
    public string Name { get; set; }
    public Dictionary<Localize.Strings.Culture, string> Localization { get; set; }
    public TranslateRecord() { Localization = new Dictionary<Strings.Culture, string>(); Commentary = String.Empty; }
  }
  public class CTSettings {
    public bool debugLog { get; set; }
    public CTSettings() {
      debugLog = false;
    }
  }
  public static class Core {
    public static CTSettings Settings;
    public static readonly string LocalizationFileName = "Localization.json";
    public static readonly string LocalizationRefPrefix = "__/";
    public static readonly string LocalizationRefSufix = "/__";
    public static Dictionary<string, Dictionary<Localize.Strings.Culture, string>> stringsTable = new Dictionary<string, Dictionary<Localize.Strings.Culture, string>>();
    public static Dictionary<string, string> localizationCache = new Dictionary<string, string>();
    public static void GatherLocalizations(string directory) {
      string locfile = Path.Combine(directory, Core.LocalizationFileName);
      Log.LogWrite("File:" + locfile + "\n");
      if (File.Exists(locfile)) {
        try {
          string content = File.ReadAllText(locfile);
          List<TranslateRecord> locs = JsonConvert.DeserializeObject<List<TranslateRecord>>(content);
          foreach (var loc in locs) {
            Log.LogWrite(" loc:" + loc.Name + "\n");
            foreach (var locval in loc.Localization) {
              Log.LogWrite("  " + locval.Key + ":" + locval.Value + "\n");
            }
            if (Core.stringsTable.ContainsKey(loc.Name) == false) {
              Core.stringsTable.Add(loc.Name, loc.Localization);
            } else {
              foreach (var locvals in loc.Localization) {
                if (Core.stringsTable[loc.Name].ContainsKey(locvals.Key) == false) {
                  Core.stringsTable[loc.Name].Add(locvals.Key, locvals.Value);
                }
              }
            }
          }
        } catch (Exception e) {
          Log.LogWrite(locfile + " exception " + e.ToString() + "\n");
        }
      }
      foreach (string d in Directory.GetDirectories(directory)) { GatherLocalizations(d);};
    }
    public static string getLocalizationString(string key) {
      if (Core.stringsTable.ContainsKey(key) == false) { return key; }
      Dictionary<Localize.Strings.Culture, string> cDict = Core.stringsTable[key];
      if (cDict.Count == 0) { return key; };
      if (cDict.ContainsKey(Localize.Strings.CurrentCulture) == false) { return cDict.First().Value; }
      return cDict[Localize.Strings.CurrentCulture];
    }
    //public static Dictionary<string, TranslateRecord> flushableTranslation = new Dictionary<string, TranslateRecord>();
    //public static TranslationSaver translationSaver = null;
    public static Regex locRegEx = new Regex("[_]{2}[/]{1}([a-zA-Z0-9\\._\\-\\,]*)[/]{1}[_]{2}");
    public static void SaveCurrentTranslation() {
      //string path = Path.Combine(Log.BaseDirectory, Core.Settings.language+"_locTable.json");
      //File.WriteAllText(path, JsonConvert.SerializeObject(flushableTranslation,Formatting.Indented));
      //Log.LogWrite("saved translation to "+path+"\n", true);
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      //settingsJson.get
      Core.Settings = JsonConvert.DeserializeObject<CustomTranslation.CTSettings>(settingsJson);
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      try {
        Core.GatherLocalizations(Path.Combine(directory, ".."));
        var harmony = HarmonyInstance.Create("io.mission.customlocalization");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        //translationSaver = new GameObject();
        //translationSaver.AddComponent<TranslationSaver>();
        //translationSaver.SetActive(true);
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
  }
}
