using CustomTranslation;
using CustormLocalizationPrepare;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomLocalizationPrepare {
  public partial class ProcessForm : Form {
    public List<LocalizationTask> prepareRecords;
    public bool DeleteOtherTranslations;
    public LocalizationDef result;
    public ProcessCommand command = ProcessCommand.Create;
    public ProcessForm() {
      InitializeComponent();
    }
    private void addIndexes(LocalizationIndexDirectory parent, ref int count) {
      string[] dirs = Directory.GetDirectories(parent.path);
      Log.M?.TWL(0, "addIndexes:" + parent.path);
      foreach(string dir in dirs) {
        if (Path.GetFileName(dir).StartsWith(".")) { continue; }
        Log.M?.WL(1, "dir:" + dir);
        LocalizationIndexDirectory child = new LocalizationIndexDirectory();
        child.path = dir;
        child.name = Path.GetFileName(dir);
        HashSet<string> files = ModDirRecord.fillFiles(child.path);
        foreach (string file in files) {
          child.files.Add(file, new Dictionary<string, List<string>>());
          ++count;
        }
        parent.childs.Add(child);
        addIndexes(child, ref count);
      }
    }
    private void checkIndexes(LocalizationIndexDirectory parent, ref int count) {
      Log.M?.TWL(0, "checkIndexes:" + parent.name);
      foreach (LocalizationIndexDirectory child in parent.childs) {
        Log.M?.WL(1, "child:" + child.path);
        HashSet<string> cfiles = new HashSet<string>();
        foreach (var file in child.files) {
          cfiles.Add(file.Key);
        }
        foreach (string file in cfiles) {
          string filepath = Path.Combine(child.path, file);
          string filename = Path.GetFileNameWithoutExtension(filepath);
          Log.M?.WL(1, "file:" + file+":"+filename+":"+filepath);
          object content = null;
          try {
            if (Path.GetExtension(filepath).ToUpper() == ".JSON") {
              string jsonCont = File.ReadAllText(filepath);
              content = JObject.Parse(jsonCont);
            } else if (Path.GetExtension(filepath).ToUpper() == ".BYTES") {
              content = new ConversationFile(filepath);
            }
          } catch (Exception ex) {
            count += Core.localizationMethods.Count;
            backgroundWorker.ReportProgress(count);
            Log.M?.TWL(0, ex.ToString(), true);
            continue;
          }
          foreach(var proc in Core.localizationMethods) {
            HashSet<string> keys = new HashSet<string>();
            Log.M?.WL(2, "proc:"+proc.Value.Name+":"+keys.Count);
            try {
              proc.Value.check(string.Empty, filename, ref content, ref keys);
              if (keys.Count != 0) {
                child.files[file].Add(proc.Value.Name, keys.ToList());
              }
            }catch(Exception e) {
              Log.M?.TWL(0, e.ToString(), true);
            }
            ++count;
            backgroundWorker.ReportProgress(count);
          }
        }
        checkIndexes(child, ref count);
      }
    }
    public void CreateIndex() {
      this.progressBar.Value = 0;
      int count = 0;
      LocalizationLoader.InitIndexes(Core.CurRootDirectory, false);
      addIndexes(LocalizationLoader.indexRoot, ref count);
      this.progressBar.Maximum = count * Core.localizationMethods.Count;
      count = 0;
      checkIndexes(LocalizationLoader.indexRoot,ref count);
      File.WriteAllText(Path.Combine(Core.CurRootDirectory, "index.json"),JsonConvert.SerializeObject(LocalizationLoader.indexRoot,Formatting.Indented));
    }
    public void UpdateDefinitions() {
      CreateIndex();
      try {
        List<LocalizationDef> updated = new List<LocalizationDef>();
        foreach (var locFile in Core.loadedDefinitions) {
          if (locFile.Value.culture != Core.defaultCulture) { continue; }
          backgroundWorker.ReportProgress(0);
          List<LocalizationTask> tasks = locFile.Value.gatherUpdateTasks();
          int count = 0;
          foreach (LocalizationTask task in tasks) { count += task.procList.Count; }
          this.progressBar.Maximum = count;
          Log.M?.TWL(0, "Update:"+ locFile.Value.filename);
          LocalizationDef localizationDef = this.CreateLocalizationFile(tasks,true);
          localizationDef.filename = locFile.Key;
          updated.Add(localizationDef);
        }
        for(int i = 0; i < updated.Count; ++i) {
          LocalizationDef localizationDef = updated[i];
          LocalizationLoader.saveFile(ref localizationDef, true);
          updated[i] = localizationDef;
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public LocalizationDef CreateLocalizationFile(List<LocalizationTask> pRecs,bool addExisting = false) {
      int counter = 0;
      LocalizationDef locDef = new LocalizationDef(Core.defaultCulture);
      locDef.culture = Core.defaultCulture;
      Dictionary<string, TargetDef> targets = new Dictionary<string, TargetDef>();
      HashSet<string> ids = new HashSet<string>();
      HashSet<string> affected = new HashSet<string>();
      foreach (LocalizationTask prepareRecord in pRecs) {
        object content = null;
        Log.M?.TWL(0, "prepare:" + prepareRecord.filename);
        affected.Add(Path.GetFileName(prepareRecord.filename));
        try {
          if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".JSON") {
            string jsonCont = File.ReadAllText(prepareRecord.filename);
            content = JObject.Parse(jsonCont);
          } else if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".BYTES") {
            content = new ConversationFile(prepareRecord.filename);
          }
        } catch (Exception ex) {
          Log.M?.TWL(0, ex.ToString(), true);
          continue;
        }
        string filename = Path.GetFileNameWithoutExtension(prepareRecord.filename);
        foreach (jtProcGenericEx proc in prepareRecord.procList) {
          try {
            Dictionary<string, string> originals = new Dictionary<string, string>();
            proc.extract(prepareRecord.mod.name, filename, ref content, originals);
            Log.M?.WL(1, "extract:" + proc.Name+":"+originals.Count);
            TargetDef targetDef = null;
            foreach (var orig in originals) {
              if (ids.Contains(orig.Key)) { continue; }
              if (addExisting == false) {
                if (Core.stringsTable.TryGetValue(orig.Key.ToUpper(), out var localization)) {
                  if (localization.ContainsKey(Core.defaultCulture)) { continue; }
                }
              }
              ids.Add(orig.Key);
              LocalizationRecordDef locRec = new LocalizationRecordDef();
              locRec.id = orig.Key;
              locRec.original = orig.Value;
              locRec.prevOriginal = orig.Value;
              locRec.content = orig.Value;
              locRec.filename = filename;
              locRec.processor = proc.Name;
              if (targetDef == null) {
                if (targets.TryGetValue(prepareRecord.targetKey, out targetDef) == false) {
                  targetDef = new TargetDef();
                  targetDef.dir.AddRange(prepareRecord.targetFileName);
                  locDef.directories.Add(targetDef);
                  targets.Add(prepareRecord.targetKey, targetDef);
                }
              }
              locDef.content.Add(locRec);
            }
            if (targetDef != null) {
              if (targetDef.processors.Contains(proc.Name) == false) {
                targetDef.processors.Add(proc.Name);
              }
            }
          } catch (Exception ex) {
            Log.M?.TWL(0, filename);
            Log.M?.TWL(0, proc.Name);
            Log.M?.TWL(0, ex.ToString(), true);
          }
          ++counter;
          backgroundWorker.ReportProgress(counter);
        }
      };
      locDef.content = locDef.content.OrderBy(x => x.id).ToList();
      locDef.files = affected.ToList();
      Log.M?.TWL(0, "result:" + (locDef == null ? "null" : "not null"));
      return locDef;
    }
    public void LocalizationReverce(List<LocalizationTask> prepareRecords) {
      int counter = 0;
      foreach (LocalizationTask prepareRecord in prepareRecords) {
        object content = null;
        Log.M?.TWL(0, "prepare:" + prepareRecord.filename);
        try {
          if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".JSON") {
            string jsonCont = File.ReadAllText(prepareRecord.filename);
            content = JObject.Parse(jsonCont);
          } else if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".BYTES") {
            content = new ConversationFile(prepareRecord.filename);
          }
          string filename = Path.GetFileName(prepareRecord.filename);
          bool reversed = false;
          foreach (jtProcGenericEx proc in prepareRecord.procList) {
            Dictionary<string, string> originals = new Dictionary<string, string>();
            Log.M?.WL(1, "reverse:" + proc.Name);
            if (proc.reverse(ref content)) { reversed = true; }
            ++counter;
            backgroundWorker.ReportProgress(counter);
          }
          if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".JSON") {
            if (reversed) {
              File.WriteAllText(prepareRecord.filename, (content as JObject).ToString(Formatting.Indented));
            }
          } else if (Path.GetExtension(prepareRecord.filename).ToUpper() == ".BYTES") {
          }
        } catch (Exception ex) {
          Log.M?.TWL(0, prepareRecord.filename, true);
          Log.M?.TWL(0, ex.ToString(), true);
        }
      };
    }
    private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
      this.result = null;
      try {
        switch (command) {
          case ProcessCommand.Create: this.result = CreateLocalizationFile(prepareRecords); break;
          case ProcessCommand.Reverse: LocalizationReverce(prepareRecords); break;
          case ProcessCommand.Index: CreateIndex(); break;
          case ProcessCommand.Update: UpdateDefinitions(); break;
        }
      } catch(Exception ex) {
        Log.M?.TWL(0,ex.ToString(),true);
      }
    }

    private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      this.progressBar.Value = e.ProgressPercentage;
    }

    private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      this.DialogResult = DialogResult.OK;
    }
  }
  public enum ProcessCommand {
    Create,Reverse,Update,Index
  }
}
