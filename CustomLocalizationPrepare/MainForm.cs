using CustomTranslation;
using isogame;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CustomLocalizationPrepare;

namespace CustormLocalizationPrepare {
  public partial class MainForm : Form {
    public Dictionary<string, string> translationCache;
    public string translationCache_filename;
    public MainForm() {
      this.translationCache = new Dictionary<string, string>();
      Core.Settings = new CTSettings();
      Core.Settings.debugLog = true;
      Log.BaseDirectory = Path.GetDirectoryName(Application.ExecutablePath);
      Log.InitLog();
      InitializeComponent();
      modsList.DisplayMember = "Name";
      partsList.DisplayMember = "Name";
      //string translated = Translator.Translate(text, Language.Chinese_Simplified, Language.English); Console.WriteLine(translated);
      saveFileDialog.FileName = CustomTranslation.Core.LocalizationFileName;
      langsList.Items.Add(Localize.Strings.Culture.CULTURE_EN_US);
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
      partsList.Items.Add(new jtFirstName());
      partsList.Items.Add(new jtLastName());
      partsList.Items.Add(new jtCallsign());
      partsList.Items.Add(new jtStockRole());
      partsList.Items.Add(new jtEffectDataName());
      partsList.Items.Add(new jtEffectDataDetails());
      partsList.Items.Add(new jtYangsThoughts());
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
      partsList.Items.Add(new jtShortDesc());
      partsList.Items.Add(new jtfirstName());
      partsList.Items.Add(new jtlastName());
      partsList.Items.Add(new jtcallsign());
      partsList.Items.Add(new jtrank());
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
        if (Directory.Exists(modsDir) == false) { modsDir = GameBaseSelector.SelectedPath; };
        tbGamePath.Text = GameBaseSelector.SelectedPath;
        saveFileDialog.InitialDirectory = modsDir;
        foreach (string modPath in Directory.GetDirectories(modsDir)) {
          string modManifest = Path.Combine(modPath, "mod.json");
          if (File.Exists(modManifest)) {
            try {
              JObject jManifest = JObject.Parse(File.ReadAllText(modManifest));
              modsList.Items.Add(new ModRecord((string)jManifest["Name"], modPath));
            } catch (Exception ex) {
              MessageBox.Show(modManifest + " - contains errors\n" + ex.ToString());
            }
          }
        }
        this.translationCache_filename = Path.Combine(modsDir, "TranslationCacheRu.json");
        if (File.Exists(translationCache_filename)) {
          translationCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(translationCache_filename));
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
            } else if (Path.GetExtension(f).ToUpper() == ".BYTES") {
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

    //private void PrepareMods_Click(object sender, EventArgs e) {
    //  saveFileDialog.OverwritePrompt = false;
    //  HashSet<string> statistics = new HashSet<string>();
    //  GoogleTranslate t = new GoogleTranslate();
    //  int debugCounter = 0;
    //  int debugCounterMax = 5;
    //  if (saveFileDialog.ShowDialog() == DialogResult.OK) {
    //    string current_file = string.Empty;
    //    try {
    //      bool fileExists = File.Exists(saveFileDialog.FileName);
    //      string baseFile = saveFileDialog.FileName;
    //      LocalizationFile locFile = new LocalizationFile(saveFileDialog.FileName);
    //      List<ModRecord> mods = modsList.CheckedItems.OfType<ModRecord>().ToList<ModRecord>();
    //      Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
    //      Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
    //      foreach (ModRecord mod in mods) {
    //        if (debugCounter > debugCounterMax) { break; }
    //        string modName = Normilize(mod.Name);
    //        List<string> jsonsPath = new List<string>();
    //        GetAllJsons(mod.Path, ref jsonsPath, 0);
    //        foreach (string jsonPath in jsonsPath) {
    //          current_file = jsonPath;
    //          bool updated = false;
    //          //MessageBox.Show(jsonPath);
    //          string filename = Normilize(Path.GetFileNameWithoutExtension(jsonPath));
    //          object content = null;
    //          if (Path.GetFileName(jsonPath).ToUpper() == "LOCALIZATION.JSON") { continue; }
    //          if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
    //            string jsonCont = File.ReadAllText(jsonPath);
    //            content = JObject.Parse(jsonCont);
    //          } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
    //            content = new ConversationFile(jsonPath);
    //          }
    //          foreach (var jtproc in partsList.CheckedItems) {
    //            if (debugCounter > debugCounterMax) { break; }
    //            jtProcGeneric jtProc = jtproc as jtProcGeneric;
    //            if (jtProc == null) { continue; }
    //            Dictionary<string, string> replaced = new Dictionary<string, string>();
    //            jtProc.proc(modName, filename, ref content, replaced, false);
    //            foreach (var replacements in replaced) {
    //              if (debugCounter > debugCounterMax) { break; }
    //              if (string.IsNullOrEmpty(replacements.Value) == false) {
    //                CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
    //                nTr.FileName = jsonPath.Substring(GameBaseSelector.SelectedPath.Length);
    //                nTr.Name = replacements.Key;
    //                nTr.Original = replacements.Value;
    //                //nTr.Localization.Add(Localize.Strings.Culture.CULTURE_EN_US, replacements.Value);
    //                foreach (Localize.Strings.Culture locLang in langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()) {
    //                  string val = replacements.Value;
    //                  string nval = Normilize(val);
    //                  if (locLang == Localize.Strings.Culture.CULTURE_RU_RU) {
    //                    if (val.Length > 30) {
    //                      if (translationCache.ContainsKey(nval) == false) {
    //                        statistics.Add(val);
    //                        MessageBox.Show(val);
    //                        val = GoogleTranslate.Translate(val);
    //                        translationCache.Add(nval, val);
    //                        Log.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
    //                        MessageBox.Show(val);
    //                        ++debugCounter;
    //                      } else {
    //                        val = translationCache[nval];
    //                      }
    //                    }
    //                  }
    //                  nTr.Localization.Add(locLang, val);
    //                }
    //                locFile.Merge(nTr);
    //                updated = true;
    //              } else {
    //                if (locFile.map.ContainsKey(replacements.Key)) {
    //                  string original = locFile.map[replacements.Key].Original;
    //                  if (string.IsNullOrEmpty(original)) {
    //                    original = locFile.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US];
    //                  }
    //                  if (string.IsNullOrEmpty(original) == false) {
    //                    CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
    //                    nTr.FileName = jsonPath.Substring(GameBaseSelector.SelectedPath.Length); ;
    //                    nTr.Name = replacements.Key;
    //                    nTr.Original = original;
    //                    foreach (Localize.Strings.Culture locLang in langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()) {
    //                      if (locFile.map[replacements.Key].Localization.ContainsKey(locLang) == false) {
    //                        string val = original;
    //                        string nval = Normilize(val);
    //                        statistics.Add(val);
    //                        if (locLang == Localize.Strings.Culture.CULTURE_RU_RU) {
    //                          if (val.Length > 30) {
    //                            if (translationCache.ContainsKey(nval) == false) {
    //                              statistics.Add(val);
    //                              MessageBox.Show(val);
    //                              val = GoogleTranslate.Translate(val);
    //                              translationCache.Add(nval, val);
    //                              Log.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
    //                              MessageBox.Show(val);
    //                              ++debugCounter;
    //                            } else {
    //                              val = translationCache[nval];
    //                            }
    //                          }
    //                        }
    //                        nTr.Localization.Add(locLang, val);
    //                      }
    //                    }
    //                    locFile.Merge(nTr);
    //                  }
    //                }
    //              }
    //            }
    //          }
    //          if (updated) {
    //            if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
    //              JObject json = content as JObject;
    //              if (json != null) { jsonUpdatedContent.Add(jsonPath, json.ToString(Formatting.Indented)); };
    //            } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
    //              ConversationFile cfile = content as ConversationFile;
    //              if (cfile != null) { convUpdatedContent.Add(jsonPath, cfile); };
    //            }
    //          };
    //        }
    //        current_file = string.Empty;
    //      }
    //      if (fileExists == false) {
    //        locFile.Save();
    //      } else {
    //        saveFileDialog.InitialDirectory = Path.GetDirectoryName(baseFile);
    //        if (saveFileDialog.ShowDialog() == DialogResult.OK) {
    //          if (saveFileDialog.FileName == baseFile) {
    //            //MessageBox.Show("Same file");
    //            locFile.Save();
    //          } else {
    //            //MessageBox.Show("Different file");
    //            locFile.filename = saveFileDialog.FileName;
    //            locFile.removeOtherTranslations(langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>());
    //            locFile.Save(true);
    //          }
    //        }
    //      }
    //      foreach (var uJsons in jsonUpdatedContent) {
    //        File.WriteAllText(uJsons.Key, uJsons.Value);
    //      }
    //      foreach (var uConv in convUpdatedContent) {
    //        uConv.Value.Save();
    //      }
    //      int overralCharsLength = 0;
    //      foreach (string str in statistics) { overralCharsLength += str.Length; }
    //      MessageBox.Show("Done. Strings: " + statistics.Count + ". Characters:" + overralCharsLength);
    //    } catch (Exception ex) {
    //      MessageBox.Show("in file:" + current_file + "\n" + ex.ToString());
    //    }
    //    string trCache = JsonConvert.SerializeObject(translationCache, Formatting.Indented);
    //    File.WriteAllText(translationCache_filename, trCache);
    //  }
    //}
    private void PrepareMods_Click(object sender, EventArgs e) {
      saveFileDialog.OverwritePrompt = false;
      HashSet<string> statistics = new HashSet<string>();
      GoogleTranslate t = new GoogleTranslate();
      int debugCounter = 0;
      int debugCounterMax = 5;
      bool deleteOtherTranslations = false;
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        string current_file = string.Empty;
        try {
          bool fileExists = File.Exists(saveFileDialog.FileName);
          LocalizationFile locFile = new LocalizationFile(saveFileDialog.FileName);
          if (File.Exists(locFile.filename)) {
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(locFile.filename);
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
              if (saveFileDialog.FileName != locFile.filename) {
                locFile.MergeFile(saveFileDialog.FileName);
                locFile.filename = saveFileDialog.FileName;
                deleteOtherTranslations = true;
              }
            }
          }
          //locFile.DebugLogDump();
          //Application.Exit();
          List<ModRecord> mods = modsList.CheckedItems.OfType<ModRecord>().ToList<ModRecord>();
          Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
          Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
          List<PrepareRecord> prepareRecords = new List<PrepareRecord>();
          foreach (ModRecord mod in mods) {
            if (debugCounter > debugCounterMax) { break; }
            List<string> jsonsPath = new List<string>();
            GetAllJsons(mod.Path, ref jsonsPath, 0);
            foreach (string jsonPath in jsonsPath) {
              current_file = jsonPath;
              object content = null;
              if (Path.GetFileName(jsonPath).ToUpper() == "LOCALIZATION.JSON") { continue; }
              if (Path.GetExtension(jsonPath).ToUpper() == ".JSON") {
                string jsonCont = File.ReadAllText(jsonPath);
                content = JObject.Parse(jsonCont);
              } else if (Path.GetExtension(jsonPath).ToUpper() == ".BYTES") {
                content = new ConversationFile(jsonPath);
              }
              foreach (var jtproc in partsList.CheckedItems) {
                if (debugCounter > debugCounterMax) { break; }
                jtProcGeneric jtProc = jtproc as jtProcGeneric;
                if (jtProc == null) { continue; }
                prepareRecords.Add(new PrepareRecord(mod, jsonPath, jtProc,
                  langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>(), translationCache, content, locFile, GameBaseSelector.SelectedPath
                  ));
              }
            }
            current_file = string.Empty;
          }
          ProcessForm processForm = new ProcessForm();
          processForm.DeleteOtherTranslations = deleteOtherTranslations;
          processForm.prepareRecords = prepareRecords;
          processForm.progressBar.Maximum = prepareRecords.Count*2;
          processForm.backgroundWorker.RunWorkerAsync();
          processForm.ShowDialog();
          if (deleteOtherTranslations) { locFile.removeOtherTranslations(langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()); }
          locFile.Save(deleteOtherTranslations);
          foreach(PrepareRecord pr in prepareRecords) {
            if (pr.updated) {
              if (Path.GetExtension(pr.jsonPath).ToUpper() == ".JSON") {
                JObject json = pr.content as JObject;
                if (json != null) {
                  if (jsonUpdatedContent.ContainsKey(pr.jsonPath) == false) { jsonUpdatedContent.Add(pr.jsonPath, json.ToString(Formatting.Indented)); }
                };
              } else if (Path.GetExtension(pr.jsonPath).ToUpper() == ".BYTES") {
                ConversationFile cfile = pr.content as ConversationFile;
                if (cfile != null) {
                  if (convUpdatedContent.ContainsKey(pr.jsonPath) == false) { convUpdatedContent.Add(pr.jsonPath, cfile); };
                };
              }
            };
          }
          foreach (var uJsons in jsonUpdatedContent) {
            File.WriteAllText(uJsons.Key, uJsons.Value);
          }
          foreach (var uConv in convUpdatedContent) {
            uConv.Value.Save();
          }
          MessageBox.Show("Compleete");
        } catch (Exception ex) {
          MessageBox.Show("in file:" + current_file + "\n" + ex.ToString());
        }
        string trCache = JsonConvert.SerializeObject(translationCache, Formatting.Indented);
        File.WriteAllText(translationCache_filename, trCache);
      }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      //MessageBox.Show("FormClosed");
      Application.Exit();
      Environment.Exit(0);
      Application.ExitThread();
    }

    private void chAllMods_Click(object sender, EventArgs e) {
      for (int t = 0; t < modsList.Items.Count; ++t) {
        modsList.SetItemCheckState(t, CheckState.Checked);
      }
    }

    private void chAllParts_Click(object sender, EventArgs e) {
      for (int t = 0; t < partsList.Items.Count; ++t) {
        partsList.SetItemCheckState(t, CheckState.Checked);
      }
    }
  }
  public class PrepareRecord {
    public ModRecord mod;
    public string jsonPath;
    public jtProcGeneric jtProc;
    public HashSet<Localize.Strings.Culture> cultures;
    public Dictionary<string, string> locCache;
    public object content;
    public bool updated;
    public LocalizationFile locFile;
    public string basePath;
    public PrepareRecord(ModRecord mod, string jsonPath, jtProcGeneric jtProc, List<Localize.Strings.Culture> cultures, Dictionary<string, string> locCache, object content, LocalizationFile locFile, string basePath) {
      this.mod = mod;
      this.jsonPath = jsonPath;
      this.jtProc = jtProc;
      this.cultures = new HashSet<Localize.Strings.Culture>();
      foreach(Localize.Strings.Culture culture in cultures) { this.cultures.Add(culture); };
      this.locCache = locCache;
      this.content = content;
      this.updated = false;
      this.locFile = locFile;
      this.basePath = basePath;
    }
    public void process() {
      if (jtProc == null) { return; }
      string modName = MainForm.Normilize(mod.Name);
      string filename = MainForm.Normilize(Path.GetFileNameWithoutExtension(jsonPath));
      Dictionary<string, string> replaced = new Dictionary<string, string>();
      jtProc.proc(modName, filename, ref content, replaced, false);
      foreach (var replacements in replaced) {
        //if (debugCounter > debugCounterMax) { break; }
        if (string.IsNullOrEmpty(replacements.Value) == false) {
          CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
          nTr.FileName = jsonPath.Substring(basePath.Length);
          nTr.Name = replacements.Key;
          nTr.Original = replacements.Value;
          //nTr.Localization.Add(Localize.Strings.Culture.CULTURE_EN_US, replacements.Value);
          foreach (Localize.Strings.Culture locLang in cultures) {
            string val = replacements.Value;
            string nval = MainForm.Normilize(val);
            if (locLang == Localize.Strings.Culture.CULTURE_RU_RU) {
              if (val.Length > 30) {
                if (locCache.ContainsKey(nval) == false) {
                  //statistics.Add(val);
                  //MessageBox.Show(val);
                  val = GoogleTranslate.Translate(val);
                  locCache.Add(nval, val);
                  System.Threading.Thread.Sleep(1000);
                  Log.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
                  //MessageBox.Show(val);
                  //++debugCounter;
                } else {
                  val = locCache[nval];
                }
              }
            }
            nTr.Localization.Add(locLang, val);
          }
          locFile.Merge(nTr);
          updated = true;
        } else {
          if (locFile.map.ContainsKey(replacements.Key)) {
            string original = locFile.map[replacements.Key].Original;
            if (string.IsNullOrEmpty(original)) {
              original = locFile.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US];
            }
            if (string.IsNullOrEmpty(original) == false) {
              CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
              nTr.FileName = jsonPath.Substring(basePath.Length); ;
              nTr.Name = replacements.Key;
              nTr.Original = original;
              foreach (Localize.Strings.Culture locLang in cultures) {
                if (locFile.map[replacements.Key].Localization.ContainsKey(locLang) == false) {
                  string val = original;
                  string nval = MainForm.Normilize(val);
                  //statistics.Add(val);
                  if (locLang == Localize.Strings.Culture.CULTURE_RU_RU) {
                    if (val.Length > 30) {
                      if (locCache.ContainsKey(nval) == false) {
                        //statistics.Add(val);
                        //MessageBox.Show(val);
                        val = GoogleTranslate.Translate(val);
                        locCache.Add(nval, val);
                        System.Threading.Thread.Sleep(1000);
                        Log.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
                        //MessageBox.Show(val);
                        //++debugCounter;
                      } else {
                        val = locCache[nval];
                      }
                    }
                  }
                  nTr.Localization.Add(locLang, val);
                }
              }
              locFile.Merge(nTr);
            }
          }
        }
      }
    }
  }
}
