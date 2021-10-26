using BattleTech;
using BattleTech.Data;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS;
using HBS.Util;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using static BattleTech.Data.DataManager;

namespace CustomTranslation {
  public class DebugLogFile {
    private string m_logfile;
    private SpinLock spinLock = new SpinLock();
    private StringBuilder m_cache = new StringBuilder();
    private StreamWriter m_fs = null;
    public DebugLogFile(string filename) {
      this.m_logfile = Path.Combine(Log.BaseDirectory, filename);
      File.Delete(this.m_logfile);
      this.m_fs = new StreamWriter(this.m_logfile);
      this.m_fs.AutoFlush = true;
    }
    public void flush() {      
      bool locked = false;
      try {
      if (spinLock.IsHeldByCurrentThread == false) { spinLock.Enter(ref locked); }
        this.m_fs.Write(this.m_cache.ToString());
        this.m_fs.Flush();
        this.m_cache.Length = 0;          
      } finally {
        if (locked) { spinLock.Exit(); }
      }
    }
    public void W(string line, bool isCritical = false) {
      LogWrite(line, isCritical);
    }
    public void WL(string line, bool isCritical = false) {
      line += "\n"; W(line, isCritical);
    }
    public void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; W(line, isCritical);
    }
    public void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; WL(line, isCritical);
    }
    public void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line, isCritical);
    }
    public void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line, isCritical);
    }
    public void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
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
    public void LogWrite(string line, bool isCritical = false) {
      bool locked = false;
      try {
        if (spinLock.IsHeldByCurrentThread == false) { spinLock.Enter(ref locked); }
        m_cache.Append(line);
        if (isCritical) { this.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { this.flush(); };
      } finally {
        if(locked)spinLock.Exit();
      }
    }

  }
  public static class Log {
    //private static string m_assemblyFile;
    private static DebugLogFile LogFile = null;
    private static DebugLogFile InterpolationReport = null;
    public static DebugLogFile M { get { return Core.Settings.debugLog ? LogFile : null; } }
    public static DebugLogFile Er { get { return LogFile; } }
    public static DebugLogFile I { get { return InterpolationReport; } }
    public static string BaseDirectory;
    public static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        //Log.M?.LogWrite("flush\n");
        //if (Core.translationSaver == null) {
        //  Core.translationSaver = UnityGameInstance.Instance.gameObject.AddComponent<TranslationSaver>();
        //}
        LogFile.flush();
        InterpolationReport.flush();
      }
    }
    public static void InitLog() {
      //if (Core.Settings.debugLog) {
      LogFile = new DebugLogFile("CustomLocalization.log");
      InterpolationReport = new DebugLogFile("InterpolationReport.log");
      //}
      flushThread.Start();
    }
  }
  [HarmonyPatch(typeof(SG_Stores_MultiPurchasePopup))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SG_Stores_MultiPurchasePopup_Refresh {
    public static void Postfix(SG_Stores_MultiPurchasePopup __instance, LocalizableText ___TitleText, string ___itemName) {
      Log.M?.LogWrite("SG_Stores_MultiPurchasePopup.Refresh dirty hack: "+ ___itemName+"\n");
      ___TitleText.SetText(new Text("SELL: "+ ___itemName).ToString());
    }
  }
  [HarmonyPatch(typeof(Interpolator))]
  [HarmonyPatch("Interpolate")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Interpolator_Interpolate {
    private static Dictionary<GameContextObjectTagEnum, string> enumToDesignerMap = null;
    private static HashSet<Type> returnTypes = new HashSet<Type>() { typeof(string), typeof(int), typeof(float), typeof(bool) };
    public static void Prefix(ref string template, GameContext context) {
      try {
        Log.M?.LogWrite("Interpolator.Interpolate " + template + "->");
        Text_Append.Localize(ref template);
        Log.M?.LogWrite(template+"\n");
        Log.I.TWL(0,"Шаблон:"+template,true);
        Log.I.WL(1, "Контекст:");
        if(enumToDesignerMap == null) {
          Type GameContextEnumTagToDesignerTag = typeof(GameContext).Assembly.GetType("GameContextEnumTagToDesignerTag");
          enumToDesignerMap = (Dictionary<GameContextObjectTagEnum, string>)GameContextEnumTagToDesignerTag.GetField("enumToDesignerMap", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        }
        foreach (var citem in Traverse.Create(context).Field<Dictionary<GameContextObjectTagEnum, object>>("context").Value) {
          Log.I.WL(2, enumToDesignerMap[citem.Key]+":"+ citem.Value.GetType().ToString());
          PropertyInfo[] properties = citem.Value.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
          foreach(PropertyInfo prop in properties) {
            if (prop.CanRead == false) { continue; }
            if (returnTypes.Contains(prop.PropertyType) == false) { continue; }
            Log.I.WL(3, prop.Name+":"+prop.PropertyType.Name);
          }
          FieldInfo[] fields = citem.Value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          foreach (FieldInfo field in fields) {
            if (returnTypes.Contains(field.FieldType) == false) { continue; }
            Log.I.WL(3, field.Name+":"+field.FieldType.Name);
          }
          MethodInfo[] methods = citem.Value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          foreach(MethodInfo method in methods) {
            if (method.GetParameters().Length > 0) { continue; }
            if (method.Name.StartsWith("get_")) { continue; }
            if (method.IsAbstract) { continue; }
            if (method.IsGenericMethod) { continue; }
            if (returnTypes.Contains(method.ReturnParameter.ParameterType) == false) { continue; }
            Log.I.WL(3, method.Name+":"+method.ReturnParameter.ParameterType.Name);
          }
        }
      } catch(Exception e) {
        Log.Er?.LogWrite(e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CSVStringsProvider))]
  [HarmonyPatch("LoadCultureFromReader")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CSVStringsProvider_LoadCultureFromReader {
    public static void Postfix(CSVStringsProvider __instance) {
      try {
        Log.M?.TWL(0,"CSVStringsProvider.LoadCultureFromReader " + __instance.CurrentCulture);
        if (Core.currentCulture.currentCulture != __instance.CurrentCulture) {
          Core.currentCulture.currentCulture = __instance.CurrentCulture;
          File.WriteAllText(Core.Settings.cultureSettingsFilePath, JsonConvert.SerializeObject(Core.currentCulture, Formatting.Indented));
          switch (Core.currentCulture.currentCulture) {
            case Strings.Culture.CULTURE_RU_RU: GenericPopupBuilder.Create("ПОЖАЛУЙСТА ПЕРЕЗАПУСТИТЕ ПРИЛОЖЕНИЕ", "Вам надо перезапустить приложение, чтобы локализация правильно применилась").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
            default: GenericPopupBuilder.Create("PLEASE RESTART", "You should restart for localization to take effect").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
          }
        }
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(StreamReader))]
  [HarmonyPatch(new Type[] { typeof(string) })]
  [HarmonyPatch(MethodType.Constructor)]
  public static class StreamReader_Constructor {
    public delegate void d_Init(StreamReader reader,Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen);
    public static d_Init i_Init = null;
    public static Stream GenStream(this string s) {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(s);
      writer.Flush();
      stream.Position = 0;
      return stream;
    }
    public static bool Prepare() {
      {
        MethodInfo method = typeof(StreamReader).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Stream), typeof(Encoding), typeof(bool), typeof(int), typeof(bool) }, null);
        var dm = new DynamicMethod("CLStreamReaderInit", null, new Type[] { typeof(StreamReader),typeof(Stream), typeof(Encoding), typeof(bool), typeof(int), typeof(bool) }, typeof(StreamReader));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);
        gen.Emit(OpCodes.Ldarg_S, 5);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_Init = (d_Init)dm.CreateDelegate(typeof(d_Init));
      }
      Log.M?.TWL(0, "StreamReader_Constructor.Prepare i_Init " + (i_Init == null ? "null" : "not null"));
      return true;
    }
    public static void Init(this StreamReader reader, Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen) {
      i_Init(reader, stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
    }
    public static bool Prefix(StreamReader __instance,ref string path) {
      try {
        if (Path.GetExtension(path).ToUpper() != ".JSON") { return true; }
        path = Path.GetFullPath(path);
        Log.M?.WL("StreamReader:"+ path);
        HashSet<jtProcGenericEx> procs = path.getLocalizationProcs();
        if (procs == null) { return true; }
        string content = File.ReadAllText(path, Encoding.UTF8);
        bool updated = Core.LocalizeString(ref content, path, procs);
        //object jcontent = JObject.Parse(content);
        //string filename = Path.GetFileNameWithoutExtension(path);
        //bool updated = false;
        //foreach (jtProcGenericEx proc in procs) {
        //  try {
        //    Log.M?.WL(1, proc.Name);
        //    if (proc.proc(string.Empty, filename, ref jcontent)) { updated = true; }
        //  } catch (Exception e) {
        //    Log.M?.TWL(0, e.ToString(), true);
        //  }
        //}
        //content = (jcontent as JObject).ToString(Formatting.Indented);
        //if (updated) { Log.M?.WL(0, Core.currentCulture.currentCulture.ToString() + ":" + content); }
        __instance.Init(content.GenStream(),Encoding.UTF8,true,1024,false);
        return false;
      } catch (Exception e) {
        Log.Er?.TWL(0,e.ToString(), true);
      }
      return true;
    }
  }
  /*[HarmonyPatch(typeof(File))]
  [HarmonyPatch("ReadAllText")]
  [HarmonyPatch(new Type[] { typeof(string) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class File_ReadAllText {
    public static bool Prefix(ref string path, ref string __result) {
      try {
        if (Path.GetExtension(path).ToUpper() != ".JSON") { return true; }
        path = Path.GetFullPath(path);
        Log.M?.WL("ReadAllText:" + path);
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString(), true);
      }
      return true;
    }
  }*/
  [HarmonyPatch(typeof(HBS.Data.DataLoader))]
  [HarmonyPatch("CallHandler")]
  [HarmonyPatch(new Type[] { typeof(string),typeof(Action<string, Stream>) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class DataLoader_CallHandler {
    public static bool Prefix(ref string path, Action<string, Stream> handler) {
      try {
        Log.M?.TWL(0, "CallHandler:" + path+":"+ Path.GetExtension(path).ToUpper());
        if (Path.GetExtension(path).ToUpper() == ".JSON") { return true; }
        HashSet<jtProcGenericEx> procs = path.getLocalizationProcs();
        if (procs == null) { return true; }
        string content = File.ReadAllText(path, Encoding.UTF8);
        bool updated = Core.LocalizeString(ref content, path, procs);
        //object jcontent = JObject.Parse(content);
        //string filename = Path.GetFileNameWithoutExtension(path);
        //bool updated = false;
        //foreach (jtProcGenericEx proc in procs) {
        //  try {
        //    Log.M?.WL(1, proc.Name);
        //    if (proc.proc(string.Empty, filename, ref jcontent)) { updated = true; }
        //  }catch(Exception e) {
        //    Log.M?.TWL(0, e.ToString(), true);
        //  }
        //}
        //content = (jcontent as JObject).ToString(Formatting.Indented);
        //if (updated) { Log.M?.WL(0, Strings.CurrentCulture.ToString() + ":" + content); }
        handler(path,content.GenStream());
        return false;
      } catch (Exception e) {
        Log.Er?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(InterpolatedText))]
  [HarmonyPatch("Append")]
  [HarmonyPatch(MethodType.Normal)]
  public static class InterpolatedText_Append {
    public static bool Prefix(ref string text, ref object[] args) {
      try {
        if (string.IsNullOrEmpty(text)) { return true; };
        Log.M?.LogWrite("InterpolatedText.Append:" + text);
        Text_Append.Localize(ref text);
        Log.M?.LogWrite("->" + text);
        if (args != null) {
          //Log.M?.LogWrite(" params:" + args.Length + "\n");
          for (int t = 0; t < args.Length; ++t) {
            if (args[t] == null) { continue; };
            if (args[t].GetType() == typeof(System.String)) {
              string arg = (string)args[t];
              Log.M?.LogWrite(" " + arg);
              Text_Append.Localize(ref arg);
              args[t] = arg;
              Log.M?.LogWrite("->" + arg);
            }
            //Log.M?.LogWrite("  string param:" + args[t] + ":"+args[t].GetType()+"\n");
          }
        }
        Log.M?.LogWrite("\n");
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + " " + text + "\n", true);
      }
      return true;
    }
    public static void Postfix(InterpolatedText __instance) {
      //Log.M?.LogWrite(" result:" + __instance + "\n");
    }
  }
  [HarmonyPatch(typeof(ShipModuleUpgrade))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class ShipModuleUpgrade_FromJSON {
    public static void Postfix(ShipModuleUpgrade __instance, string json) {
      try {
        Log.M?.TWL(0,"ShipModuleUpgrade.FromJSON "+ __instance.Description.Id);
        Log.M?.WL(1,Environment.StackTrace);
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + "\n", true);
      }
    }
  }
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class StringDataLoadRequest_OnLoadedWithText {
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(StringDataLoadRequest<WeaponDef>), "OnLoadedWithText");
    }
    public static void Prefix(VersionManifestEntry ___manifestEntry, ref string text) {
      try {
        string resourceId = ___manifestEntry.Id;
        string path = ___manifestEntry.FilePath;
        string ext = Path.GetExtension(path).ToUpper();
        Log.M?.TWL(0, "OnLoadedWithText:" + resourceId + ":" + path+":"+ ext);
        if (ext != ".JSON") { return; }
        HashSet<jtProcGenericEx> procs = path.getLocalizationProcs();
        if (procs == null) { return; }
        bool updated = Core.LocalizeString(ref text, path, procs);
        //object jcontent = JObject.Parse(text);
        //bool updated = false;
        //foreach (jtProcGenericEx proc in procs) {
        //  try {
        //    Log.M?.WL(1, proc.Name);
        //    if (proc.proc(string.Empty, resourceId, ref jcontent)) { updated = true; }
        //  } catch (Exception e) {
        //    Log.M?.TWL(0, e.ToString(), true);
        //  }
        //}
        //text = (jcontent as JObject).ToString(Formatting.Indented);
        //if (updated) { Log.M?.WL(0, Strings.CurrentCulture.ToString() + ":" + text); }
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + "\n", true);
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechDef")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class MechValidationRules_ValidateMechDef {
    public static void Postfix(MechValidationLevel validationLevel, DataManager dataManager, MechDef mechDef, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> __result) {
      try {
        Log.M?.LogWrite("MechValidationRules.ValidateMechDef\n");
        foreach (var vtype in __result) {
          for (int index = 0; index < vtype.Value.Count; ++index){
            if (vtype.Value[index] == null) {
              Log.M?.LogWrite(" null detected: "+ vtype.Key.ToString()+" "+index+"\n");
              vtype.Value[index] = new Text("Hey! Wanker fix this! "+vtype.Key.ToString() + "["+index+"]");
            }
          }
        }
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + "\n", true);
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("GetMechFieldableWarnings")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_GetMechFieldableWarnings {
    public static void Postfix(DataManager dataManager, MechDef mechDef,ref List<Text> __result) {
      try {
        Log.M?.LogWrite("MechValidationRules.GetMechFieldableWarnings\n");
        if (__result == null) { __result = new List<Text>(); }
        for(int index=0;index < __result.Count; ++index) {
          if (__result[index] == null) { __result[index] = new Text("NULL"); }
        }
      }catch(Exception e) {
        Log.Er?.LogWrite(e.ToString()+"\n",true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDFloatie))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDFloatie_Init {
    public static void Prefix(CombatHUDFloatie __instance, CombatHUD HUD, Text text, float fontSize, Color color, Transform parent, Vector3 worldPos, CombatHUDFloatie.OnDeath onDeath) {
      //Log.M?.LogWrite("CombatHUDFloatie.Init prefix:"+text.ToString()+"\n");
    }
    public static void Postfix(CombatHUDFloatie __instance, CombatHUD HUD, Text text, float fontSize, Color color, Transform parent, Vector3 worldPos, CombatHUDFloatie.OnDeath onDeath) {
      //Log.M?.LogWrite("CombatHUDFloatie.Init postfix:" + __instance.floatieText.text + "\n");
    }
  }
  [HarmonyPatch(typeof(CombatHUDStatusStackItem))]
  [HarmonyPatch("SetDescription")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDStatusStackItem_SetDescription {
    public static void Prefix(CombatHUDStatusStackItem __instance, ref Text description) {
      //if (description.ToString() == "УКЛОНЕНИЕ") { description = new Text("EVASIVE"); };
      //Log.M?.LogWrite("CombatHUDStatusStackItem.SetDescription prefix:" + description.ToString() + "\n");
    }
    public static void Postfix(CombatHUDStatusStackItem __instance, Text description, LocalizableText ___Text) {
      //Log.M?.LogWrite("CombatHUDStatusStackItem.SetDescription postfix:" + ___Text.text + "\n");
    }
  }
  [HarmonyPatch(typeof(FontLocTable))]
  [HarmonyPatch("ConvertFontForCulture")]
  [HarmonyPatch(MethodType.Normal)]
  public static class FontLocTable_ConvertFontForCulture {
    public static bool Prefix(FontLocTable __instance, Strings.Culture curCulture, TMP_FontAsset curFont, ref TMP_FontAsset __result) {
      Log.M?.TWL(0, "FontLocTable.ConvertFontForCulture prefix:" + (curFont == null ? "null" : curFont.name) + ":" + curFont.atlas.name);
      if (Core.Settings.fontsReplacementTable.TryGetValue(curCulture, out var replaceFontsTable)) {
        if (replaceFontsTable.TryGetValue(curFont.name, out string replaceFont)) {
          if (Core.fonts.TryGetValue(replaceFont, out TMP_FontAsset font)) {
            __result = font;
            return false;
          } else {
            Log.M?.WL(1, "can't find " + replaceFont);
          }
        } else {
          Log.M?.WL(1, "not in preplacement table");
        }
      }
      return true;
    }
    public static void Postfix(FontLocTable __instance, Strings.Culture curCulture, TMP_FontAsset curFont, ref TMP_FontAsset __result) {
      Log.M?.TWL(0, "FontLocTable.ConvertFontForCulture postfix:" + (__result == null ? "null" : __result.name) + ":" + __result.atlas.name);
      if (Core.Settings.fontsReplacementTable.TryGetValue(curCulture, out var replaceFontsTable)) {
        if (replaceFontsTable.TryGetValue(__result.name, out string replaceFont)) {
          if (Core.fonts.TryGetValue(replaceFont, out TMP_FontAsset font)) {
            __result = font;
          } else {
            Log.M?.WL(1, "can't find " + replaceFont);
          }
        } else {
          Log.M?.WL(1, "not in preplacement table");
        }
      }
      Log.M?.TWL(0, "FontLocTable.ConvertFontForCulture:" + (__result == null ? "null":__result.name)+":"+__result.atlas.name);
    }
  }
  [HarmonyPatch(typeof(CombatHUDInWorldElementMgr))]
  [HarmonyPatch("AddFloatieMessage")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDInWorldElementMgr_AddFloatieMessage {
    public static void Prefix(CombatHUDInWorldElementMgr __instance, MessageCenterMessage message) {
      //FloatieMessage msg = message as FloatieMessage;
      //Log.M?.LogWrite("CombatHUDInWorldElementMgr.AddFloatieMessage prefix:" + msg.text.ToString() + ":" + msg.nature + "\n");
    }
  }
  [HarmonyPatch(typeof(FloatieMessage))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(Text), typeof(float), typeof(FloatieMessage.MessageNature), typeof(float), typeof(float), typeof(float) })]

  public static class FloatieMessage_Constructor {
    public static void Prefix(string attackerGuid, string targetGuid, Text text, float fontSize, FloatieMessage.MessageNature nature, float vX, float vY, float vZ) {
      //Log.M?.LogWrite("FloatieMessage.Constructor prefix:" + text.ToString() + ":"+fontSize+":"+nature+"\n");
    }
  }
  [HarmonyPatch(typeof(CombatHUDFloatieAnchor))]
  [HarmonyPatch("AddFloatie")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDFloatieAnchor_AddFloatie {
    public static void Prefix(CombatHUDFloatieAnchor __instance, Text text, float fontSize, FloatieMessage.MessageNature nature, Vector3 worldPos) {
      //Log.M?.LogWrite("CombatHUDFloatieAnchor.AddFloatie prefix:" + text.ToString() + ":"+fontSize+":"+nature+"\n");
    }
  }
  [HarmonyPatch(typeof(UnityGameInstance))]
  [HarmonyPatch("Reset")]
  [HarmonyPatch(MethodType.Normal)]
  public static class UnityGameInstance_Reset {
    public static void Postfix(UnityGameInstance __instance) {
      Log.M?.TWL(0, "font localization table:");
      try {
        foreach (FontLocTable fontLocTable in SceneSingletonBehavior<UnityGameInstance>.Instance.fontLocalizationSource.AllLocTables) {
          Log.M?.WL(1, fontLocTable.Culture.ToString());
          IEnumerable fontsList = (IEnumerable)Traverse.Create(fontLocTable).Field("m_fontConversions").GetValue();
          foreach (var fontRec in fontsList) {
            TMP_FontAsset baseFont = Traverse.Create(fontRec).Field<TMP_FontAsset>("baseFont").Value;
            TMP_FontAsset replacementFont = Traverse.Create(fontRec).Field<TMP_FontAsset>("replacementFont").Value;
            Log.M?.WL(2, baseFont.name+"->"+ replacementFont.name);
            if (Core.fonts.ContainsKey(baseFont.name) == false) { Core.fonts.Add(baseFont.name,baseFont); }
            if (Core.fonts.ContainsKey(replacementFont.name) == false) { Core.fonts.Add(replacementFont.name, replacementFont); }
          }
        };
      }catch(Exception e) {
        Log.Er?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MainMenu))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MainMenu_Init {
    public static void Postfix(MainMenu __instance) {
      Core.SubscribeOnLanguageChanged();
      try {
        Log.M?.TWL(0, "MainMenu.Init " + Strings.CurrentCulture);
        if (Core.currentCulture.currentCulture != Strings.CurrentCulture) {
          Core.currentCulture.currentCulture = Strings.CurrentCulture;
          Core.CallResetCacheActions();
          File.WriteAllText(Core.Settings.cultureSettingsFilePath, JsonConvert.SerializeObject(Core.currentCulture, Formatting.Indented));
          switch (Core.currentCulture.currentCulture) {
            case Strings.Culture.CULTURE_RU_RU: GenericPopupBuilder.Create("ПОЖАЛУЙСТА ПЕРЕЗАПУСТИТЕ ПРИЛОЖЕНИЕ", "Вам надо перезапустить приложение, что бы локализация правильно применилась").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
            default: GenericPopupBuilder.Create("PLEASE RESTART", "You should restart for localization to take effect").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
          }
        }
      } catch (Exception e) {
        Log.Er?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(LanguageSelect))]
  [HarmonyPatch("Save")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LanguageSelect_Save {
    public static void Postfix(LanguageSelect __instance) {
      try {
        Log.M?.TWL(0, "LanguageSelect.Save " + Strings.CurrentCulture);
        if (Core.currentCulture.currentCulture != Strings.CurrentCulture) {
          Core.currentCulture.currentCulture = Strings.CurrentCulture;
          Core.CallResetCacheActions();
          File.WriteAllText(Core.Settings.cultureSettingsFilePath, JsonConvert.SerializeObject(Core.currentCulture, Formatting.Indented));
          switch (Core.currentCulture.currentCulture) {
            case Strings.Culture.CULTURE_RU_RU: GenericPopupBuilder.Create("ПОЖАЛУЙСТА ПЕРЕЗАПУСТИТЕ ПРИЛОЖЕНИЕ", "Вам надо перезапустить приложение, что бы локализация правильно применилась").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
            default: GenericPopupBuilder.Create("PLEASE RESTART", "You should restart for localization to take effect").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render(); break;
          }
        }
      } catch (Exception e) {
        Log.Er?.TWL(0, e.ToString());
      }
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
      //Log.M?.LogWrite(text + "\n");
      string original = text;
      if (matches.Count != 0) {
        for (int t = 0; t < matches.Count; ++t) {
          //Log.M?.LogWrite(" '" + matches[t].Groups[1].Value + "':" + matches[t].Index + ":" + matches[t].Length + "\n");
        }
        StringBuilder newText = new StringBuilder();
        int pos = 0;
        for (int t = 0; t < matches.Count; ++t) {
          newText.Append(text.Substring(pos, matches[t].Index - pos));
          newText.Append(Core.getLocalizationString(matches[t].Groups[1].Value.ToUpper(CultureInfo.InvariantCulture)));
          pos = matches[t].Index + matches[t].Length;
        }
        if (pos < text.Length) { newText.Append(text.Substring(pos)); };
        text = newText.ToString();
      }
      Core.localizationCache.Add(original, text);
    }
    public static void Localize(ref string text, Strings.Culture culture) {
      if (string.IsNullOrEmpty(text)) { return; };
      if (Core.localizationCache.ContainsKey(text)) {
        text = Core.localizationCache[text];
        return;
      }
      MatchCollection matches = Core.locRegEx.Matches(text);
      //Log.M?.LogWrite(text + "\n");
      string original = text;
      if (matches.Count != 0) {
        for (int t = 0; t < matches.Count; ++t) {
          //Log.M?.LogWrite(" '" + matches[t].Groups[1].Value + "':" + matches[t].Index + ":" + matches[t].Length + "\n");
        }
        StringBuilder newText = new StringBuilder();
        int pos = 0;
        for (int t = 0; t < matches.Count; ++t) {
          newText.Append(text.Substring(pos, matches[t].Index - pos));
          newText.Append(Core.getLocalizationString(matches[t].Groups[1].Value.ToUpper(CultureInfo.InvariantCulture),culture));
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
        Log.M?.LogWrite("Localize.Text:" + text);
        Text_Append.Localize(ref text);
        Log.M?.LogWrite("->"+text);
        if (args != null) {
          //Log.M?.LogWrite(" params:" + args.Length + "\n");
          for (int t = 0; t < args.Length; ++t) {
            if (args[t] == null) { continue; };
            if (args[t].GetType() == typeof(System.String)) {
              string arg = (string)args[t];
              Log.M?.LogWrite(" "+arg);
              Text_Append.Localize(ref arg);
              args[t] = arg;
              Log.M?.LogWrite("->" + arg);
            }
            //Log.M?.LogWrite("  string param:" + args[t] + ":"+args[t].GetType()+"\n");
          }
        }
        Log.M?.LogWrite("\n");
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + " " + text + "\n",true);
      }
      return true;
    }
    public static void Postfix(Text __instance) {
      Log.M?.LogWrite(" result:" + __instance+"\n");
    }
  }
  public class TranslateRecord {
    public string Original { get; set; }
    public string Commentary { get; set; }
    public string Name { get; set; }
    public Dictionary<Localize.Strings.Culture, string> Localization { get; set; }
    public TranslateRecord() { Localization = new Dictionary<Strings.Culture, string>(); Commentary = String.Empty; }
  }
  public class MetaEvaluator {
    public string Name { get; set; }
    public Dictionary<string, Dictionary<Localize.Strings.Culture, Dictionary<string,string>>> Localization { get; set; }
    public MetaEvaluator() {
      Localization = new Dictionary<string, Dictionary<Strings.Culture, Dictionary<string, string>>>();
    }
    public List<TranslateRecord> expand(TranslateRecord locRec) {
      List<TranslateRecord> result = new List<TranslateRecord>();
      if (locRec.Name.Contains(this.Name) == false) { return result; }
      foreach(var metaValue in Localization) {
        TranslateRecord eRec = new TranslateRecord();
        eRec.Original = locRec.Original;
        eRec.Commentary = locRec.Commentary;
        eRec.Name = locRec.Name.Replace(this.Name, metaValue.Key);
        foreach(var locValue in locRec.Localization) {
          string eLocalization = locValue.Value;
          if(metaValue.Value.TryGetValue(locValue.Key, out var metaReplaces)) {
            foreach(var metaReplace in metaReplaces) {
              eLocalization = eLocalization.Replace(metaReplace.Key,metaReplace.Value);
            }
          }
          eRec.Localization.Add(locValue.Key, eLocalization);
        }
        result.Add(eRec);
      }
      return result;
    }
    public static List<TranslateRecord> evaluate(TranslateRecord locRec,ref HashSet<MetaEvaluator> evaluators) {
      List<TranslateRecord> result = new List<TranslateRecord>();
      if (evaluators.Count == 0) { return result; }
      MetaEvaluator evaluator = evaluators.First();
      evaluators.Remove(evaluator);
      List<TranslateRecord> tresult = evaluator.expand(locRec);
      foreach(TranslateRecord rec in tresult) {
        List<TranslateRecord> tres = MetaEvaluator.evaluate(rec, ref evaluators);
        result.Add(rec);
        result.AddRange(tres);
      }
      return result;
    }
  }
  public enum LocalizationProcType {
    Dummy,Key,Content,None
  }
  public class CTSettings {
    [JsonIgnore]
    public string cultureSettingsFilePath;
    public bool debugLog { get; set; }
    public List<MetaEvaluator> metaEvaluators { get; set; }
    public LocalizationProcType localizationProcType { get; set; }
    public Strings.Culture defaultCulture { get; set; }
    public Dictionary<Strings.Culture, Dictionary<string, string>> fontsReplacementTable { get; set; }
    public CTSettings() {
      debugLog = false;
      metaEvaluators = new List<MetaEvaluator>();
      localizationProcType = LocalizationProcType.Dummy;
      fontsReplacementTable = new Dictionary<Strings.Culture, Dictionary<string, string>>();
      defaultCulture = Strings.Culture.CULTURE_EN_US;
    }
    public HashSet<MetaEvaluator> listEvaluators(string key) {
      HashSet<MetaEvaluator> result = new HashSet<MetaEvaluator>();
      foreach(MetaEvaluator metaEvaluator in metaEvaluators) {
        if (key.Contains(metaEvaluator.Name)) { result.Add(metaEvaluator); };
      }
      return result;
    }
  }
  public class COnLanguageChanged { 
  }
  public class CLCultureSettings {
    public Strings.Culture currentCulture { get; set; }
    public CLCultureSettings() {
      currentCulture = Strings.Culture.CULTURE_EN_US;
    }
  }
  public static class Core {
    private static HashSet<Action> resetCacheActions = new HashSet<Action>();
    public static CTSettings Settings;
    public static CLCultureSettings currentCulture = new CLCultureSettings();
    public static readonly string LocalizationFileName = "Localization.json";
    public static readonly string LocalizationDefPrefix = "Localization";
    public static readonly string LocalizationDefSuffix = "Def";
    public static readonly string LocalizationRefPrefix = "__/";
    public static readonly string LocalizationRefSufix = "/__";
    public static Dictionary<string, Dictionary<Localize.Strings.Culture, string>> stringsTable = new Dictionary<string, Dictionary<Localize.Strings.Culture, string>>();
    public static Dictionary<string, string> localizationCache = new Dictionary<string, string>();
    public static Dictionary<string, jtProcGenericEx> localizationMethods = new Dictionary<string, jtProcGenericEx>();
    public static Dictionary<string, HashSet<jtProcGenericEx>> proccessDirectories = new Dictionary<string, HashSet<jtProcGenericEx>>();
    public static Dictionary<string, HashSet<jtProcGenericEx>> proccessFiles = new Dictionary<string, HashSet<jtProcGenericEx>>();
    public static Dictionary<string, LocalizationDef> loadedDefinitions = new Dictionary<string, LocalizationDef>();
    public static HashSet<string> affectedFiles = new HashSet<string>();
    public static HashSet<string> skipLocalizationProc = new HashSet<string>();
    public static Dictionary<string, TMP_FontAsset> fonts = new Dictionary<string, TMP_FontAsset>();
    public static List<string> PathToList(string filename) {
      List<string> result = new List<string>();
      do {
        string dir = Path.GetFileName(filename);
        if (string.IsNullOrEmpty(dir) == false) { result.Add(dir); };
        filename = Path.GetDirectoryName(filename);
      } while (string.IsNullOrEmpty(filename) == false);
      result.Reverse();
      return result;
    }
    public static HashSet<jtProcGenericEx> getLocalizationProcs(this string path) {
      if (Core.currentCulture.currentCulture == Core.Settings.defaultCulture) { return null; }
      string filename = Path.GetFileNameWithoutExtension(path);
      if (Core.proccessFiles.TryGetValue(filename, out HashSet<jtProcGenericEx> procs)) {
        Log.M?.WL(1, "found filename:" + filename);
        return procs;
      }
      string dir = Path.GetDirectoryName(Path.GetFullPath(path));
      if (Core.proccessDirectories.TryGetValue(dir, out procs)) {
        Log.M?.WL(1, "found directory:" + dir);
        return procs;
      }
      return null;
    }
    public static void CallResetCacheActions() {
      foreach (Action act in resetCacheActions) { act(); }
    }
    public static void RegisterResetCache(Action action) {
      resetCacheActions.Add(action);
    }
    public static bool LocalizeString(ref string content, string path, HashSet<jtProcGenericEx> procs) {
      if (Core.currentCulture.currentCulture == Core.Settings.defaultCulture) { return false; }
      if (procs == null) { return false; }
      object jcontent = JObject.Parse(content);
      string filename = Path.GetFileNameWithoutExtension(path);
      bool updated = false;
      foreach (jtProcGenericEx proc in procs) {
        try {
          Log.M?.WL(1, proc.Name);
          if (proc.proc(string.Empty, filename, ref jcontent)) { updated = true; }
        } catch (Exception e) {
          Log.Er?.TWL(0, e.ToString(), true);
        }
      }
      content = (jcontent as JObject).ToString(Formatting.Indented);
      if (updated) { Log.M?.WL(0, Core.currentCulture.currentCulture.ToString() + ":" + content); }
      return updated;
    }
    public static void InitStructure() {
      foreach (Type locMethod in typeof(Core).Assembly.GetTypes()) {
        if (typeof(jtProcGenericEx).IsAssignableFrom(locMethod)) {
          if (locMethod.IsAbstract) { continue; }
          jtProcGenericEx lMethod = locMethod.GetConstructor(new Type[] { }).Invoke(new object[] { }) as jtProcGenericEx;
          if (lMethod != null) {
            if (localizationMethods.ContainsKey(lMethod.Name)) {
              Log.M?.TWL(0, locMethod.ToString() +" have same name "+ localizationMethods[lMethod.Name].GetType().ToString(), true);
            } else {
              localizationMethods.Add(lMethod.Name, lMethod);
            }
          };
        }
      }
      skipLocalizationProc.Add("MOD");
      skipLocalizationProc.Add("LOCALIZATION");
      skipLocalizationProc.Add("SETTINGS");
      skipLocalizationProc.Add("MODSTATE");
      skipLocalizationProc.Add("CUSTOMAMMOCATEGORIESSETTINGS");
      skipLocalizationProc.Add("CUSTOMAMMOCATEGORIES");
      skipLocalizationProc.Add("WEAPONREALIZERSETTINGS");
      skipLocalizationProc.Add("AIM_SETTINGS");
      skipLocalizationProc.Add("BASELOCALIZATION");
    }
    public static void SubscribeOnLanguageChanged() {
      UnityGameInstance.Instance.Game.MessageCenter.AddSubscriber(MessageCenterMessageType.OnLanguageChanged, new ReceiveMessageCenterMessage(Core.OnLanguageChanged));
    }
    public static void OnLanguageChanged(MessageCenterMessage msg) {
      Log.M?.LogWrite("Language changed!");
      localizationCache.Clear();
    }
    public static void AddTranslationRecord(TranslateRecord bloc) {
      List<TranslateRecord> elocs = new List<TranslateRecord>();
      HashSet<MetaEvaluator> metaEvaluators = Core.Settings.listEvaluators(bloc.Name);
      if (metaEvaluators.Count == 0) { elocs.Add(bloc); } else {
        elocs = MetaEvaluator.evaluate(bloc, ref metaEvaluators);
        elocs.Add(bloc);
      }
      foreach (var loc in elocs) {
        string name = loc.Name.ToUpper(CultureInfo.InvariantCulture);
        Log.M?.LogWrite(" loc:" + name + "\n");
        foreach (var locval in loc.Localization) {
          Log.M?.LogWrite("  " + locval.Key + ":" + locval.Value + "\n");
        }
        if (Core.stringsTable.ContainsKey(name) == false) {
          Core.stringsTable.Add(name, loc.Localization);
        } else {
          foreach (var locvals in loc.Localization) {
            if (Core.stringsTable[name].ContainsKey(locvals.Key) == false) {
              Core.stringsTable[name].Add(locvals.Key, locvals.Value);
            }
          }
        }
      }
    }
    public static void GatherLocalizations(string directory) {
      string locfile = Path.Combine(directory, Core.LocalizationFileName);
      Log.M?.LogWrite("File:" + locfile + "\n");
      if (File.Exists(locfile)) {
        try {
          string content = File.ReadAllText(locfile);
          List<TranslateRecord> locs = JsonConvert.DeserializeObject<List<TranslateRecord>>(content);
          
          foreach (var bloc in locs) {
            AddTranslationRecord(bloc);
          }
        } catch (Exception e) {
          Log.Er?.LogWrite(locfile + " exception " + e.ToString() + "\n");
        }
      }
      foreach (string d in Directory.GetDirectories(directory)) {
        if (Path.GetFileName(d).StartsWith(".")) { continue; }
        GatherLocalizations(d);
      };
    }
    public static void ProcessLocalizationDefinition(LocalizationDef def) {
      foreach (var locRec in def.content) {
        TranslateRecord trRec = new TranslateRecord();
        trRec.Name = locRec.id;
        trRec.Localization = new Dictionary<Strings.Culture, string>();
        trRec.Localization.Add(def.culture, locRec.content);
        trRec.Localization.Add(Strings.Culture.CULTURE_EN_US, locRec.original);
        trRec.Original = locRec.original;
        trRec.Commentary = locRec.localizatorComment;
        if (Core.localizationMethods.TryGetValue(locRec.processor, out jtProcGenericEx proc)) {
          if (proccessFiles.TryGetValue(locRec.filename, out HashSet<jtProcGenericEx> procs) == false) {
            procs = new HashSet<jtProcGenericEx>();
            proccessFiles.Add(locRec.filename, procs);
          }
          procs.Add(proc);
        }
        AddTranslationRecord(trRec);
      }
      foreach (var trgRec in def.directories) {
        string dir = trgRec.getDirectory(Core.ModsRootDirectory);
        if (Core.proccessDirectories.TryGetValue(dir, out HashSet<jtProcGenericEx> procs) == false) {
          procs = new HashSet<jtProcGenericEx>();
          proccessDirectories.Add(dir, procs);
        }
        foreach (string procName in trgRec.processors) {
          if (Core.localizationMethods.TryGetValue(procName, out jtProcGenericEx proc)) {
            procs.Add(proc);
          }
        }
      }
      foreach (string afile in def.files) {
        Core.affectedFiles.Add(afile);
      }
      if (loadedDefinitions.ContainsKey(def.filename)) {
        loadedDefinitions[def.filename] = def;
      } else {
        loadedDefinitions.Add(def.filename, def);
      }
    }
    public static void GatherLocalizationDefs(string directory) {
      string[] locDefs = Directory.GetFiles(directory,"Localization?*.json",SearchOption.AllDirectories);
      foreach(string locDef in locDefs) {
        Log.M?.TWL(0, locDef);
        try {
          if (Path.GetFileName(locDef).ToUpper() == Core.LocalizationFileName.ToUpper()) { continue; }
          LocalizationDef def = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(locDef));
          def.filename = locDef;
          ProcessLocalizationDefinition(def);
          //Strings.Culture defCulture = def.culture;
        } catch(Exception e) {
          Log.Er?.TWL(0, locDef, true);
          Log.Er?.TWL(0, e.ToString(),true);
        }
      }
      foreach(var procFile in Core.proccessFiles) {
        Log.M?.TWL(0, "'"+procFile.Key+"'");
        foreach(var proc in procFile.Value) {
          Log.M?.WL(1,proc.Name);
        }
      }
      foreach (var procDir in Core.proccessDirectories) {
        Log.M?.TWL(0, procDir.Key);
        foreach (var proc in procDir.Value) {
          Log.M?.WL(1, proc.Name);
        }
      }
      Log.M?.TWL(0, "affectedFiles:"+ Core.affectedFiles.Count);
      foreach (string afile in Core.affectedFiles) {
        Log.M?.WL(1, afile);
      }
    }
    public static string getLocalizationString(string key) {
      if (Core.stringsTable.TryGetValue(key, out var cDict)) {
        if (cDict.Count == 0) { return key; };
        if (cDict.TryGetValue(Localize.Strings.CurrentCulture, out string value)) {
          return value;
        } else {
          if (cDict.TryGetValue(Localize.Strings.Culture.CULTURE_EN_US, out value)) {
            return value;
          } else {
            return cDict.First().Value;
          }
        }
      }
      return key;
    }
    public static string getLocalizationString(string key, Strings.Culture culture) {
      if (Core.stringsTable.ContainsKey(key) == false) { return key; }
      Dictionary<Localize.Strings.Culture, string> cDict = Core.stringsTable[key];
      if (cDict.Count == 0) { return key; };
      if (cDict.ContainsKey(culture) == false) { return cDict.First().Value; }
      return cDict[culture];
    }
    //public static Dictionary<string, TranslateRecord> flushableTranslation = new Dictionary<string, TranslateRecord>();
    //public static TranslationSaver translationSaver = null;
    public static Regex locRegEx = new Regex("[_]{2}[/]{1}([a-zA-Z0-9\\._\\-\\,\\{\\}\\s\\+\\(\\)\\#]*)[/]{1}[_]{2}");
    public static void SaveCurrentTranslation() {
      //string path = Path.Combine(Log.BaseDirectory, Core.Settings.language+"_locTable.json");
      //File.WriteAllText(path, JsonConvert.SerializeObject(flushableTranslation,Formatting.Indented));
      //Log.M?.LogWrite("saved translation to "+path+"\n", true);
    }
    public static string ModsRootDirectory = string.Empty;
    public static string CurRootDirectory = string.Empty;
    public static Strings.Culture defaultCulture = Strings.Culture.CULTURE_EN_US;
    public static void InitStandalone(string directory) {
      CurRootDirectory = directory;
      ModsRootDirectory = Path.GetDirectoryName(directory);
      Log.BaseDirectory = directory;
      //settingsJson.get
      Core.Settings = new CTSettings();
      Core.Settings.debugLog = true;
      Log.InitLog();
      Log.M?.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      InitStructure();
      Core.GatherLocalizations(ModsRootDirectory);
      Core.GatherLocalizationDefs(Path.GetDirectoryName(directory));
    }

    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      
      //settingsJson.get
      Core.Settings = JsonConvert.DeserializeObject<CustomTranslation.CTSettings>(settingsJson);
      Core.Settings.cultureSettingsFilePath = Path.Combine(directory, "..", "..", "cultureSettings.json");
      if (File.Exists(Core.Settings.cultureSettingsFilePath)) {
        Core.currentCulture = JsonConvert.DeserializeObject<CLCultureSettings>(File.ReadAllText(Core.Settings.cultureSettingsFilePath));
      } else {
        Core.currentCulture = new CLCultureSettings();
        File.WriteAllText(Core.Settings.cultureSettingsFilePath, JsonConvert.SerializeObject(Core.currentCulture,Formatting.Indented));
      }
      Log.InitLog();
      Log.Er?.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      Log.Er?.WL(1, "localizationProcType:" + Core.Settings.localizationProcType);
      Log.Er?.WL(1, "CurrentCulture:" + Core.currentCulture.currentCulture+"/"+Strings.CurrentCulture);
      InitStructure();
      try {
        Log.Er?.TWL(0, "loading assets:" + Path.Combine(directory, "assets"));
        string[] fontsAssets = Directory.GetFiles(Path.Combine(directory, "assets"));
        foreach (string path in fontsAssets) {
          var assetBundle = AssetBundle.LoadFromFile(path);
          if (assetBundle != null) {
            Log.M?.WL(1, "asset " + path + ":" + assetBundle.name + " loaded");
            TMP_FontAsset[] assetFonts = assetBundle.LoadAllAssets<TMP_FontAsset>();
            foreach(TMP_FontAsset font in assetFonts) {
              Log.M?.WL(2, font.name+":"+(font.atlas == null?"null":font.atlas.name));
              if (font.atlas == null) { continue; }
              if (Core.fonts.ContainsKey(font.name) == false) {
                Core.fonts.Add(font.name, font);
              } else {
                Core.fonts[font.name] = font;
              }
            }
          } else {
            Log.M?.WL(1, "asset " + path + ":" + "fail to load");
          }
        }
        if (Core.Settings.localizationProcType != LocalizationProcType.None) {
          Core.GatherLocalizations(Path.GetDirectoryName(directory));
          Core.GatherLocalizationDefs(Path.GetDirectoryName(directory));
          var harmony = HarmonyInstance.Create("io.mission.customlocalization");
          harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        //LazySingletonBehavior<UnityGameInstance>.Instance.Game.MessageCenter.AddSubscriber(MessageCenterMessageType.OnLanguageChanged, new ReceiveMessageCenterMessage(Core.OnLanguageChanged));
        //translationSaver = new GameObject();
        //translationSaver.AddComponent<TranslationSaver>();
        //translationSaver.SetActive(true);
      } catch (Exception e) {
        Log.Er?.LogWrite(e.ToString() + "\n");
      }
    }
  }
}
