using CustomTranslation;
using isogame;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace CustormLocalizationPrepare {
  public partial class MainForm : Form {
    public MainForm() {
      Core.Settings = new CTSettings();
      Core.Settings.debugLog = true;
      Log.BaseDirectory = Path.GetDirectoryName(Application.ExecutablePath);
      Log.InitLog();
      InitializeComponent();
      modsList.DisplayMember = "Name";
      partsList.DisplayMember = "Name";
      saveFileDialog.FileName = CustomTranslation.Core.LocalizationFileName;
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_DE_DE);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_ZH_CN);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_ES_ES);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_FR_FR);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_IT_IT);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_RU_RU);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_PT_PT);
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_PT_BR);
      partsList.Items.Add(new jtUIname());
      partsList.Items.Add(new jtName());
      partsList.Items.Add(new jtDetails());
      partsList.Items.Add(new jtStatusEffectsName());
      partsList.Items.Add(new jtStatusEffectsDetails());
      partsList.Items.Add(new jtModeStatusEffectsName());
      partsList.Items.Add(new jtModeStatusEffectsDetails());
      partsList.Items.Add(new jtCAEStatusEffectsName());
      partsList.Items.Add(new jtCAEStatusEffectsDetails());
      partsList.Items.Add(new jtCAEofflineStatusEffectsName());
      partsList.Items.Add(new jtCAEofflineStatusEffectsDetails());
      partsList.Items.Add(new jtCAEExplosionStatusEffectsName());
      partsList.Items.Add(new jtCAEExplosionStatusEffectsDetails());
      partsList.Items.Add(new jtcontractName());
      partsList.Items.Add(new jtshortDescription());
      partsList.Items.Add(new jtlongDescription());
      partsList.Items.Add(new jtFlashpointShortDescription());
      partsList.Items.Add(new jtobjectiveList_title());
      partsList.Items.Add(new jtobjectiveList_description());
      partsList.Items.Add(new jtcontents_words());
      partsList.Items.Add(new jtdialogueListdialogueContentWords());
      partsList.Items.Add(new jtOptionsDescriptionName());
      partsList.Items.Add(new jtOptionsDescriptionDetails());
      partsList.Items.Add(new jtOptionsResultSetsDescriptionName());
      partsList.Items.Add(new jtOptionsResultSetsDescriptionDetails());
      partsList.Items.Add(new jtConversations());
    }
    private void openMods_Click(object sender, EventArgs e) {
      if (GameBaseSelector.ShowDialog() == DialogResult.OK) {
        modsList.Items.Clear();
        string modsDir = Path.Combine(GameBaseSelector.SelectedPath, "Mods");
        tbGamePath.Text = GameBaseSelector.SelectedPath;
        saveFileDialog.InitialDirectory = modsDir;
        foreach (string modPath in Directory.GetDirectories(modsDir)) {
          string modManifest = Path.Combine(modPath,"mod.json");
          if (File.Exists(modManifest)) {
            try {
              JObject jManifest = JObject.Parse(File.ReadAllText(modManifest));
              modsList.Items.Add(new ModRecord((string)jManifest["Name"],modPath));
            } catch(Exception ex) {
              MessageBox.Show(modManifest+" - contains errors\n"+ex.ToString());
            }
          }
        }
      }
    }

    private void backgroundParse_ProgressChanged(object sender, ProgressChangedEventArgs e) {

    }

    private void backgroundParse_DoWork(object sender, DoWorkEventArgs e) {

    }
    public static void GetAllJsons(string path, ref List<string> jsons, int initiation) {
      try {
        string init = new string(' ', initiation);
        foreach (string d in Directory.GetDirectories(path)) {
          Console.WriteLine(init + d);
          foreach (string f in Directory.GetFiles(d)) {
            Console.WriteLine(init + " " + f + ":" + Path.GetExtension(f).ToUpper());
            if (Path.GetExtension(f).ToUpper() == ".JSON") {
              jsons.Add(f);
            }else if (Path.GetExtension(f).ToUpper() == ".BYTES") {
              jsons.Add(f);
            }
          }
          GetAllJsons(d, ref jsons, initiation + 1);
        }
      } catch (System.Exception excpt) {
        Console.WriteLine(excpt.Message);
      }
    }
    private static Regex rgx = new Regex("[^a-zA-Z0-9 -]");
    public static string Normilize(string val) {
      Regex rgx = new Regex("[^a-zA-Z0-9\\-_]");
      return rgx.Replace(val, "_");
    }

    private void PrepareMods_Click(object sender, EventArgs e) {
      if(saveFileDialog.ShowDialog() == DialogResult.OK) {
        try {
          LocalizationFile locFile = new LocalizationFile(saveFileDialog.FileName);
          List<ModRecord> mods = modsList.CheckedItems.OfType<ModRecord>().ToList<ModRecord>();
          Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
          Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
          foreach (ModRecord mod in mods) {
            string modName = Normilize(mod.Name);
            List<string> jsonsPath = new List<string>();
            GetAllJsons(mod.Path, ref jsonsPath, 0);
            foreach(string jsonPath in jsonsPath) {
              bool updated = false;
              //MessageBox.Show(jsonPath);
              string filename = Normilize(Path.GetFileNameWithoutExtension(jsonPath));
              object content = null;
              if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
                string jsonCont = File.ReadAllText(jsonPath);
                content = JObject.Parse(jsonCont);
              } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
                content = new ConversationFile(jsonPath);
              }
              foreach(var jtproc in partsList.CheckedItems) {
                jtProcGeneric jtProc = jtproc as jtProcGeneric;
                if (jtProc == null) { continue; }
                Dictionary<string, string> replaced = new Dictionary<string, string>();
                jtProc.proc(modName, filename, ref content, replaced, false);
                foreach (var replacements in replaced) {
                  if (string.IsNullOrEmpty(replacements.Value) == false) {
                    CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
                    nTr.FileName = jsonPath;
                    nTr.Name = replacements.Key;
                    nTr.Localization.Add(Localize.Strings.Culture.CULTURE_EN_US, replacements.Value);
                    foreach (Localize.Strings.Culture locLang in langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()) {
                      nTr.Localization.Add(locLang, replacements.Value);
                    }
                    locFile.Merge(nTr);
                    updated = true;
                  } else {
                    if (locFile.map.ContainsKey(replacements.Key)) {
                      if (locFile.map[replacements.Key].Localization.ContainsKey(Localize.Strings.Culture.CULTURE_EN_US)) {
                        CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
                        nTr.FileName = jsonPath;
                        nTr.Name = replacements.Key;
                        foreach (Localize.Strings.Culture locLang in langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()) {
                          if (locFile.map[replacements.Key].Localization.ContainsKey(locLang) == false) {
                            nTr.Localization.Add(locLang, locFile.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US]);
                          }
                        }
                        locFile.Merge(nTr);
                      }
                    }
                  }
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
          locFile.Save();
          foreach (var uJsons in jsonUpdatedContent) {
            File.WriteAllText(uJsons.Key, uJsons.Value);
          }
          foreach (var uConv in convUpdatedContent) {
            uConv.Value.Save();
          }
          MessageBox.Show("Done");
        }catch(Exception ex) {
          MessageBox.Show(ex.ToString());
        }
      }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      //MessageBox.Show("FormClosed");
      Application.Exit();
      Environment.Exit(0);
      Application.ExitThread();
    }
  }
}
