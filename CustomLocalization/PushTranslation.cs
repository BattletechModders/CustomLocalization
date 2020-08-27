using isogame;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomTranslation {
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
        Log.Debug?.Write(FileName+"\n"+ex.ToString()+"\n");
      }
    }
    public void Save() {
      if (conversation == null) { return; };
      RuntimeTypeModel runtimeTypeModel = TypeModel.Create();
      try {
        using (FileStream fileStream = new FileStream(FileName, FileMode.Create))
          runtimeTypeModel.Serialize((Stream)fileStream, (object)conversation);
      } catch (Exception ex) {
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
      Log.Debug?.Write("\n" + JsonConvert.SerializeObject(content, Formatting.Indented) + "\n",true);
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
    public void Merge(CustomTranslation.TranslateRecord inc) {
      if (map.ContainsKey(inc.Name) == false) {
        content.Add(inc);
        map.Add(inc.Name, inc);
        changed = true;
      } else {
        if (string.IsNullOrEmpty(map[inc.Name].Original)) {
          map[inc.Name].Original = inc.Original;
        }
        if (string.IsNullOrEmpty(map[inc.Name].Commentary)) {
          map[inc.Name].Commentary = inc.Commentary;
        }
        foreach (var tr in inc.Localization) {
          if (map[inc.Name].Localization.ContainsKey(tr.Key) == false) { map[inc.Name].Localization.Add(tr.Key, tr.Value); changed = true; }
        }
      }
    }
    public void removeOtherTranslations(List<Strings.Culture> neededLocs) {
      HashSet<Strings.Culture> stay_cultures = new HashSet<Strings.Culture>();
      foreach(Strings.Culture culture in neededLocs) {
        stay_cultures.Add(culture);
      }
      HashSet<Strings.Culture> del_cultures = new HashSet<Strings.Culture>();
      foreach (Strings.Culture culture in Enum.GetValues(typeof(Strings.Culture))) {
        if (stay_cultures.Contains(culture) == false) { del_cultures.Add(culture); };
      }
      foreach (CustomTranslation.TranslateRecord rec in this.content) {
        foreach(Strings.Culture culture in del_cultures) {
          if (rec.Localization.ContainsKey(culture)) { rec.Localization.Remove(culture); };
        }
      }
    }
    public void Save(bool forced = false) {
      if (string.IsNullOrEmpty(filename)) { return; }
      if ((changed == false)&&(forced == false)) { return; }
      File.WriteAllText(filename, JsonConvert.SerializeObject(content, Formatting.Indented));
    }
  }
  public class ModRecord {
    public string Name { get; set; }
    public string Path { get; set; }
    public ModRecord(string n, string p) {
      Name = n; Path = p;
    }
    private static Regex rgx = new Regex("[^a-zA-Z0-9 -]");
    public static string Normilize(string val) {
      Regex rgx = new Regex("[^a-zA-Z0-9\\-_]");
      return rgx.Replace(val, "_");
    }
    public static List<ModRecord> GatherMods(string modsPath) {
      List<ModRecord> result = new List<ModRecord>();
      foreach(string modPath in Directory.GetDirectories(modsPath)) {
        string manifestPath = System.IO.Path.Combine(modPath,"mod.json");
        try {
          JObject mainfest = JObject.Parse(File.ReadAllText(manifestPath));
          result.Add(new ModRecord((string)mainfest["Name"], modPath));
        } catch (Exception) {
          continue;
        }
      }
      return result;
    }
    public static void GetAllJsons(string path, ref List<string> jsons, int initiation) {
      try {
        string init = new string(' ', initiation);
        foreach (string d in Directory.GetDirectories(path)) {
          Console.WriteLine(init + d);
          foreach (string f in Directory.GetFiles(d)) {
            Console.WriteLine(init + " " + f + ":" + System.IO.Path.GetExtension(f).ToUpper());
            if (System.IO.Path.GetExtension(f).ToUpper() == ".JSON") {
              jsons.Add(f);
            } else if (System.IO.Path.GetExtension(f).ToUpper() == ".BYTES") {
              jsons.Add(f);
            }
          }
          GetAllJsons(d, ref jsons, initiation + 1);
        }
      } catch (System.Exception excpt) {
        Console.WriteLine(excpt.Message);
      }
    }
  }
  public abstract class jtProcGeneric {
    public virtual string Name { get { return "Generic"; } }
    public jtProcGeneric() { }
    public abstract bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check);
  }
  public class jtUIname : jtProcGeneric {
    public override string Name { get { return "Description.UIName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["UIName"] == null) { return false; };
      string value = (string)json["Description"]["UIName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".UIName";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["Description"]["UIName"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["UIName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtName : jtProcGeneric {
    public override string Name { get { return "Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["Name"] == null) { return false; };
      string value = (string)json["Description"]["Name"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".Name";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      string original = (string)json["Description"]["Name"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtDetails : jtProcGeneric {
    public override string Name { get { return "Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["Details"] == null) { return false; };
      string value = (string)json["Description"]["Details"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".Details";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["Description"]["Details"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtFirstName : jtProcGeneric {
    public override string Name { get { return "Description.FirstName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["FirstName"] == null) { return false; };
      string value = (string)json["Description"]["FirstName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".FirstName";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["Description"]["FirstName"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["FirstName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtLastName : jtProcGeneric {
    public override string Name { get { return "Description.LastName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["LastName"] == null) { return false; };
      string value = (string)json["Description"]["LastName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".LastName";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["Description"]["LastName"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["LastName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtCallsign : jtProcGeneric {
    public override string Name { get { return "Description.Callsign"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Description"] == null) { return false; };
      if (json["Description"]["Callsign"] == null) { return false; };
      string value = (string)json["Description"]["Callsign"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".Callsign";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["Description"]["Callsign"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["Description"]["Callsign"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtStockRole : jtProcGeneric {
    public override string Name { get { return "StockRole"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["StockRole"] == null) { return false; };
      string value = (string)json["StockRole"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".StockRole";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["StockRole"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["StockRole"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtYangsThoughts : jtProcGeneric {
    public override string Name { get { return "YangsThoughts"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["YangsThoughts"] == null) { return false; };
      string value = (string)json["YangsThoughts"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".YangsThoughts";
      MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
      if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; return false; };
      string original = (string)json["YangsThoughts"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["YangsThoughts"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtEffectDataName : jtProcGeneric {
    public override string Name { get { return "EffectData.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["EffectData"] == null) { return false; };
      if (json["EffectData"].Count() == 0) { return false; };
      int count = json["EffectData"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["EffectData"][t]["Description"] == null) { continue; }
        string value = (string)json["EffectData"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["EffectData"][t]["Description"]["Id"] + ".effect" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["EffectData"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtEffectDataDetails : jtProcGeneric {
    public override string Name { get { return "EffectData.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["EffectData"] == null) { return false; };
      if (json["EffectData"].Count() == 0) { return false; };
      int count = json["EffectData"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["EffectData"][t]["Description"] == null) { continue; }
        string value = (string)json["EffectData"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["EffectData"][t]["Description"]["Id"] + ".effect" + t + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["EffectData"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtStatusEffectsName : jtProcGeneric {
    public override string Name { get { return "statusEffects.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["statusEffects"] == null) { return false; };
      if (json["statusEffects"].Count() == 0) { return false; };
      int count = json["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["statusEffects"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["statusEffects"][t]["Description"]["Id"] + ".effect" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["statusEffects"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtStatusEffectsDetails : jtProcGeneric {
    public override string Name { get { return "statusEffects.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["statusEffects"] == null) { return false; };
      if (json["statusEffects"].Count() == 0) { return false; };
      int count = json["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["statusEffects"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["statusEffects"][t]["Description"]["Id"] + ".effect" + t + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["statusEffects"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtModeStatusEffectsName : jtProcGeneric {
    public override string Name { get { return "Mode.statusEffects.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Modes"] == null) { return false; };
      if (json["Modes"].Count() == 0) { return false; };
      int count = json["Modes"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Modes"][t]["statusEffects"] == null) { continue; }
        int count2 = json["Modes"][t]["statusEffects"].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json["Modes"][t]["statusEffects"][tt]["Description"] == null) { continue; }
          string value = (string)json["Modes"][t]["statusEffects"][tt]["Description"]["Name"];
          if (string.IsNullOrEmpty(value)) { continue; };
          string key = modName + "." + filename + "." + json["Modes"][t]["statusEffects"][tt]["Description"]["Id"] + ".mode" + t + "." + tt + ".Name";
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            json["Modes"][t]["statusEffects"][tt]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtModeStatusEffectsDetails : jtProcGeneric {
    public override string Name { get { return "Mode.statusEffects.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Modes"] == null) { return false; };
      if (json["Modes"].Count() == 0) { return false; };
      int count = json["Modes"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Modes"][t]["statusEffects"] == null) { continue; }
        int count2 = json["Modes"][t]["statusEffects"].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json["Modes"][t]["statusEffects"][tt]["Description"] == null) { continue; }
          string value = (string)json["Modes"][t]["statusEffects"][tt]["Description"]["Details"];
          if (string.IsNullOrEmpty(value)) { continue; };
          string key = modName + "." + filename + "." + json["Modes"][t]["statusEffects"][tt]["Description"]["Id"] + ".mode" + t + "." + tt + ".Details";
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            json["Modes"][t]["statusEffects"][tt]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEStatusEffectsName : jtProcGeneric {
    public override string Name { get { return "CAE.statusEffects.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["statusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["statusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Id"] + ".CAE" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEStatusEffectsDetails : jtProcGeneric {
    public override string Name { get { return "CAE.statusEffects.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["statusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["statusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Id"] + ".CAE" + t + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEofflineStatusEffectsName : jtProcGeneric {
    public override string Name { get { return "CAE.offlineStatusEffects.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["offlineStatusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["offlineStatusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["offlineStatusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Id"] + ".CAEOFF" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEofflineStatusEffectsDetails : jtProcGeneric {
    public override string Name { get { return "CAE.offlineStatusEffects.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["offlineStatusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["offlineStatusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["offlineStatusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Id"] + ".CAEOFF" + t + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["offlineStatusEffects"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEExplosionStatusEffectsName : jtProcGeneric {
    public override string Name { get { return "CAE.Explosion.statusEffects.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Id"] + ".Expl" + t + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtCAEExplosionStatusEffectsDetails : jtProcGeneric {
    public override string Name { get { return "CAE.Explosion.statusEffects.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Custom"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"] == null) { return false; };
      if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"].Count() == 0) { return false; };
      int count = json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"] == null) { continue; }
        string value = (string)json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Id"] + ".Expl" + t + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Custom"]["ActivatableComponent"]["Explosion"]["statusEffects"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtcontractName : jtProcGeneric {
    public override string Name { get { return "contractName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["contractName"] == null) { return false; };
      string value = (string)json["contractName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".contractName";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      string original = (string)json["contractName"];
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["contractName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, original);
      }
      inc = json;
      return true;
    }
  }
  public class jtshortDescription : jtProcGeneric {
    public override string Name { get { return "shortDescription"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["shortDescription"] == null) { return false; };
      string value = (string)json["shortDescription"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".shortDescription";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["shortDescription"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtShortDesc : jtProcGeneric {
    public override string Name { get { return "ShortDesc"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["ShortDesc"] == null) { return false; };
      string value = (string)json["ShortDesc"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".ShortDesc";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["ShortDesc"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtfirstName : jtProcGeneric {
    public override string Name { get { return "firstName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["firstName"] == null) { return false; };
      string value = (string)json["firstName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".firstName";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["firstName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtlastName : jtProcGeneric {
    public override string Name { get { return "lastName"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["lastName"] == null) { return false; };
      string value = (string)json["lastName"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".lastName";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["lastName"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtcallsign : jtProcGeneric {
    public override string Name { get { return "callsign"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["callsign"] == null) { return false; };
      string value = (string)json["callsign"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".callsign";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["callsign"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtrank : jtProcGeneric {
    public override string Name { get { return "rank"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["rank"] == null) { return false; };
      string value = (string)json["rank"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".rank";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["rank"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtlongDescription : jtProcGeneric {
    public override string Name { get { return "longDescription"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["longDescription"] == null) { return false; };
      string value = (string)json["longDescription"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".longDescription";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["longDescription"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtFlashpointShortDescription : jtProcGeneric {
    public override string Name { get { return "FlashpointShortDescription"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["FlashpointShortDescription"] == null) { return false; };
      string value = (string)json["FlashpointShortDescription"];
      if (string.IsNullOrEmpty(value)) { return false; };
      string key = modName + "." + filename + ".FlashpointShortDescription";
      if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); return false; };
      if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
        json["FlashpointShortDescription"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
        replaced.Add(key, value);
      }
      inc = json;
      return true;
    }
  }
  public class jtobjectiveList_title : jtProcGeneric {
    public override string Name { get { return "objectiveList.title"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["objectiveList"] == null) { return false; };
      if (json["objectiveList"].Count() == 0) { return false; };
      int count = json["objectiveList"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["objectiveList"][t]["title"] == null) { continue; }
        string value = (string)json["objectiveList"][t]["title"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + "objective" + t + ".title";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["objectiveList"][t]["title"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtobjectiveList_description : jtProcGeneric {
    public override string Name { get { return "objectiveList.description"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["objectiveList"] == null) { return false; };
      if (json["objectiveList"].Count() == 0) { return false; };
      int count = json["objectiveList"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["objectiveList"][t]["description"] == null) { continue; }
        string value = (string)json["objectiveList"][t]["description"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + "objective" + t + ".description";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["objectiveList"][t]["description"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtcontents_words : jtProcGeneric {
    public override string Name { get { return "contents.words"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["contents"] == null) { return false; };
      if (json["contents"].Count() == 0) { return false; };
      int count = json["contents"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["contents"][t]["words"] == null) { continue; }
        string value = (string)json["contents"][t]["words"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + "content" + t + ".words";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["contents"][t]["words"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtdialogueListdialogueContentWords : jtProcGeneric {
    public override string Name { get { return "dialogueList.dialogueContent.words"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["dialogueList"] == null) { return false; };
      if (json["dialogueList"].Count() == 0) { return false; };
      int count = json["dialogueList"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["dialogueList"][t]["dialogueContent"] == null) { continue; }
        int count2 = json["dialogueList"][t]["dialogueContent"].Count();
        for (int tt = 0; tt < count2; ++tt) {
          string value = (string)json["dialogueList"][t]["dialogueContent"][tt]["words"];
          if (string.IsNullOrEmpty(value)) { continue; };
          string key = modName + "." + filename + "."  + ".dialogueList" + t + ".dialogueContent" + tt + ".words";
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            json["dialogueList"][t]["dialogueContent"][tt]["words"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtOptionsDescriptionName : jtProcGeneric {
    public override string Name { get { return "Options.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Options"] == null) { return false; };
      if (json["Options"].Count() == 0) { return false; };
      int count = json["Options"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Options"][t]["Description"] == null) { continue; }
        string value = (string)json["Options"][t]["Description"]["Name"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + ".Options" + t + "." + json["Options"][t]["Description"]["Id"] + ".Name";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Options"][t]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtOptionsDescriptionDetails : jtProcGeneric {
    public override string Name { get { return "Options.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Options"] == null) { return false; };
      if (json["Options"].Count() == 0) { return false; };
      int count = json["Options"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Options"][t]["Description"] == null) { continue; }
        string value = (string)json["Options"][t]["Description"]["Details"];
        if (string.IsNullOrEmpty(value)) { continue; };
        string key = modName + "." + filename + "." + ".Options" + t + "."+json["Options"][t]["Description"]["Id"] + ".Details";
        MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
        if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          json["Options"][t]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtOptionsResultSetsDescriptionName : jtProcGeneric {
    public override string Name { get { return "Options.ResultSets.Description.Name"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Options"] == null) { return false; };
      if (json["Options"].Count() == 0) { return false; };
      int count = json["Options"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Options"][t]["ResultSets"] == null) { continue; }
        int count2 = json["Options"][t]["ResultSets"].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json["Options"][t]["ResultSets"][tt]["Description"] == null) { continue; }
          string value = (string)json["Options"][t]["ResultSets"][tt]["Description"]["Name"];
          if (string.IsNullOrEmpty(value)) { continue; };
          string key = modName + "." + filename + "." + ".Options" + t + ".ResultSets" + tt + "Description.Name";
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            json["Options"][t]["ResultSets"][tt]["Description"]["Name"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtOptionsResultSetsDescriptionDetails : jtProcGeneric {
    public override string Name { get { return "Options.ResultSets.Description.Details"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      JObject json = inc as JObject;
      if (json == null) { return false; }
      if (json["Options"] == null) { return false; };
      if (json["Options"].Count() == 0) { return false; };
      int count = json["Options"].Count();
      for (int t = 0; t < count; ++t) {
        if (json["Options"][t]["ResultSets"] == null) { continue; }
        int count2 = json["Options"][t]["ResultSets"].Count();
        for (int tt = 0; tt < count2; ++tt) {
          if (json["Options"][t]["ResultSets"][tt]["Description"] == null) { continue; }
          string value = (string)json["Options"][t]["ResultSets"][tt]["Description"]["Details"];
          if (string.IsNullOrEmpty(value)) { continue; };
          string key = modName + "." + filename + "." + ".Options" + t + ".ResultSets" + tt + "Description.Details";
          MatchCollection matches = CustomTranslation.Core.locRegEx.Matches(value);
          if (matches.Count != 0) { foreach (Match match in matches) { replaced.Add(match.Groups[1].Value, String.Empty); }; continue; };
          if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
            json["Options"][t]["ResultSets"][tt]["Description"]["Details"] = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
            replaced.Add(key, value);
          }
        }
      }
      inc = json;
      return true;
    }
  }
  public class jtConversations : jtProcGeneric {
    public override string Name { get { return "Conversations(binary)"; } }
    public override bool proc(string modName, string filename, ref object inc, Dictionary<string, string> replaced, bool check) {
      ConversationFile cFile = inc as ConversationFile;
      if (cFile == null) { return false; }
      //Log.Debug?.Write("Conversation:"+cFile.FileName+"\n");
      if (cFile.conversation == null) {
        //Log.Debug?.Write(" null\n");
        return false;
      };
      //Log.Debug?.Write(" ui_name:" + cFile.conversation.ui_name + "\n");

      //Log.Debug?.Write(" roots:"+ cFile.conversation.roots.Count + "\n");
      for(int t = 0; t < cFile.conversation.roots.Count; ++t) {
        string value = cFile.conversation.roots[t].responseText;
        string key = modName + "." + filename + ".root" + t + ".responseText";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          cFile.conversation.roots[t].responseText = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
      }
      //Log.Debug?.Write(" nodes:\n");
      for (int t = 0; t < cFile.conversation.nodes.Count; ++t) {
        string value = cFile.conversation.nodes[t].text;
        string key = modName + "." + filename + ".node" + t + ".text";
        if (CustomTranslation.Core.locRegEx.Matches(value).Count != 0) { replaced.Add(key, String.Empty); continue; };
        if ((check == false) || (Core.stringsTable.ContainsKey(key) == true)) {
          cFile.conversation.nodes[t].text = CustomTranslation.Core.LocalizationRefPrefix + key + CustomTranslation.Core.LocalizationRefSufix;
          replaced.Add(key, value);
        }
        //Log.Debug?.Write(" [" + t + "] comment:" + cFile.conversation.nodes[t].comment + "\n");
        //Log.Debug?.Write(" [" + t + "] text:" + cFile.conversation.nodes[t].text + "\n");
        //Log.Debug?.Write(" [" + t + "] branches:" + cFile.conversation.nodes[t].branches.Count + "\n");
        for (int tt = 0; tt < cFile.conversation.nodes[t].branches.Count; ++tt) {
          value = cFile.conversation.nodes[t].branches[tt].responseText;
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
  }
}