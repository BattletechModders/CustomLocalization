using CustomTranslation;
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
using System.Windows.Forms;

namespace CustomLocalizationSetup {
  public partial class MainForm : Form {
    private List<object> partsList;
    public MainForm() {
      InitializeComponent();
      try {
        partsList = new List<object>();

        /*partsList.Add(new jtUIname());
        partsList.Add(new jtName());
        partsList.Add(new jtDetails());
        partsList.Add(new jtEffectDataName());
        partsList.Add(new jtEffectDataDetails());
        partsList.Add(new jtStatusEffectsName());
        partsList.Add(new jtStatusEffectsDetails());
        partsList.Add(new jtModeStatusEffectsName());
        partsList.Add(new jtModeStatusEffectsDetails());
        partsList.Add(new jtCAEStatusEffectsName());
        partsList.Add(new jtCAEStatusEffectsDetails());
        partsList.Add(new jtCAEofflineStatusEffectsName());
        partsList.Add(new jtCAEofflineStatusEffectsDetails());
        partsList.Add(new jtCAEExplosionStatusEffectsName());
        partsList.Add(new jtCAEExplosionStatusEffectsDetails());
        partsList.Add(new jtcontractName());
        partsList.Add(new jtshortDescription());
        partsList.Add(new jtlongDescription());
        partsList.Add(new jtFlashpointShortDescription());
        partsList.Add(new jtobjectiveList_title());
        partsList.Add(new jtobjectiveList_description());
        partsList.Add(new jtcontents_words());
        partsList.Add(new jtdialogueListdialogueContentWords());
        partsList.Add(new jtOptionsDescriptionName());
        partsList.Add(new jtOptionsDescriptionDetails());
        partsList.Add(new jtOptionsResultSetsDescriptionName());
        partsList.Add(new jtOptionsResultSetsDescriptionDetails());
        partsList.Add(new jtConversations());*/
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
      backgroundWorker.RunWorkerAsync();
    }

    private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      Application.Exit();
      Environment.Exit(0);
      Application.ExitThread();
    }
    public bool IsInVanilla(string value) {
      return false;
    }
    private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
      try {
        List<ModDirRecord> mods = ModDirRecord.GatherMods(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), ".."));
        Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
        Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
        int modcounter = 0;
        foreach (ModDirRecord mod in mods) {
          backgroundWorker.ReportProgress((int)Math.Round((float)modcounter* 100.0f/(float)mods.Count)); ++modcounter;
          string modName = ModDirRecord.Normilize(mod.name);
          //MessageBox.Show(modName);
          List<string> jsonsPath = new List<string>();
          //ModDirRecord.GetAllJsons(mod.path, ref jsonsPath, 0);
          foreach (string jsonPath in jsonsPath) {
            bool updated = false;
            //MessageBox.Show(jsonPath);
            string filename = ModDirRecord.Normilize(Path.GetFileNameWithoutExtension(jsonPath));
            object content = null;
            if (Path.GetFileName(jsonPath).ToUpper() == "LOCALIZATION.JSON") { continue; }
            if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
              string jsonCont = File.ReadAllText(jsonPath);
              content = JObject.Parse(jsonCont);
            } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
              content = new ConversationFile(jsonPath);
            }
            foreach (var jtproc in partsList) {
              jtProcGenericEx jtProc = jtproc as jtProcGenericEx;
              if (jtProc == null) { continue; }
              Dictionary<string, string> replaced = new Dictionary<string, string>();
              //jtProc.proc(modName, filename, ref content, replaced, true, new Func<string, bool>(IsInVanilla));
              foreach (var replacements in replaced) {
                if (string.IsNullOrEmpty(replacements.Value) == false) { updated = true; }
              }
            }
            if (updated) {
              if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
                JObject json = content as JObject;
                if (json != null) { jsonUpdatedContent.Add(jsonPath, json.ToString(Formatting.Indented)); };
              } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
                ConversationFile cfile = content as ConversationFile;
                if (cfile != null) { convUpdatedContent.Add(jsonPath, cfile); };
              }
            };
          }
        }
        foreach (var uJsons in jsonUpdatedContent) {
          File.WriteAllText(uJsons.Key, uJsons.Value);
        }
        foreach (var uConv in convUpdatedContent) {
          uConv.Value.Save();
        }
      } catch (Exception ex) {
        MessageBox.Show(ex.ToString());
      }
    }

    private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      this.progressBar.Value = e.ProgressPercentage;
    }
  }
}
