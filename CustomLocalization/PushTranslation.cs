using BattleTech;
using isogame;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CustomTranslation {
  public static class ConvFileHelper {
    public static bool isReverse { get; set; } = false;
    public static LocalizationFile reverseFile { get; set; } = null;
  }
  public class ConversationFile {
    public string FileName;
    public string errorString;
    public isogame.Conversation conversation;
    public bool Loaded { get { return conversation != null; } }
    public ConversationFile(string filename) {
      try {
        FileName = filename;
        RuntimeTypeModel runtimeTypeModel = TypeModel.Create();
        using (FileStream fileStream = new FileStream(filename, FileMode.Open)) {
          this.conversation = runtimeTypeModel.Deserialize((Stream)fileStream, (object)null, typeof(Conversation)) as Conversation;
        }
        errorString = string.Empty;
      } catch (Exception ex) {
        conversation = null;
        errorString = ex.ToString();
        Log.M?.LogWrite(FileName + "\n" + ex.ToString() + "\n");
      }
    }
    public void Save() {
      if (conversation == null) { return; };
      RuntimeTypeModel runtimeTypeModel = TypeModel.Create();
      try {
        using (FileStream fileStream = new FileStream(FileName, FileMode.Create))
          runtimeTypeModel.Serialize((Stream)fileStream, (object)conversation);
      } catch (Exception) {
        //MessageBox.Show(ex.ToString());
      }
    }
  }
  public class LocalizationFile {
    public bool changed;
    public string filename;
    public List<CustomTranslation.TranslateRecord> content;
    public Dictionary<string, CustomTranslation.TranslateRecord> map;
    public void DebugLogDump() {
      Log.M?.LogWrite("\n" + JsonConvert.SerializeObject(content, Formatting.Indented) + "\n", true);
    }
    public LocalizationFile() {
      changed = false;
      filename = string.Empty;
      content = new List<CustomTranslation.TranslateRecord>();
      map = new Dictionary<string, CustomTranslation.TranslateRecord>();
    }
    public void MergeFile(string filename) {
      changed = false;
      if (File.Exists(filename)) {
        string loccont = File.ReadAllText(filename);
        List<CustomTranslation.TranslateRecord> content = JsonConvert.DeserializeObject<List<CustomTranslation.TranslateRecord>>(loccont);
        foreach (CustomTranslation.TranslateRecord locRec in content) {
          this.Merge(locRec);
        }
      }
    }
    public LocalizationFile(string filename) {
      this.filename = filename;
      changed = false;
      if (File.Exists(filename)) {
        string loccont = File.ReadAllText(filename);
        map = new Dictionary<string, CustomTranslation.TranslateRecord>();
        content = JsonConvert.DeserializeObject<List<CustomTranslation.TranslateRecord>>(loccont);
        foreach (CustomTranslation.TranslateRecord locRec in content) {
          if (map.ContainsKey(locRec.Name) == false) { map.Add(locRec.Name, locRec); }
        }
      } else {
        content = new List<CustomTranslation.TranslateRecord>();
        map = new Dictionary<string, CustomTranslation.TranslateRecord>();
      }
    }
    public void Merge(CustomTranslation.TranslateRecord inc, bool replace = false) {
      if (map.ContainsKey(inc.Name) == false) {
        content.Add(inc);
        map.Add(inc.Name, inc);
        changed = true;
      } else {
        if (string.IsNullOrEmpty(map[inc.Name].Original) || (replace && (string.IsNullOrEmpty(inc.Original) == false))) {
          map[inc.Name].Original = inc.Original;
        }
        if (string.IsNullOrEmpty(map[inc.Name].Commentary)) {
          map[inc.Name].Commentary = inc.Commentary;
        } else if (replace) {
          map[inc.Name].Commentary = inc.Commentary;
        }
        foreach (var tr in inc.Localization) {
          if (map[inc.Name].Localization.ContainsKey(tr.Key) == false) { map[inc.Name].Localization.Add(tr.Key, tr.Value); changed = true; } else
          if (replace == true) { map[inc.Name].Localization[tr.Key] = tr.Value; changed = true; }
        }
      }
    }
    public void removeOtherTranslations(List<Strings.Culture> neededLocs) {
      HashSet<Strings.Culture> stay_cultures = new HashSet<Strings.Culture>();
      foreach (Strings.Culture culture in neededLocs) {
        stay_cultures.Add(culture);
      }
      HashSet<Strings.Culture> del_cultures = new HashSet<Strings.Culture>();
      foreach (Strings.Culture culture in Enum.GetValues(typeof(Strings.Culture))) {
        if (stay_cultures.Contains(culture) == false) { del_cultures.Add(culture); };
      }
      foreach (CustomTranslation.TranslateRecord rec in this.content) {
        foreach (Strings.Culture culture in del_cultures) {
          if (rec.Localization.ContainsKey(culture)) { rec.Localization.Remove(culture); };
        }
      }
    }
    public void Save(bool forced = false) {
      if (string.IsNullOrEmpty(filename)) { return; }
      if ((changed == false) && (forced == false)) { return; }
      File.WriteAllText(filename, JsonConvert.SerializeObject(content, Formatting.Indented));
    }
  }
  public class TargetDef {
    public List<string> dir { get; set; }
    public List<string> processors { get; set; }
    public TargetDef() {
      dir = new List<string>();
      processors = new List<string>();
    }
    public string getDirectory(string directory) {
      string result = directory;
      for (int t = 0; t < dir.Count; ++t) { result = Path.Combine(result, dir[t]); };
      return result;
    }
  }
  public class LocalizationRecordDef {
    public string id { get; set; }
    public string original { get; set; }
    public string prevOriginal { get; set; }
    public string content { get; set; }
    public string localizatorComment { get; set; }
    public string systemComment { get; set; }
    public string backColor { get; set; }
    public string textColor { get; set; }
    public string filename { get; set; }
    public string processor { get; set; }
    [JsonIgnore]
    public LocalizationDef parent { get; set; } = null;
    public string GetShortValue() {
      if (content.Length < 20) { return this.content; }
      return this.content.Substring(0, 17) + "...";
    }
    public LocalizationRecordDef() {
      id = string.Empty;
      original = string.Empty;
      prevOriginal = string.Empty;
      content = string.Empty;
      localizatorComment = string.Empty;
      systemComment = string.Empty;
      backColor = string.Empty;
      textColor = string.Empty;
    }
  }
  public class LocalizationDef {
    [JsonIgnore]
    public string filename { get; set; }
    public Strings.Culture culture;
    //public List<string> files { get; set; }
    //public List<TargetDef> directories { get; set; }
    public List<LocalizationRecordDef> content { get; set; }
    [JsonIgnore]
    public Dictionary<string, LocalizationRecordDef> index { get; set; } = new Dictionary<string, LocalizationRecordDef>();
    [JsonIgnore]
    public int diffcounter = 0;
    public LocalizationDef(string filename, Strings.Culture culture) {
      this.filename = filename;
      this.culture = culture;
      //this.directories = new List<TargetDef>();
      this.content = new List<LocalizationRecordDef>();
    }
    public LocalizationDef() {
      this.culture = Core.defaultCulture;
      //this.directories = new List<TargetDef>();
      this.content = new List<LocalizationRecordDef>();
      //this.files = new List<string>();
    }
    public LocalizationDef(string filename) {
      this.filename = filename;
      //this.directories = new List<TargetDef>();
      this.content = new List<LocalizationRecordDef>();
      //this.files = new List<string>();
    }
    public LocalizationDef(Strings.Culture culture) {
      this.culture = culture;
      //this.directories = new List<TargetDef>();
      this.content = new List<LocalizationRecordDef>();
      //this.files = new List<string>();
    }
    public void reindex() {
      index.Clear();
      foreach (var rec in content) {
        rec.parent = this;
        index[rec.id] = rec;
      }
    }
    public string Merge(LocalizationDef second, bool replaceContent) {
      if (this.culture != second.culture) { return "File cultures not equal"; }
      Dictionary<string, LocalizationRecordDef> dict = new Dictionary<string, LocalizationRecordDef>();
      Dictionary<string, TargetDef> dirs = new Dictionary<string, TargetDef>();
      foreach (LocalizationRecordDef locRec in this.content) {
        if (dict.ContainsKey(locRec.id)) { continue; }
        dict.Add(locRec.id, locRec);
      }
      Log.M?.TWL(0, "merging:" + second.filename);
      foreach (LocalizationRecordDef nlocRec in second.content) {
        Log.M?.WL(1, nlocRec.id);
        if (dict.TryGetValue(nlocRec.id, out LocalizationRecordDef locRec) == false) {
          Log.M?.WL(2, "new");
          this.content.Add(nlocRec);
          dict.Add(nlocRec.id, nlocRec);
        } else {
          Log.M?.WL(2, "exists:" + locRec.original + ":" + nlocRec.original);
          if (replaceContent) { locRec.content = nlocRec.content; }
          if (locRec.original != nlocRec.original) {
            locRec.prevOriginal = locRec.original;
            locRec.original = nlocRec.original;
            locRec.backColor = "#FF0000";
          }
        }
      }
      return string.Empty;
    }
  }
  public enum ModDirRecordState {
    None, Partial, Complete, Empty
  }
  public class ModDirRecord {
    public string root { get; set; }
    public string name { get; set; }
    public string path { get; set; }
    public HashSet<string> files { get; set; }
    public ModDirRecordState getState() {
      if (files.Count == 0) { return ModDirRecordState.Empty; }
      bool atleastone = false;
      bool all = true;
      foreach (string file in files) {
        if (Core.affectedFiles.Contains(file)) {
          atleastone = true;
        } else {
          all = false;
        }
      }
      if (all) { return ModDirRecordState.Complete; }
      if (atleastone) { return ModDirRecordState.Partial; }
      return ModDirRecordState.None;
    }
    public ModDirRecord(string n, string r, string p) {
      name = n; path = p; root = r;
      fillFiles();
    }
    private static Regex rgx = new Regex("[^a-zA-Z0-9 -]");
    public static string Normilize(string val) {
      Regex rgx = new Regex("[^a-zA-Z0-9\\-_]");
      return rgx.Replace(val, "_");
    }
    public static List<ModDirRecord> GatherMods(string modsPath) {
      List<ModDirRecord> result = new List<ModDirRecord>();
      foreach (string modPath in Directory.GetDirectories(modsPath)) {
        string manifestPath = System.IO.Path.Combine(modPath, "mod.json");
        try {
          JObject mainfest = JObject.Parse(File.ReadAllText(manifestPath));
          result.Add(new ModDirRecord((string)mainfest["Name"], modPath, modPath));
        } catch (Exception) {
          continue;
        }
      }
      return result;
    }
    public static HashSet<string> fillFiles(string path, string modName) {
      HashSet<string> files = new HashSet<string>();
      string[] foundfiles = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
      foreach (string filename in foundfiles) {
        string name = Path.GetFileName(filename).ToUpper();
        if (name == Core.LocalizationFileName.ToUpper()) { continue; }
        name = Path.GetFileNameWithoutExtension(filename).ToUpper();
        if (name.Contains("SETTINGS") && (modName.Contains("Affinity"))) { } else {
          if (Core.skipLocalizationProc.Contains(name)) { continue; }
        }
        if (name.StartsWith(Core.LocalizationDefPrefix.ToUpper())
          && name.EndsWith(Core.LocalizationDefSuffix.ToUpper())) { continue; }
        files.Add(Path.GetFileName(filename));
      }
      foundfiles = Directory.GetFiles(path, "*.bytes", SearchOption.TopDirectoryOnly);
      foreach (string filename in foundfiles) {
        files.Add(Path.GetFileName(filename));
      }
      return files;
    }
    private void fillFiles() {
      this.files = fillFiles(this.path, this.name);
    }
  }
  public abstract class jtProcGeneric {
    public virtual string Name { get { return "Generic"; } }
    public jtProcGeneric() { }
    public abstract bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check, Func<string, bool> isInVanilla);
  }
  public abstract class jtProcGenericEx {
    public virtual string Name { get { return "Generic"; } }
    public static Func<string, bool> f_isInVanilla = null;
    public static bool isInVanilla(string value) {
      if (f_isInVanilla == null) { return false; }
      return f_isInVanilla(value);
    }
    public jtProcGenericEx() { }
    public virtual string getValue(string key, string value) {
      if (Core.stringsTable.TryGetValue(key, out var locTable)) {
        switch (Core.Settings.localizationProcType) {
          case LocalizationProcType.Key: {
              if (locTable.ContainsKey(Strings.Culture.CULTURE_EN_US) == false) {
                locTable.Add(Strings.Culture.CULTURE_EN_US, value);
              } else {
                locTable[Strings.Culture.CULTURE_EN_US] = value;
              }
              return CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            };
          case LocalizationProcType.Dummy:
            return key;
          case LocalizationProcType.Content: {
              if (locTable.TryGetValue(Core.currentCulture.currentCulture, out string locVal)) {
                return string.IsNullOrEmpty(locVal)?value:locVal;
              } else {
                return value;
              }
            };
        }
      }
      return value;
    }
    public abstract bool check(string modName, string filename, ref object inc, ref HashSet<string> keys);
    public abstract bool reverse(ref object inc);
    public abstract bool proc(string modName, string filename, ref object inc);
    public abstract bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace);
  }
  public abstract class jtL2K2 : jtProcGenericEx {
    protected virtual bool AbstractActorCheck() { return false; }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    protected virtual string K1 { get { return "Description"; } }
    protected virtual string K2 { get { return "Id"; } }
    public override string Name { get { return L1 + "." + L2; } }
    public bool check_pre(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[L1] == null) { return false; };
      if (json[L1][L2] == null) { return false; };
      if (AbstractActorCheck()) {
        if (json[K1][K2] == null) { return false; };
        string id = (string)json[K1][K2];
        if (id.Contains("mechdef")) { return false; }
        if (id.Contains("vehicledef")) { return false; }
        if (id.Contains("chassisdef")) { return false; }
        if (id.Contains("vehiclechassisdef")) { return false; }
        if (id.Contains("turretchassisdef")) { return false; }
        if (id.Contains("turretdef")) { return false; }
      }
      return true;
    }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[L1][L2];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + L1 + "." + L2;
      keys.Add(key);
      return true;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[L1] == null) { return false; };
      Log.M?.WL(2, L1 + ":" + json[L1].Type);
      if (json[L1].Type != JTokenType.Object) { return false; };
      if (json[L1][L2] == null) { return false; };
      string value = (string)json[L1][L2];
      if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { return false; }
      Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
      json[L1][L2] = value;
      return true;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[L1][L2];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + L1 + "." + L2;
      key = key.ToUpper();
      if (Core.stringsTable.ContainsKey(key) == false) { return false; }
      json[L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
      inc = json;
      return true;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[L1][L2];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + L1 + "." + L2;
      //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { return false; }
      replace.Add(key, value);
      return true;
    }
  }
  public abstract class jtField : jtProcGenericEx {
    protected virtual string FieldName() { return "Generic"; }
    public override string Name { get { return FieldName(); } }
    public bool check_pre(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[FieldName()] == null) { return false; };
      return true;
    }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[FieldName()];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + FieldName();
      keys.Add(key);
      return true;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[FieldName()] == null) { return false; };
      string value = (string)json[FieldName()];
      if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { return false; }
      Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
      json[FieldName()] = value;
      return true;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[FieldName()];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + FieldName();
      key = key.ToUpper();
      if (Core.stringsTable.ContainsKey(key) == false) { return false; }
      json[FieldName()] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
      inc = json;
      return true;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      if (check_pre(ref inc) == false) { return false; }
      JObject json = inc as JObject;
      string value = (string)json[FieldName()];
      if (string.IsNullOrEmpty(value)) { return false; }
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { return false; };
      string key = filename + "." + FieldName();
      //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { return false; }
      replace.Add(key, value);
      return true;
    }
  }
  public class jtDescriptionUIname : jtL2K2 {
    protected override bool AbstractActorCheck() { return true; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "UIName"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtDescriptionName : jtL2K2 {
    protected override bool AbstractActorCheck() { return true; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtDescriptionDetails : jtL2K2 {
    protected override bool AbstractActorCheck() { return false; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtFirstName : jtL2K2 {
    protected override bool AbstractActorCheck() { return false; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "FirstName"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtLastName : jtL2K2 {
    protected override bool AbstractActorCheck() { return false; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "LastName"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtCallsign : jtL2K2 {
    protected override bool AbstractActorCheck() { return false; }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Callsign"; } }
    protected override string K1 { get { return "Description"; } }
    protected override string K2 { get { return "Id"; } }
  }
  public class jtStockRole : jtField {
    protected override string FieldName() { return "StockRole"; }
  }
  public class jtYangsThoughts : jtField {
    protected override string FieldName() { return "YangsThoughts"; }
  }
  public class jtName : jtField {
    protected override string FieldName() { return "Name"; }
  }
  public class jtShortName : jtField {
    protected override string FieldName() { return "ShortName"; }
  }
  public class jtDescription : jtField {
    protected override string FieldName() { return "Description"; }
  }
  public class jtFlashpointShortDescription : jtField {
    protected override string FieldName() { return "FlashpointShortDescription"; }
  }
  public class jtquirkName : jtField {
    protected override string FieldName() { return "quirkName"; }
  }
  public class jtdescription : jtField {
    protected override string FieldName() { return "description"; }
  }
  public class jtOptionName : jtField {
    protected override string FieldName() { return "OptionName"; }
  }
  public class jtOptionDescription : jtField {
    protected override string FieldName() { return "OptionDescription"; }
  }
  public class jtBioDescription : jtField {
    protected override string FieldName() { return "BioDescription"; }
  }
  public abstract class jtI1L2K1 : jtProcGenericEx {
    protected virtual string I1 { get { return "EffectData"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    protected virtual string K1 { get { return "Id"; } }
    public override string Name { get { return I1 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + "." + I1 + t + "." + L2;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        result = true;
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][t][L1][L2] = value;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + "." + I1 + t + "." + L2;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == false) { continue; }
        json[I1][t][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        result = true;
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + "." + I1 + t + "." + L2;
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
      }
      return true;
    }
  }
  public class jtEffectDataName : jtI1L2K1 {
    protected override string I1 { get { return "EffectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtEffectDataDetails : jtI1L2K1 {
    protected override string I1 { get { return "EffectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jteffectDataName : jtI1L2K1 {
    protected override string I1 { get { return "effectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jteffectDataDetails : jtI1L2K1 {
    protected override string I1 { get { return "effectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtStatusEffectsName : jtI1L2K1 {
    protected override string I1 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtStatusEffectsDetails : jtI1L2K1 {
    protected override string I1 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public abstract class jtI1II1L2K1 : jtProcGenericEx {
    protected virtual string I1 { get { return "Modes"; } }
    protected virtual string II1 { get { return "statusEffects"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    protected virtual string K1 { get { return "Id"; } }
    public override string Name { get { return I1 + "." + II1 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + json[I1][t][II1][tt][L1][K1] + "." + I1 + t + "." + tt + "." + L2;
          keys.Add(key);
          result = true;
        }
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
          result = false;
          Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
          json[I1][t][II1][tt][L1][L2] = value;
        }
      }
      inc = json;
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + json[I1][t][II1][tt][L1][K1] + "." + I1 + t + "." + tt + "." + L2;
          key = key.ToUpper();
          if (Core.stringsTable.ContainsKey(key) == true) {
            json[I1][t][II1][tt][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            result = true;
          }
        }
      }
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + json[I1][t][II1][tt][L1]["Id"] + "." + I1 + t + "." + tt + "." + L2;
          //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
          replace.Add(key, value);
          result = true;
        }
      }
      return result;
    }
  }
  public abstract class jtI1K1II1L1L2K2 : jtProcGenericEx {
    protected virtual string I1 { get { return "pilotQuirks"; } }
    protected virtual string K1 { get { return "tag"; } }
    protected virtual string II1 { get { return "effectData"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    protected virtual string K2 { get { return "Id"; } }
    protected virtual string keyPrefix { get { return "MechAffinity"; } }
    public override string Name { get { return keyPrefix + "."+I1 + "." + II1 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = string.IsNullOrEmpty(keyPrefix)?filename:keyPrefix + "." + json[I1][t][K1] + "." + II1 + "." + json[I1][t][II1][tt][L1][K2] + tt + "." + L2;
          keys.Add(key);
          result = true;
        }
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
          result = false;
          Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
          json[I1][t][II1][tt][L1][L2] = value;
        }
      }
      inc = json;
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = string.IsNullOrEmpty(keyPrefix) ? filename : keyPrefix + "." + json[I1][t][K1] + "." + II1 + "." + json[I1][t][II1][tt][L1][K2] + tt + "." + L2;
          key = key.ToUpper();
          if (Core.stringsTable.ContainsKey(key) == true) {
            json[I1][t][II1][tt][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            result = true;
          }
        }
      }
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1][L2];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = string.IsNullOrEmpty(keyPrefix) ? filename : keyPrefix + "." + json[I1][t][K1] + "." + II1 + "." + json[I1][t][II1][tt][L1][K2] + tt + "." + L2;
          //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
          replace.Add(key, value);
          result = true;
        }
      }
      return result;
    }
  }
  public class jtModeStatusEffectsName : jtI1II1L2K1 {
    protected override string I1 { get { return "Modes"; } }
    protected override string II1 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtpilotQuirksStatusEffectsName : jtI1K1II1L1L2K2 {
    protected override string I1 { get { return "pilotQuirks"; } }
    protected override string K1 { get { return "tag"; } }
    protected override string II1 { get { return "effectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K2 { get { return "Id"; } }
    protected override string keyPrefix { get { return "MechAffinity"; } }
  }
  public class jtpilotQuirksStatusEffectsDetails : jtI1K1II1L1L2K2 {
    protected override string I1 { get { return "pilotQuirks"; } }
    protected override string K1 { get { return "tag"; } }
    protected override string II1 { get { return "effectData"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K2 { get { return "Id"; } }
    protected override string keyPrefix { get { return "MechAffinity"; } }
  }
  public class jtModeStatusEffectsDetails : jtI1II1L2K1 {
    protected override string I1 { get { return "Modes"; } }
    protected override string II1 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public abstract class jtI3L2 : jtProcGenericEx {
    protected virtual string I1 { get { return "Custom"; } }
    protected virtual string I2 { get { return "ActivatableComponent"; } }
    protected virtual string I3 { get { return "statusEffects"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    public override string Name { get { return I1 + "." + I2 + "." + I3 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3].Count() == 0) { return false; };
      int count = json[I1][I2][I3].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        //string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Id"] + ".CAE" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][I3][t][L1]["Id"] + ".CAE" + t + "." + L2;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3].Count() == 0) { return false; };
      int count = json[I1][I2][I3].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][I2][I3][t][L1][L2] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3].Count() == 0) { return false; };
      int count = json[I1][I2][I3].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][I3][t][L1]["Id"] + ".CAE" + t + "." + L2;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == true) {
          json[I1][I2][I3][t][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          result = true;
        }
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3].Count() == 0) { return false; };
      int count = json[I1][I2][I3].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        string key = filename + "." + json[I1][I2][I3][t][L1]["Id"] + ".CAE" + t + "." + L2;
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
        result = true;
      }
      return result;
    }
  }
  public abstract class jtI2L2 : jtProcGenericEx {
    protected virtual string I1 { get { return "deferredEffect"; } }
    protected virtual string I2 { get { return "statusEffects"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    public override string Name { get { return I1 + "." + I2 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2].Count() == 0) { return false; };
      int count = json[I1][I2].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        //string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Id"] + ".CAE" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][t][L1]["Id"] + t + "." + L2;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2].Count() == 0) { return false; };
      int count = json[I1][I2].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][I2][t][L1][L2] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2].Count() == 0) { return false; };
      int count = json[I1][I2].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][t][L1]["Id"] + t + "." + L2;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == true) {
          json[I1][I2][t][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          result = true;
        }
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2].Count() == 0) { return false; };
      int count = json[I1][I2].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        string key = filename + "." + json[I1][I2][t][L1]["Id"] + t + "." + L2;
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
        result = true;
      }
      return result;
    }
  }
  public abstract class jtI1L1 : jtProcGenericEx {
    protected virtual string I1 { get { return "objectiveList"; } }
    protected virtual string L1 { get { return "title"; } }
    public override string Name { get { return I1 + "." + L1; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + I1 + t + "." + L1;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][t][L1] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + I1 + t + "." + L1;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == false) { continue; };
        json[I1][t][L1] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        result = true;
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        string key = filename + "." + I1 + t + "." + L1;
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
        result = true;
      }
      return result;
    }
  }
  public abstract class jtI1L1K1 : jtProcGenericEx {
    protected virtual string I1 { get { return "Settings"; } }
    protected virtual string L1 { get { return "Short"; } }
    protected virtual string K1 { get { return "Bonus"; } }
    protected virtual string keyPrefix { get { return string.Empty; } }
    public override string Name { get { return (string.IsNullOrEmpty(keyPrefix) ? "" : (keyPrefix + ".")) + I1 + "." + L1; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = (string.IsNullOrEmpty(keyPrefix) ? (filename) : keyPrefix) + "." + json[I1][t][K1] + "." + L1;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][t][L1] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = (string.IsNullOrEmpty(keyPrefix) ? (filename) : keyPrefix) + "." + json[I1][t][K1] + "." + L1;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == false) { continue; };
        json[I1][t][L1] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        result = true;
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      //Log.M.TWL(0, "extract:"+modName+":"+filename+":"+I1+"."+L1+" key:"+K1);
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        string key = (string.IsNullOrEmpty(keyPrefix) ? (filename) : keyPrefix) + "." + json[I1][t][K1] + "." + L1;
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
        result = true;
      }
      return result;
    }
  }
  public abstract class jtI4L2 : jtProcGenericEx {
    protected virtual string I1 { get { return "Custom"; } }
    protected virtual string I2 { get { return "ActivatableComponent"; } }
    protected virtual string I3 { get { return "Explosion"; } }
    protected virtual string I4 { get { return "statusEffects"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    public override string Name { get { return I1 + "." + I2 + "." + I3 + "." + I4 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3][I4] == null) { return false; };
      if (json[I1][I2][I3][I4].Count() == 0) { return false; };
      int count = json[I1][I2][I3][I4].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][I4][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][I4][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        //string key = filename + "." + json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Id"] + ".CAE" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][I3][I4][t][L1]["Id"] + "." + I3 + t + "." + L2;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3][I4] == null) { return false; };
      if (json[I1][I2][I3][I4].Count() == 0) { return false; };
      int count = json[I1][I2][I3][I4].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][I4][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][I4][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][I2][I3][I4][t][L1][L2] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3][I4] == null) { return false; };
      if (json[I1][I2][I3][I4].Count() == 0) { return false; };
      int count = json[I1][I2][I3][I4].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][I4][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][I4][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][I2][I3][I4][t][L1]["Id"] + "." + I3 + t + "." + L2;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == false) { continue; }
        json[I1][I2][I3][I4][t][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        result = true;
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1][I2] == null) { return false; };
      if (json[I1][I2][I3] == null) { return false; };
      if (json[I1][I2][I3][I4] == null) { return false; };
      if (json[I1][I2][I3][I4].Count() == 0) { return false; };
      int count = json[I1][I2][I3][I4].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][I2][I3][I4][t][L1] == null) { continue; }
        string value = (string)json[I1][I2][I3][I4][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if (isInVanilla(value)) { continue; }
        string key = filename + "." + json[I1][I2][I3][t][I4][L1]["Id"] + "." + I3 + t + "." + L2;
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
        result = true;
      }
      return result;
    }
  }
  public class jtCACDefferedEffectsName : jtI2L2 {
    protected override string I1 { get { return "deferredEffect"; } }
    protected override string I2 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
  }
  public class jtCACDefferedEffectsDetails : jtI2L2 {
    protected override string I1 { get { return "deferredEffect"; } }
    protected override string I2 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public class jtCAEStatusEffectsName : jtI3L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
  }
  public class jtCAEStatusEffectsDetails : jtI3L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public class jtCAEofflineStatusEffectsName : jtI3L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "offlineStatusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
  }
  public class jtCAEofflineStatusEffectsDetails : jtI3L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "offlineStatusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public class jtCAEExplosionStatusEffectsName : jtI4L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "Explosion"; } }
    protected override string I4 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
  }
  public class jtCAEExplosionStatusEffectsDetails : jtI4L2 {
    protected override string I1 { get { return "Custom"; } }
    protected override string I2 { get { return "ActivatableComponent"; } }
    protected override string I3 { get { return "Explosion"; } }
    protected override string I4 { get { return "statusEffects"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public class jtcontractName : jtField {
    protected override string FieldName() { return "contractName"; }
  }
  public class jtshortDescription : jtField {
    protected override string FieldName() { return "shortDescription"; }
  }
  public class jtShortDesc : jtField {
    protected override string FieldName() { return "ShortDesc"; }
  }
  public class jtfirstName : jtField {
    protected override string FieldName() { return "firstName"; }
  }
  public class jtlastName : jtField {
    protected override string FieldName() { return "lastName"; }
  }
  public class jtcallsign : jtField {
    protected override string FieldName() { return "callsign"; }
  }
  public class jtrank : jtField {
    protected override string FieldName() { return "rank"; }
  }
  public class jtlongDescription : jtField {
    protected override string FieldName() { return "longDescription"; }
  }
  public class jtobjectiveList_title : jtI1L1 {
    protected override string I1 { get { return "objectiveList"; } }
    protected override string L1 { get { return "title"; } }
  }
  public class jtpilotQuirks_description : jtI1L1K1 {
    protected override string I1 { get { return "pilotQuirks"; } }
    protected override string L1 { get { return "description"; } }
    protected override string K1 { get { return "tag"; } }
    protected override string keyPrefix { get { return "MechAffinity"; } }
  }
  public class jtpilotQuirks_quirkName : jtI1L1K1 {
    protected override string I1 { get { return "pilotQuirks"; } }
    protected override string L1 { get { return "quirkName"; } }
    protected override string K1 { get { return "tag"; } }
    protected override string keyPrefix { get { return "MechAffinity"; } }
  }
  public class jtobjectiveList_description : jtI1L1 {
    protected override string I1 { get { return "objectiveList"; } }
    protected override string L1 { get { return "description"; } }
  }
  public class jtaffinityLevels_description : jtI1L1 {
    protected override string I1 { get { return "affinityLevels"; } }
    protected override string L1 { get { return "description"; } }
  }
  public class jtaffinityLevels_levelName : jtI1L1 {
    protected override string I1 { get { return "affinityLevels"; } }
    protected override string L1 { get { return "levelName"; } }
  }
  public class jtcontents_words : jtI1L1 {
    protected override string I1 { get { return "contents"; } }
    protected override string L1 { get { return "words"; } }
  }
  public abstract class jtI1II1L1 : jtProcGenericEx {
    protected virtual string I1 { get { return "dialogueList"; } }
    protected virtual string II1 { get { return "dialogueContent"; } }
    protected virtual string L1 { get { return "words"; } }
    public override string Name { get { return I1 + "." + II1 + "." + L1; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + I1 + t + "." + II1 + tt + "." + L1;
          keys.Add(key);
          result = true;
        }
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1];
          if (string.IsNullOrEmpty(value)) { continue; };
          if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
          Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
          json[I1][t][II1][tt][L1] = value;
          result = true;
        }
      }
      inc = json;
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + I1 + t + "." + II1 + tt + "." + L1;
          key = key.ToUpper();
          if (Core.stringsTable.ContainsKey(key) == true) {
            json[I1][t][II1][tt][L1] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            result = true;
          }
        }
      }
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][II1] == null) { continue; }
        int count2 = json[I1][t][II1].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json[I1][t][II1][tt][L1] == null) { continue; }
          string value = (string)json[I1][t][II1][tt][L1];
          if (string.IsNullOrEmpty(value)) { continue; };
          if (isInVanilla(value)) { continue; }
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { continue; };
          string key = filename + "." + I1 + t + "." + II1 + tt + "." + L1;
          //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
          replace.Add(key, value);
          result = true;
        }
      }
      return result;
    }
  }
  public class jtdialogueListdialogueContentWords : jtI1II1L1 {
    protected override string I1 { get { return "dialogueList"; } }
    protected override string II1 { get { return "dialogueContent"; } }
    protected override string L1 { get { return "words"; } }
  }
  public class jtOptionsDescriptionName : jtI1L2K1 {
    protected override string I1 { get { return "Options"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
  }
  public class jtOptionsDescriptionDetails : jtI1L2K1 {
    protected override string I1 { get { return "Options"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
  }
  public class jtOptionsResultSetsDescriptionName : jtI1II1L2K1 {
    protected override string I1 { get { return "Options"; } }
    protected override string II1 { get { return "ResultSets"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtOptionsResultSetsDescriptionDetails : jtI1II1L2K1 {
    protected override string I1 { get { return "Options"; } }
    protected override string II1 { get { return "ResultSets"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Id"; } }
  }
  /*public class jtConversations : jtProcGeneric {
    public override string Name { get { return "Conversations(binary)"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check, Func<string, bool> isInVanilla) {
      ConversationFile cFile = inc as ConversationFile;
      if (cFile == null) { return false; }
      //Log.M?.LogWrite("Conversation:"+cFile.FileName+"\n");
      if (cFile.conversation == null) {
        //Log.M?.LogWrite(" null\n");
        return false;
      };
      //Log.M?.LogWrite(" ui_name:" + cFile.conversation.ui_name + "\n");

      //Log.M?.LogWrite(" roots:"+ cFile.conversation.roots.Count + "\n");
      for(int t = 0; t < cFile.conversation.roots.Count; ++t) {
        string value = cFile.conversation.roots[t].responseText;
        if (isInVanilla(value)) { continue; }
        string key = modName + "." + filename + ".root" + t + ".responseText";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          cFile.conversation.roots[t].responseText = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      //Log.M?.LogWrite(" nodes:\n");
      for (int t = 0; t < cFile.conversation.nodes.Count; ++t) {
        string value = cFile.conversation.nodes[t].text;
        if (isInVanilla(value)) { continue; }
        string key = modName + "." + filename + ".node" + t + ".text";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          cFile.conversation.nodes[t].text = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
        //Log.M?.LogWrite(" [" + t + "] comment:" + cFile.conversation.nodes[t].comment + "\n");
        //Log.M?.LogWrite(" [" + t + "] text:" + cFile.conversation.nodes[t].text + "\n");
        //Log.M?.LogWrite(" [" + t + "] branches:" + cFile.conversation.nodes[t].branches.Count + "\n");
        for (int tt = 0; tt < cFile.conversation.nodes[t].branches.Count; ++tt) {
          value = cFile.conversation.nodes[t].branches[tt].responseText;
          if (isInVanilla(value)) { continue; }
          key = modName + "." + filename + ".node" + t + ".branch"+tt+ ".responseText";
          if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            cFile.conversation.nodes[t].branches[tt].responseText = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      return false;
    }
  }*/
  public class jtSettingsShort : jtI1L1K1 {
    protected override string I1 { get { return "Settings"; } }
    protected override string L1 { get { return "Short"; } }
    protected override string K1 { get { return "Bonus"; } }
    protected override string keyPrefix { get { return "MechEngineer"; } }
  }
  public class jtSettingsLong : jtI1L1K1 {
    protected override string I1 { get { return "Settings"; } }
    protected override string L1 { get { return "Long"; } }
    protected override string K1 { get { return "Bonus"; } }
    protected override string keyPrefix { get { return "MechEngineer"; } }
  }
  public class jtSettingsFull : jtI1L1K1 {
    protected override string I1 { get { return "Settings"; } }
    protected override string L1 { get { return "Full"; } }
    protected override string K1 { get { return "Bonus"; } }
    protected override string keyPrefix { get { return "MechEngineer"; } }
  }
  public abstract class jtMechEngineerEffect : jtProcGenericEx {
    protected virtual string I1 { get { return "Settings"; } }
    protected virtual string L1 { get { return "Description"; } }
    protected virtual string L2 { get { return "Name"; } }
    protected virtual string K1 { get { return "Id"; } }
    public override string Name { get { return I1 + "." + L1 + "." + L2; } }
    public override bool check(string modName, string filename, ref object inc, ref HashSet<string> keys) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + ".effect." + L2;
        keys.Add(key);
        result = true;
      }
      return result;
    }
    public override bool reverse(ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        if ((value.Contains(Core.LocalizationRefPrefix) == false) && (value.Contains(Core.LocalizationRefSufix) == false)) { continue; }
        Text_Append.Localize(ref value, Strings.Culture.CULTURE_EN_US);
        json[I1][t][L1][L2] = value;
        result = true;
      }
      return result;
    }
    public override bool proc(string modName, string filename, ref object inc) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      bool result = false;
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + ".effect." + L2;
        key = key.ToUpper();
        if (Core.stringsTable.ContainsKey(key) == false) { continue; }
        json[I1][t][L1][L2] = this.getValue(key, value);//CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        result = true;
      }
      inc = json;
      return result;
    }
    public override bool extract(string modName, string filename, ref object inc, Dictionary<string, string> replace) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json[I1] == null) { return false; };
      if (json[I1].Type != JTokenType.Array) { return false; }
      if (json[I1].Count() == 0) { return false; };
      int count = json[I1].Count();
      for (int t = 0; t < count; ++t) {
        if (json[I1][t][L1] == null) { continue; }
        string value = (string)json[I1][t][L1][L2];
        if (string.IsNullOrEmpty(value)) { continue; };
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { continue; };
        string key = filename + "." + json[I1][t][L1][K1] + ".effect." + L2;
        //if (Core.stringsTable.ContainsKey(key.ToUpper()) == false) { continue; }
        replace.Add(key, value);
      }
      return true;
    }
  }
  public class jtSettingsEffectDataName : jtMechEngineerEffect {
    protected override string I1 { get { return "Settings"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Name"; } }
    protected override string K1 { get { return "Id"; } }
  }
  public class jtSettingsEffectDataDetails : jtMechEngineerEffect {
    protected override string I1 { get { return "Settings"; } }
    protected override string L1 { get { return "Description"; } }
    protected override string L2 { get { return "Details"; } }
    protected override string K1 { get { return "Id"; } }
  }
}