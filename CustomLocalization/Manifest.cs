using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomTranslation {
  public static class ManifestHelper {
    private static bool inited = false;
    public static List<LocalizationDef> localizationFiles { get; set; } = new List<LocalizationDef>();
    public static Dictionary<string, LocalizationRecordDef> localizable { get; set; } = new Dictionary<string, LocalizationRecordDef>();
    public static void GatherLocalizationFiles(string directory, Action<float, string> progress) {
      string[] locDefs = Directory.GetFiles(directory, "Localization?*.json", SearchOption.AllDirectories);
      float progress_counter = 0f;
      foreach (string locDef in locDefs) {
        Log.M?.TWL(0, locDef);
        progress_counter+=(1000f/ (float)locDefs.Length);
        if (progress_counter > 1f) { progress_counter = 1f; }
        progress?.Invoke(progress_counter, locDef);
        try {
          if (Path.GetFileName(locDef).ToUpper() == Core.LocalizationFileName.ToUpper()) { continue; }
          LocalizationDef def = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(locDef));
          def.filename = locDef;
          if (def.culture != Core.defaultCulture) { continue; }
          localizationFiles.Add(def);
          foreach (var locRec in def.content) {
            if(localizable.TryGetValue(locRec.id, out var existing)) {
              if (existing.parent == null) {
                locRec.filename = existing.filename;
                locRec.prevOriginal = locRec.original;
                locRec.original = existing.original;
                locRec.parent = def;
                localizable[locRec.id] = locRec;
                if (locRec.prevOriginal != locRec.original) { ++def.diffcounter; }
              } else {
                Log.Er?.TWL(0,$"Conflict {def.filename} and {existing.parent.filename} both have {locRec.id}");
              }
            }
          }
        } catch (Exception e) {
          Log.Er?.TWL(0, locDef, true);
          Log.Er?.TWL(0, e.ToString(), true);
        }
      }
    }
    public static void ProcessManifest(string manifestPath, Action<float, string> progress) {
      if (inited) { return; }
      inited = true;
      try {
        using (var manifestStream = new StreamReader(manifestPath)) {
          using (var manifest = new CSVFile.CSVReader(manifestStream)) {
            int progress_counter = 0;
            int lines = 0;
            foreach (var line in manifest) {
              ++lines;
              try {
                if (string.IsNullOrWhiteSpace(line[7]) == false) { goto next; }
                if (File.Exists(line[2]) == false) { goto next; }
                if (line[2].StartsWith(Core.ModsRootDirectory) == false) { goto next; }
                if (Core.Settings.procSettings.TryGetValue(line[1], out var procs)) {
                  Log.M?.WL(0, $"{line[0]}:{line[1]} proc count:{procs.procs.Count}");
                  if (procs.procs.Count == 0) {
                    goto next;
                  }
                  object entry = null;
                  try {
                    entry = JObject.Parse(File.ReadAllText(line[2]));
                  } catch (Exception e) {
                    Log.Er?.TWL(0, e.ToString(), true);
                  }
                  Log.M?.WL(0, $"{line[0]}:{line[1]}");
                  foreach (var procname in procs.procs) {
                    if (Core.localizationMethods.TryGetValue(procname, out var proc)) {
                      Dictionary<string, string> locrecs = new Dictionary<string, string>();
                      bool ret = proc.extract(string.Empty, line[0], ref entry, locrecs);
                      Log.M?.WL(1, $"{procname}:{ret}:{locrecs.Count}");
                      foreach (var locrec in locrecs) {
                        LocalizationRecordDef locDef = new LocalizationRecordDef();
                        locDef.id = locrec.Key;
                        locDef.processor = procname;
                        locDef.original = locrec.Value;
                        locDef.prevOriginal = locrec.Value;
                        locDef.content = locrec.Value;
                        locDef.filename = line[2];
                        localizable[locDef.id] = locDef;
                      }
                    }
                  }
                }
              next:
                ++progress_counter;
                if (progress_counter > 1000) { progress_counter = 0; }
                progress?.Invoke((float)progress_counter / 1000f, line[0]);
              }catch(Exception e) {
                Log.Er?.TWL(0, e.ToString(), true);
              }
            }
            Log.M?.TWL(0, $"lines:{lines}");
          }
        }
      } catch(Exception e) {
        Log.Er?.TWL(0, e.ToString(), true);
      }
    }
  }
}