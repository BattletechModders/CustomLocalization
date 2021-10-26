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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using BattleTech;
using Localize;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using CSVFile;

namespace CustormLocalizationPrepare {
  public partial class MainForm : Form {
    public Dictionary<string, string> translationCache;
    public string translationCache_filename;
    public MainForm() {
      this.translationCache = new Dictionary<string, string>();
      //Core.Settings = new CTSettings();
      //Core.Settings.debugLog = true;
      //Log.BaseDirectory = Path.GetDirectoryName(Application.ExecutablePath);
      //Log.InitLog();
      InitializeComponent();
      //modsList.DisplayMember = "Name";
      //modsList.is
      partsList.DisplayMember = "Name";
      //string translated = Translator.Translate(text, Language.Chinese_Simplified, Language.English); Console.WriteLine(translated);
      //saveFileDialog.FileName = CustomTranslation.Core.LocalizationFileName;
      foreach (var lMethod in Core.localizationMethods) {
        partsList.Items.Add(lMethod.Value);
      }
      tbGamePath.Text = Core.ModsRootDirectory;
      LocalizationLoader.InitIndexes(Core.CurRootDirectory);
      LocalizationLoader.InitRU(Core.CurRootDirectory);
      this.openMods();
      this.updateModsState();
    }
    private void addSubDirectories(TreeNode root, LocalizationIndexDirectory indexDir) {
      foreach (LocalizationIndexDirectory indexChild in indexDir.childs) {
        TreeNode modDir = new TreeNode(indexChild.name);
        modDir.Tag = indexChild;
        root.Nodes.Add(modDir);
        addSubDirectories(modDir, indexChild);
      }
    }
    private void openMods() {
      //if (GameBaseSelector.ShowDialog() == DialogResult.OK) 
      {
        modsList.Nodes.Clear();
        //string modsDir = Path.Combine(GameBaseSelector.SelectedPath, "Mods");
        //string modsDir = "c:\\Games\\steamapps\\common\\BATTLETECH\\RogueTechWorking";
        //if (Directory.Exists(modsDir) == false) { modsDir = GameBaseSelector.SelectedPath; };
        //MessageBox.Show(new BTLocalization.Text("Destroy the {TEAM_TAR.FactionDef.Demonym} Shipment and Escape").ToString());
        //MessageBox.Show(new BTLocalization.Text("fdgfdgfdge {TEAM_TAR.FactionDef.Demonym} Shipment and Escape").ToString());
        //tbGamePath.Text = GameBaseSelector.SelectedPath;
        saveFileDialog.InitialDirectory = Core.CurRootDirectory;
        foreach (LocalizationIndexDirectory indexDir in LocalizationLoader.indexRoot.childs) {
          TreeNode modRoot = new TreeNode(indexDir.name);
          modsList.Nodes.Add(modRoot);
          TreeNode baseDir = new TreeNode(".");
          modRoot.Tag = indexDir;
          baseDir.Tag = indexDir;
          modRoot.Nodes.Add(baseDir);
          addSubDirectories(modRoot, indexDir);
          //modsList.Items.Add(new ModRecord((string)jManifest["Name"], modPath));
        }
        this.translationCache_filename = Path.Combine(Core.ModsRootDirectory, "TranslationCacheRu.json");
        if (File.Exists(translationCache_filename)) {
          translationCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(translationCache_filename));
        }
      }
    }
    public void updateModsState() {
      foreach (TreeNode treeNode in modsList.Nodes) {
        this.updateModsState(treeNode);
      }
    }
    public void updateModsState(TreeNode modNode) {
      LocalizationIndexDirectory mod = modNode.Tag as LocalizationIndexDirectory;
      foreach(TreeNode childNode in modNode.Nodes) {
        updateModsState(childNode);
      }
      if (mod == null) { modNode.ForeColor = Color.DarkGray; return;  }
      ModDirRecordState state = mod.getState();
      switch (state) {
        case ModDirRecordState.None: modNode.ForeColor = Color.DarkRed; break;
        case ModDirRecordState.Complete: modNode.ForeColor = Color.DarkGreen; break;
        case ModDirRecordState.Partial: modNode.ForeColor = Color.DarkOrange; break;
        case ModDirRecordState.Empty: modNode.ForeColor = Color.DarkGray; break;
      }
      if (modNode.Nodes.Count == 0) { return; }
      if (state == ModDirRecordState.Partial) { modNode.ForeColor = Color.DarkOrange; return; }
      foreach (TreeNode childNode in modNode.Nodes) {
        if ((state == ModDirRecordState.Complete) && (childNode.ForeColor != Color.DarkGreen) && (childNode.ForeColor != Color.DarkGray)) { modNode.ForeColor = Color.DarkOrange; return; }
        if ((state == ModDirRecordState.None) && (childNode.ForeColor != Color.DarkRed) && (childNode.ForeColor != Color.DarkGray)) { modNode.ForeColor = Color.DarkOrange; return; }
        if ((state == ModDirRecordState.Empty) && (childNode.ForeColor != Color.DarkGray)) {
          modNode.ForeColor = childNode.ForeColor;
          if (childNode.ForeColor == Color.DarkGreen) { state = ModDirRecordState.Complete; }else
          if (childNode.ForeColor == Color.DarkRed) { state = ModDirRecordState.Empty; }else
          if (childNode.ForeColor == Color.DarkOrange) { state = ModDirRecordState.Partial; return; }
        }
      }
      /*  bool hasNotComplete = false;
        bool hasNotNone = false;
        bool hasNotEmpty = false;
        foreach (TreeNode childNode in modNode.Nodes) {
          if((childNode.ForeColor == Color.DarkRed)||(modNode.ForeColor == Color.DarkOrange)) {
            hasNotComplete = true;
            hasNotEmpty = true;
          }
          if ((childNode.ForeColor == Color.DarkGreen) || (modNode.ForeColor == Color.DarkOrange)) {
            hasNotNone = true;
            hasNotEmpty = true;
          }
        }
        switch (mod.getState()) {
          case ModDirRecordState.None: modNode.ForeColor = hasNotNone? Color.DarkOrange : Color.DarkRed; break;
          case ModDirRecordState.Complete: modNode.ForeColor = hasNotComplete? Color.DarkOrange : Color.DarkGreen; break;
          case ModDirRecordState.Partial: modNode.ForeColor = Color.DarkOrange; break;
          case ModDirRecordState.Empty:
            if (hasNotEmpty) {
              if (hasNotComplete && hasNotNone) { modNode.ForeColor = Color.DarkOrange; } else
              if (hasNotNone) { modNode.ForeColor = Color.DarkGreen; } else
              if (hasNotComplete) { modNode.ForeColor = Color.DarkRed; } else {
                modNode.ForeColor = Color.DarkOrange;
              }
            } else {
              modNode.ForeColor = Color.Gray;
            }
            break;
        }
      }*/
    }

    private void backgroundParse_ProgressChanged(object sender, ProgressChangedEventArgs e) {

    }

    private void backgroundParse_DoWork(object sender, DoWorkEventArgs e) {

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
    //                        Log.M?.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
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
    //                              Log.M?.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
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
    private void CollectNodes(TreeNode root, ref List<LocalizationIndexDirectory> modRecords) {
      LocalizationIndexDirectory rec = root.Tag as LocalizationIndexDirectory;
      if (rec != null) {
        if (root.Checked) {
          modRecords.Add(rec);
        }
      }
      foreach (TreeNode node in root.Nodes) {
        CollectNodes(node, ref modRecords);
      }
    }
    private void PrepareMods1_Click(object sender, EventArgs e) {
      List<LocalizationIndexDirectory> mods = new List<LocalizationIndexDirectory>();
      foreach (TreeNode node in modsList.Nodes) { CollectNodes(node, ref mods); };
      List<LocalizationTask> tasks = new List<LocalizationTask>();
      List<jtProcGenericEx> procs = new List<jtProcGenericEx>();
      foreach (var jtproc in partsList.CheckedItems) {
        jtProcGenericEx jp = jtproc as jtProcGenericEx;
        if (jp == null) { continue; }
        procs.Add(jp);
      }
      int complexity = 0;
      foreach (LocalizationIndexDirectory mod in mods) {
        Log.M?.TWL(0, "ModDirRecord:" + mod.name + ":" + mod.path);
        foreach (var jsonName in mod.files) {
          string jsonPath = Path.Combine(mod.path, jsonName.Key);
          tasks.Add(new LocalizationTask(mod, jsonPath, procs));
          complexity += procs.Count;
          Log.M?.WL(1, "task:" + jsonPath);
        }
      }
      ProcessForm processForm = new ProcessForm();
      if (isReverse.Checked) { processForm.command = ProcessCommand.Reverse; } else { processForm.command = ProcessCommand.Create; }
      //processForm.DeleteOtherTranslations = deleteOtherTranslations;
      processForm.prepareRecords = tasks;
      processForm.progressBar.Maximum = complexity;
      processForm.backgroundWorker.RunWorkerAsync();
      processForm.ShowDialog();
      if (processForm.result != null) {
        if (saveFileDialog.ShowDialog() == DialogResult.OK) {
          DialogResult ret = DialogResult.Yes;
          if (File.Exists(saveFileDialog.FileName)) {
            ret = MessageBox.Show("File already exists. Merge?\nYes - merge\nNo - overwrite\nCancel - do nothing", "file already exists", MessageBoxButtons.YesNoCancel);
          }
          if (ret == DialogResult.Cancel) { return; }
          processForm.result.filename = saveFileDialog.FileName;
          LocalizationDef def = processForm.result;
          processForm.result = null;
          LocalizationLoader.saveFile(ref def, ret == DialogResult.Yes);
          this.updateModsState();
          //File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(processForm.result,Formatting.Indented));
        }
      }
    }
    /*private void PrepareMods_Click(object sender, EventArgs e) {
      saveFileDialog.OverwritePrompt = false;
      ConvFileHelper.isReverse = isReverse.Checked;
      HashSet<string> statistics = new HashSet<string>();
      GoogleTranslate t = new GoogleTranslate();
      int debugCounter = 0;
      int debugCounterMax = 5;
      bool deleteOtherTranslations = false;
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        string current_file = string.Empty;
        try {
          bool fileExists = File.Exists(saveFileDialog.FileName);
          LocalizationFile originalSrcFile = new LocalizationFile(saveFileDialog.FileName);
          ConvFileHelper.reverseFile = originalSrcFile;
          LocalizationFile locFile = null;
          if (File.Exists(originalSrcFile.filename)) {
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(originalSrcFile.filename);
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
              if (saveFileDialog.FileName != originalSrcFile.filename) {
                locFile = new LocalizationFile(saveFileDialog.FileName);
                deleteOtherTranslations = true;
              } else {
                locFile = originalSrcFile;
              }
            }
          } else {
            locFile = originalSrcFile;
          }
          //locFile.DebugLogDump();
          //Application.Exit();
          List<ModDirRecord> mods = new List<ModDirRecord>();
          foreach (TreeNode node in modsList.Nodes) { CollectNodes(node, ref mods); };
          MessageBox.Show("mods:" + mods.Count);
          //modsList.
          //modsList.CheckedItems.OfType<ModRecord>().ToList<ModRecord>();
          Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
          Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
          List<PrepareRecord> prepareRecords = new List<PrepareRecord>();
          foreach (ModDirRecord mod in mods) {
            if (debugCounter > debugCounterMax) { break; }
            List<string> jsonsPath = new List<string>();
            GetAllJsons(mod.path, ref jsonsPath, 0);
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
                  new List<Strings.Culture>() { Core.defaultCulture }, translationCache, content, originalSrcFile, locFile, GameBaseSelector.SelectedPath
                ));
              }
            }
            current_file = string.Empty;
          }
          ProcessForm processForm = new ProcessForm();
          processForm.DeleteOtherTranslations = deleteOtherTranslations;
          //processForm.prepareRecords = prepareRecords;
          //processForm.progressBar.Maximum = prepareRecords.Count * 2;
          processForm.backgroundWorker.RunWorkerAsync();
          processForm.ShowDialog();
          if (deleteOtherTranslations) { locFile.removeOtherTranslations(new List<Strings.Culture>() { Core.defaultCulture }); }
          locFile.Save(deleteOtherTranslations);
          foreach (PrepareRecord pr in prepareRecords) {
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
    }*/

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      //MessageBox.Show("FormClosed");
      Application.Exit();
      Environment.Exit(0);
      Application.ExitThread();
    }

    private void chAllMods_Click(object sender, EventArgs e) {
      foreach (TreeNode node in this.modsList.Nodes) {
        node.Checked = true;
      }
      //for (int t = 0; t < modsList.Items.Count; ++t) {
      //  modsList.SetItemCheckState(t, CheckState.Checked);
      //}
    }

    private void chAllParts_Click(object sender, EventArgs e) {
      for (int t = 0; t < partsList.Items.Count; ++t) {
        partsList.SetItemCheckState(t, CheckState.Checked);
      }
    }

    private void cbGoogleTranslate_CheckedChanged(object sender, EventArgs e) {
      PrepareSettings.UseGoogleTranslate = cbGoogleTranslate.CheckState == CheckState.Checked;
    }

    private void btnToExcel_Click(object sender, EventArgs e) {
      if (OpenJsonDialog.ShowDialog() == DialogResult.OK) {
        var output = new FileInfo(Path.ChangeExtension(OpenJsonDialog.FileName, ".xlsx"));
        ExcelPackage package = null;
        ExcelWorksheet worksheet = null;
        LocalizationFile locFile = new LocalizationFile(OpenJsonDialog.FileName);
        Localize.Strings.Culture locLang = Core.defaultCulture;
        //foreach (Localize.Strings.Culture lang in langsList.CheckedItems.OfType<Localize.Strings.Culture>().ToList<Localize.Strings.Culture>()) { locLang = lang; break; };
        Dictionary<string, int> jsonContent = new Dictionary<string, int>();
        HashSet<int> jsonNewContent = new HashSet<int>();
        int index = 0;
        for (index = 0; index < locFile.content.Count; ++index) {
          jsonContent.Add(locFile.content[index].Name, index);
          jsonNewContent.Add(index);
        }
        if (!output.Exists) {
          package = new ExcelPackage();
          worksheet = package.Workbook.Worksheets.Add("Localization");
          worksheet.Cells[1, 1].Value = "ID";
          worksheet.Cells[1, 2].Value = "Original";
          worksheet.Cells[1, 3].Value = locLang.ToString();
          worksheet.Cells[1, 4].Value = "Commentary";
          index = 2;
        } else {
          package = new ExcelPackage(output);
          worksheet = package.Workbook.Worksheets.First();
          index = 2;
          Dictionary<string, int> excelContent = new Dictionary<string, int>();
          while (worksheet.Cells[index, 1].Value != null) {
            string Name = worksheet.Cells[index, 1].Value as string;
            string Original = worksheet.Cells[index, 2].Value as string;
            string Localization = worksheet.Cells[index, 3].Value as string;
            string Commentary = worksheet.Cells[index, 4].Value as string;
            if (jsonContent.TryGetValue(Name, out int locRecIndex)) {
              jsonNewContent.Remove(locRecIndex);
              var locRec = locFile.content[locRecIndex];
              if (string.IsNullOrEmpty(Localization)) { worksheet.Cells[index, 3].Value = locRec.Localization[locLang]; }
              if (string.IsNullOrEmpty(Commentary)) { worksheet.Cells[index, 4].Value = locRec.Commentary; }
              if (IsSanitizedEqual(Original,locRec.Original) == false) {
                worksheet.Cells[index, 2].Value = locRec.Original;
                worksheet.Row(index).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Row(index).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF0000"));
              }
            }
            excelContent.Add(Name, index);
            ++index;
          };
        }
        MessageBox.Show("New index:" + index);
        foreach (var locRecIndex in jsonNewContent) {
          var locRec = locFile.content[locRecIndex];
          worksheet.Cells[index, 1].Value = locRec.Name;
          worksheet.Cells[index, 2].Value = UnixCR(locRec.Original);
          worksheet.Cells[index, 3].Value = UnixCR(locRec.Localization[locLang]);
          worksheet.Cells[index, 4].Value = locRec.Commentary;
          ++index;
        };
        //var xlFile = FileSystem.
        //var output = new FileInfo(Path.ChangeExtension(OpenJsonDialog.FileName,".xlsx"));
        output.Delete();
        // save our new workbook in the output directory and we are done!
        package.SaveAs(output);
      }
    }
    public static string sanitizestring(string a) {
      return a.Replace("  ", " ").Replace("\r\n","\n");
    }
    public static bool IsSanitizedEqual(string a, string b) {
      return sanitizestring(a) == sanitizestring(b);
    }
    public static string UnixCR(string a) {
      return a.Replace("\r\n", "\n");
    }
    private void btnFromExcel_Click(object sender, EventArgs e) {
      if (OpenXLSXDialog.ShowDialog() == DialogResult.OK) {
        //MessageBox.Show(OpenXLSXDialog.FileName + ":" + package.Workbook.Worksheets.Count);
        OpenJsonDialog.InitialDirectory = Path.GetDirectoryName(OpenXLSXDialog.FileName);
        OpenJsonDialog.FileName = Path.ChangeExtension(OpenXLSXDialog.FileName,".json");
        if (OpenJsonDialog.ShowDialog() == DialogResult.OK) {
          if(Path.GetFileNameWithoutExtension(OpenXLSXDialog.FileName) != Path.GetFileNameWithoutExtension(OpenJsonDialog.FileName)) {
            DialogResult ret = MessageBox.Show("XLSX and JSON file names not match. Proceed anyway?", "File names not match", MessageBoxButtons.YesNo);
            if (ret == DialogResult.No) { return; }
          }
        } else {
          return;
        }
        FileInfo existingFile = new FileInfo(OpenXLSXDialog.FileName);
        ExcelPackage package = new ExcelPackage(existingFile);
        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
        LocalizationDef locFile = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(OpenJsonDialog.FileName));
        locFile.filename = OpenJsonDialog.FileName;
        Dictionary<string, LocalizationRecordDef> locDict = new Dictionary<string, LocalizationRecordDef>();
        foreach (LocalizationRecordDef locRec in locFile.content) {
          if (locDict.ContainsKey(locRec.id)) { continue; }
          locDict.Add(locRec.id, locRec);
        }
        int index = 1;
        HashSet<string> affected = new HashSet<string>();
        foreach(string fn in locFile.files) {
          affected.Add(fn);
        }
        while (worksheet.Cells[index, 1].Value != null) {
          //nTr.FileName = OpenXLSXDialog.FileName;
          string Name = worksheet.Cells[index, 1].Value as string;
          if (string.IsNullOrEmpty(Name)) { continue; };
          string Original = worksheet.Cells[index, 2].Value as string; if (Original == null) { Original = string.Empty; };
          string Content = worksheet.Cells[index, 3].Value as string; if (Content == null) { Content = string.Empty; };
          string Commentary = worksheet.Cells[index, 4].Value as string; if (Commentary == null) { Commentary = string.Empty; };
          string textColor = "#" + (worksheet.Cells[index, 1].Style.Font.Color.Rgb != null?worksheet.Cells[index, 1].Style.Font.Color.Rgb.Substring(2):"000000");
          string backColor = "#" + (worksheet.Cells[index, 1].Style.Fill.BackgroundColor.Rgb != null?worksheet.Cells[index, 1].Style.Fill.BackgroundColor.Rgb.Substring(2):"FFFFFF");
          if(locDict.TryGetValue(Name,out LocalizationRecordDef locRec)) {
            if (IsSanitizedEqual(locRec.original,Original) == false) {
              if (Original.Contains("\n---\n")) { 
                locRec.prevOriginal = Original.Substring(0,Original.IndexOf("\n---\n"));
              } else {
                locRec.prevOriginal = Original;
              }
              locRec.backColor = "#FF0000";
              worksheet.Cells[index, 2].Value = locRec.prevOriginal + "\n---\n" + locRec.original;
              worksheet.Row(index).Style.Fill.PatternType = ExcelFillStyle.Solid;
              worksheet.Row(index).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(locRec.backColor));
            } else {
              locRec.prevOriginal = locRec.original;
              locRec.backColor = backColor;
            }
            locRec.localizatorComment = Commentary;
            locRec.content = Content;
            locRec.textColor = textColor;
          } else {
            locRec = new LocalizationRecordDef();
            locRec.id = Name;
            int pos = Name.IndexOf('.');
            if(pos != -1) {
              locRec.filename = Name.Substring(0, pos);
            } else {
              locRec.filename = Name;
            }
            affected.Add(locRec.filename);
            locRec.original = Original;
            locRec.content = Content;
            string proc = string.Empty;
            if (locRec.id.Contains(".YangsThoughts")) {
              proc = "YangsThoughts";
            } else
            if (locRec.id.Contains(".StockRole")) {
              proc = "StockRole";
            } else
            if (locRec.id.Contains("chassisdef_")
              || locRec.id.Contains("mechdef_")
              || locRec.id.Contains("vehicledef_")
              || locRec.id.Contains("vehiclechassisdef_")
              ) 
            {
              if (locRec.id.Contains("Details")) {
                proc = "Description.Details";
                locRec.id = locRec.filename + "." + proc;
                worksheet.Cells[index, 1].Value = locRec.id;
              }
            }
            if (string.IsNullOrEmpty(proc) == false) {
              locRec.processor = proc;
              locFile.content.Add(locRec);
              locDict.Add(locRec.id, locRec);
            }
          }
          ++index;
        };
        locFile.files = affected.ToList();
        Core.ProcessLocalizationDefinition(locFile);
        LocalizationLoader.saveCSV(locFile, true);
        File.WriteAllText(locFile.filename, JsonConvert.SerializeObject(locFile, Formatting.Indented));
        package.Save();
        updateModsState();
        MessageBox.Show("Success");
      }
    }

    private void modsList_AfterCheck(object sender, TreeViewEventArgs e) {
      foreach (TreeNode node in e.Node.Nodes) {
        node.Checked = e.Node.Checked;
      }
    }

    private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e) {

    }

    private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e) {

    }

    private void MainForm_Load(object sender, EventArgs e) {

    }

    private void reindex_Click(object sender, EventArgs e) {
      ProcessForm processForm = new ProcessForm();
      processForm.command = ProcessCommand.Index;
      //processForm.DeleteOtherTranslations = deleteOtherTranslations;
      processForm.backgroundWorker.RunWorkerAsync();
      processForm.ShowDialog();
      openMods();
      updateModsState();
    }

    private void button1_Click(object sender, EventArgs e) {
      ProcessForm processForm = new ProcessForm();
      processForm.command = ProcessCommand.Update;
      //processForm.DeleteOtherTranslations = deleteOtherTranslations;
      processForm.backgroundWorker.RunWorkerAsync();
      processForm.ShowDialog();
      openMods();
      updateModsState();
    }
  }
  public static class PrepareSettings {
    public static bool UseGoogleTranslate = true;
  }
  public class PrepareRecord {
    public ModDirRecord mod;
    public string jsonPath;
    public jtProcGeneric jtProc;
    public HashSet<Localize.Strings.Culture> cultures;
    public Dictionary<string, string> locCache;
    public object content;
    public bool updated;
    public LocalizationFile locFile;
    public LocalizationFile originalSrc;
    public string basePath;
    public PrepareRecord(ModDirRecord mod, string jsonPath, jtProcGeneric jtProc, List<Localize.Strings.Culture> cultures, Dictionary<string, string> locCache, object content, LocalizationFile originalSrc, LocalizationFile locFile, string basePath) {
      this.mod = mod;
      this.jsonPath = jsonPath;
      this.jtProc = jtProc;
      this.cultures = new HashSet<Localize.Strings.Culture>();
      foreach (Localize.Strings.Culture culture in cultures) { this.cultures.Add(culture); };
      this.locCache = locCache;
      this.content = content;
      this.updated = false;
      this.locFile = locFile;
      this.originalSrc = originalSrc;
      this.basePath = basePath;
    }
    public bool isInVanilla(string str) {
      string locstr = new BTLocalization.Text(str).ToString();
      return locstr != str;
    }
    public void process() {
      if (jtProc == null) { return; }
      string modName = MainForm.Normilize(mod.name);
      string filename = MainForm.Normilize(Path.GetFileNameWithoutExtension(jsonPath));
      Dictionary<string, string> replaced = new Dictionary<string, string>();
      jtProc.proc(modName, filename, ref content, replaced, false, new Func<string, bool>(isInVanilla));
      if (ConvFileHelper.isReverse) { updated = true; }
      foreach (var replacements in replaced) {
        //if (debugCounter > debugCounterMax) { break; }
        if (string.IsNullOrEmpty(replacements.Value) == false) {
          CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
          //nTr.FileName = jsonPath.Substring(basePath.Length);
          nTr.Name = replacements.Key;
          nTr.Original = replacements.Value;
          //nTr.Localization.Add(Localize.Strings.Culture.CULTURE_EN_US, replacements.Value);
          foreach (Localize.Strings.Culture locLang in cultures) {
            string val = replacements.Value;
            string nval = MainForm.Normilize(val);
            if ((locLang == Localize.Strings.Culture.CULTURE_RU_RU) && (PrepareSettings.UseGoogleTranslate)) {
              if (val.Length > 30) {
                if (locCache.ContainsKey(nval) == false) {
                  //statistics.Add(val);
                  //MessageBox.Show(val);
                  val = GoogleTranslate.Translate(val);
                  locCache.Add(nval, val);
                  System.Threading.Thread.Sleep(1000);
                  Log.M?.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
                  //MessageBox.Show(val);
                  //++debugCounter;
                } else {
                  val = locCache[nval];
                }
              }
            }
            nTr.Localization.Add(locLang, val);
          }
          locFile.Merge(nTr, true);
          updated = true;
        } else {
          if (locFile.map.ContainsKey(replacements.Key) || originalSrc.map.ContainsKey(replacements.Key)) {
            string original = String.Empty;
            if (locFile.map.ContainsKey(replacements.Key)) { original = locFile.map[replacements.Key].Original; }
            if (originalSrc.map.ContainsKey(replacements.Key)) { original = originalSrc.map[replacements.Key].Original; }
            if (string.IsNullOrEmpty(original)) {
              if (locFile.map.ContainsKey(replacements.Key)) { original = locFile.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US]; }
            }
            if (string.IsNullOrEmpty(original)) {
              if (originalSrc.map.ContainsKey(replacements.Key)) { original = originalSrc.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US]; }
            }
            string en_original = string.Empty;
            if (locFile.map.ContainsKey(replacements.Key)) {
              if (locFile.map[replacements.Key].Localization.ContainsKey(Localize.Strings.Culture.CULTURE_EN_US)) {
                en_original = locFile.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US];
              }
            }
            if (string.IsNullOrEmpty(en_original)) {
              if (originalSrc.map.ContainsKey(replacements.Key)) {
                if (originalSrc.map[replacements.Key].Localization.ContainsKey(Localize.Strings.Culture.CULTURE_EN_US)) {
                  en_original = originalSrc.map[replacements.Key].Localization[Localize.Strings.Culture.CULTURE_EN_US];
                }
              }
            }
            if (cultures.Contains(Localize.Strings.Culture.CULTURE_EN_US) == false) {
              if (string.IsNullOrEmpty(en_original) == false) {
                if (en_original != original) { original = en_original; }
              }
            }
            if (string.IsNullOrEmpty(original) == false) {
              CustomTranslation.TranslateRecord nTr = new CustomTranslation.TranslateRecord();
              //nTr.FileName = jsonPath.Substring(basePath.Length); ;
              nTr.Name = replacements.Key;
              nTr.Original = original;
              foreach (Localize.Strings.Culture locLang in cultures) {
                if (locFile.map.ContainsKey(replacements.Key)) { if (locFile.map[replacements.Key].Localization.ContainsKey(locLang)) { continue; } }
                if (originalSrc.map.ContainsKey(replacements.Key)) {
                  if (originalSrc.map[replacements.Key].Localization.ContainsKey(locLang)) {
                    nTr.Localization.Add(locLang, originalSrc.map[replacements.Key].Localization[locLang]);
                    continue;
                  }
                }
                string val = original;
                string nval = MainForm.Normilize(val);
                //statistics.Add(val);
                if ((locLang == Localize.Strings.Culture.CULTURE_RU_RU) && (PrepareSettings.UseGoogleTranslate)) {
                  if (val.Length > 30) {
                    if (locCache.ContainsKey(nval) == false) {
                      //statistics.Add(val);
                      //MessageBox.Show(val);
                      val = GoogleTranslate.Translate(val);
                      locCache.Add(nval, val);
                      System.Threading.Thread.Sleep(1000);
                      Log.M?.LogWrite(0, "'" + replacements.Value + "' - '" + val + "'\n", true);
                      //MessageBox.Show(val);
                      //++debugCounter;
                    } else {
                      val = locCache[nval];
                    }
                  }
                }
                nTr.Localization.Add(locLang, val);
              }
              locFile.Merge(nTr, true);
            }
          }
        }
      }
    }
  }
  public static class PathAppendHelper {
    public static string PA(this string path, string a) {
      return Path.Combine(path, a);
    }
  }
  public class LocalizationTask {
    public LocalizationIndexDirectory mod { get; set; }
    public string filename { get; set; }
    public string targetKey { get; set; }
    public List<string> targetFileName { get; set; }
    public List<jtProcGenericEx> procList { get; set; }
    public LocalizationTask(LocalizationIndexDirectory mod, string filename, List<jtProcGenericEx> procList) {
      this.mod = mod;
      this.filename = filename;
      this.procList = new List<jtProcGenericEx>();
      this.procList.AddRange(procList);
      List<string> basePath = Core.PathToList(Core.ModsRootDirectory);
      List<string> filePath = Core.PathToList(Path.GetDirectoryName(filename));
      targetFileName = new List<string>();
      bool diff = false;
      for (int t = 0; t < filePath.Count; ++t) {
        if (t >= basePath.Count) { targetFileName.Add(filePath[t]); continue; }
        if (diff) { targetFileName.Add(filePath[t]); continue; }
        if (basePath[t] != filePath[t]) { diff = true; targetFileName.Add(filePath[t]); continue; }
      }
      targetKey = string.Empty;
      foreach (string d in targetFileName) { targetKey = targetKey.PA(d); };
    }
  }
  public class CSVContent{
    public string id { get; set; }
    public string original { get; set; }
    public string content { get; set; }
    public string comment { get; set; }
    public string textColor { get; set; }
    public string backColor { get; set; }
  }
  public class LocalizationIndexDirectory {
    [JsonIgnore]
    public string path { get; set; }
    public string name { get; set; }
    public Dictionary<string, Dictionary<string, List<string>>> files { get; set; }
    public List<LocalizationIndexDirectory> childs { get; set; }
    public LocalizationIndexDirectory() {
      files = new Dictionary<string, Dictionary<string, List<string>>>();
      childs = new List<LocalizationIndexDirectory>();
    }
    public ModDirRecordState getState() {
      if (files.Count == 0) { return ModDirRecordState.Empty; };
      bool atLeastOne = false;
      bool all = true;
      bool empty = true;
      foreach (var file in this.files) {
        foreach(var proc in file.Value) {
          empty = false;
          foreach(string key in proc.Value) {
            if(Core.stringsTable.TryGetValue(key.ToUpper(), out var localization)) {
              if (localization.ContainsKey(Core.defaultCulture)) {
                atLeastOne = true;
                continue;
              }
            }
            all = false;
          }
        }
      }
      if (empty) { return ModDirRecordState.Empty; }
      if (all) { return ModDirRecordState.Complete; }
      if (atLeastOne) { return ModDirRecordState.Partial; }
      return ModDirRecordState.None;
    }
    public void calcPath() {
      foreach(LocalizationIndexDirectory child in this.childs) {
        child.path = Path.Combine(this.path,child.name);
        child.calcPath();
      }
    }
  }
  public static class LocalizationLoader {
    public static LocalizationIndexDirectory indexRoot = null;
    private static LocalizationIndexDirectory findIndex(LocalizationIndexDirectory index, TargetDef trgDef, int level) {
      if (trgDef.dir.Count >= level) { return index; }
      foreach (LocalizationIndexDirectory child in index.childs) {
        if (child.name == trgDef.dir[level]) { return findIndex(child, trgDef, level + 1); }
      }
      return null;
    }
    private static LocalizationIndexDirectory findIndex(LocalizationIndexDirectory index, LocalizationRecordDef locRec) {
      string filename = locRec.filename + ".json";
      Log.M?.WL(1,"findIndex:"+index.name+":"+filename);
      if (index.files.TryGetValue(filename, out var processors)) {
        Log.M?.WL(2, "file found:"+ locRec.processor);
        if (processors.TryGetValue(locRec.processor, out var keys)) {
          if (keys.Contains(locRec.id)) {
            return index;
          }
        }
      }
      filename = locRec.filename + ".bytes";
      if (index.files.TryGetValue(filename, out processors)) {
        if (processors.TryGetValue(locRec.processor, out var keys)) {
          if (keys.Contains(locRec.id)) {
            return index;
          }
        }
      }
      foreach(var child in index.childs) {
        LocalizationIndexDirectory result = findIndex(child, locRec);
        if (result != null) { return result; }
      }
      return null;
    }
    public static LocalizationIndexDirectory findIndex(this LocalizationRecordDef locRec) {
      return findIndex(indexRoot, locRec);
    }
    public static LocalizationIndexDirectory findIndex(this TargetDef trgDef) {
      return findIndex(indexRoot, trgDef, 0);
    }
    internal class updateTaskInfo {
      public LocalizationIndexDirectory index { get; private set; }
      public HashSet<string> processors { get; private set; }
      public updateTaskInfo(LocalizationIndexDirectory i) {
        index = i; processors = new HashSet<string>();
      }
    }
    public static List<LocalizationTask> gatherUpdateTasks(this LocalizationDef def) {
      Log.M?.TWL(0, "gatherUpdateTasks:" + def.filename);
      List<LocalizationTask> result = new List<LocalizationTask>();
      Dictionary<string, updateTaskInfo> affectedFiles = new Dictionary<string, updateTaskInfo>();
      foreach(LocalizationRecordDef locRec in def.content) {
        LocalizationIndexDirectory index = locRec.findIndex();
        if (index == null) {
          Log.M?.WL(1, "index not found:" + locRec.id+":"+locRec.filename);
          continue;
        }
        string filename = locRec.filename+".json";
        if(index.files.ContainsKey(filename) == false) {
          filename = locRec.filename + ".bytes";
        }
        if (index.files.ContainsKey(filename) == false) { continue; };
        string filepath = Path.Combine(index.path, filename);
        if(affectedFiles.TryGetValue(filepath,out var procs) == false) {
          procs = new updateTaskInfo(index);
          affectedFiles.Add(filepath, procs);
        }
        procs.processors.Add(locRec.processor);
      }
      foreach (TargetDef trgDef in def.directories) {
        LocalizationIndexDirectory index = trgDef.findIndex();
        if (index == null) { continue; }
        foreach(var file in index.files) {
          string filepath = Path.Combine(index.path, file.Key);
          if (affectedFiles.TryGetValue(filepath, out var procs) == false) {
            procs = new updateTaskInfo(index);
            affectedFiles.Add(filepath, procs);
          }
          foreach(string proc in trgDef.processors) {
            procs.processors.Add(proc);
          }
        }
      }
      foreach (var file in affectedFiles) {
        List<jtProcGenericEx> procs = new List<jtProcGenericEx>();
        foreach(var proc in file.Value.processors) {
          if(Core.localizationMethods.TryGetValue(proc,out var procobj)) {
            procs.Add(procobj);
          }
        }
        result.Add(new LocalizationTask(file.Value.index, file.Key, procs));
      }
      return result;
    }
    public static string encodeCSV(this string val) {
      return val.Replace("\n", "\\n").Replace("\t","\\t").Replace("\r","\\r").Replace("\"","\\\"");
    }
    public static void saveCSV(LocalizationDef def, bool merge = true) {
      string csvfile = Path.ChangeExtension(def.filename, ".csv");
      if (merge == false) { File.Delete(csvfile); };
      List<CSVContent> csvContent = new List<CSVContent>();
      CSVSettings csvSettings = new CSVSettings();
      csvSettings.HeaderRowIncluded = false;
      csvSettings.ForceQualifiers = true;
      csvSettings.FieldDelimiter = ';';
      if (File.Exists(csvfile)) {
        using (StreamReader reader = new StreamReader(csvfile,Encoding.UTF8))
        using (CSVFile.CSVReader cr = new CSVFile.CSVReader(reader, csvSettings)) {
          foreach (string[] line in cr) {
            CSVContent row = new CSVContent();
            row.id = line.Length > 0?line[0]:string.Empty;
            row.original = line.Length > 1 ? line[1]:string.Empty;
            row.content = line.Length > 2 ? line[2] : string.Empty;
            row.comment = line.Length > 3 ? line[3] : string.Empty;
            row.textColor = line.Length > 4 ? line[4] : string.Empty;
            row.backColor = line.Length > 5 ? line[5] : string.Empty;
            csvContent.Add(row);
          }
        }
      }
      Dictionary<string, int> csvMap = new Dictionary<string, int>();
      for (int t = 0; t < csvContent.Count; ++t) {
        if (string.IsNullOrEmpty(csvContent[t].id)) { continue; }
        if (csvMap.ContainsKey(csvContent[t].id)) { continue; }
        csvMap.Add(csvContent[t].id, t);
      }
      foreach (LocalizationRecordDef locRec in def.content) {
        CSVContent csvRec = null;
        if (csvMap.TryGetValue(locRec.id, out int csvIndex) == false) {
          csvRec = new CSVContent();
          csvRec.id = locRec.id;
          csvMap.Add(locRec.id, csvContent.Count);
          csvContent.Add(csvRec);
        } else {
          csvRec = csvContent[csvIndex];
        }
        csvRec.content = locRec.content;
        if ((locRec.prevOriginal != locRec.original)&&(string.IsNullOrEmpty(locRec.prevOriginal) == false)) {
          csvRec.original = (locRec.prevOriginal + "\n----\n" + locRec.original).encodeCSV();
        } else {
          csvRec.original = locRec.original.encodeCSV();
        }
        csvRec.textColor = locRec.textColor;
        csvRec.backColor = locRec.backColor;
      }
      using (var writer = new StreamWriter(csvfile,false, Encoding.UTF8)) {
        using (var csv = new CSVFile.CSVWriter(writer, csvSettings)) {
          foreach (CSVContent row in csvContent) {
            List<string> line = new List<string>();
            line.Add(row.id);line.Add(row.original);line.Add(row.content);line.Add(row.comment);line.Add(row.textColor);line.Add(row.backColor);
            csv.WriteLine(line);
          }
        }
      }
    }
    public static void saveXLSX(LocalizationDef def, bool merge = true) {
      string xlsxfile = Path.ChangeExtension(def.filename, ".xlsx");
      if (merge == false) { File.Delete(xlsxfile); };
      var output = new FileInfo(xlsxfile);
      ExcelPackage package = null;
      ExcelWorksheet worksheet = null;
      Localize.Strings.Culture locLang = Core.defaultCulture;
      Dictionary<string, int> xslxContent = new Dictionary<string, int>();
      Dictionary<string, LocalizationRecordDef> locContent = new Dictionary<string, LocalizationRecordDef>();
      foreach(LocalizationRecordDef locRec in def.content) {
        if (locContent.ContainsKey(locRec.id)) { continue; }
        locContent.Add(locRec.id, locRec);
      }
      int xlsxIndex = 1;
      if (!output.Exists) {
        package = new ExcelPackage();
        worksheet = package.Workbook.Worksheets.Add("Localization");
      } else {
        package = new ExcelPackage(output);
        worksheet = package.Workbook.Worksheets.First();
        while (worksheet.Cells[xlsxIndex, 1].Value != null) {
          string Name = worksheet.Cells[xlsxIndex, 1].Value as string;
          string Original = worksheet.Cells[xlsxIndex, 2].Value as string;
          string Localization = worksheet.Cells[xlsxIndex, 3].Value as string;
          string Commentary = worksheet.Cells[xlsxIndex, 4].Value as string;
          if(locContent.TryGetValue(Name,out LocalizationRecordDef locRec)) {
            if (locRec.prevOriginal.IndexOf("\n---\n") >= 0) {
              locRec.prevOriginal = locRec.prevOriginal.Substring(0, locRec.prevOriginal.IndexOf("\n---\n"));
            }
            if(locRec.prevOriginal != locRec.original) {
              worksheet.Cells[xlsxIndex, 2].Value = locRec.prevOriginal + "\n---\n" + locRec.original;
              worksheet.Row(xlsxIndex).Style.Fill.PatternType = ExcelFillStyle.Solid;
              worksheet.Row(xlsxIndex).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF0000"));
              locRec.backColor = "#FF0000";
            } else {
              if(Original != locRec.original) {
                worksheet.Cells[xlsxIndex, 2].Value = locRec.original;
                worksheet.Row(xlsxIndex).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Row(xlsxIndex).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF0000"));
              }
            }
            locContent.Remove(Name);
          }
          ++xlsxIndex;
        };
      }
      //MessageBox.Show("New index:" + index);
      foreach (var locRec in locContent) {
        worksheet.Cells[xlsxIndex, 1].Value = locRec.Value.id;
        worksheet.Cells[xlsxIndex, 2].Value = locRec.Value.original;
        worksheet.Cells[xlsxIndex, 3].Value = locRec.Value.content;
        worksheet.Cells[xlsxIndex, 4].Value = locRec.Value.localizatorComment;
        ++xlsxIndex;
      };
      //var xlFile = FileSystem.
      //var output = new FileInfo(Path.ChangeExtension(OpenJsonDialog.FileName,".xlsx"));
      output.Delete();
      // save our new workbook in the output directory and we are done!
      package.SaveAs(output);
    }
    public static void saveFile(ref LocalizationDef def, bool merge = true) {
      if (merge == false) { File.Delete(def.filename); }
      if (File.Exists(def.filename)) {
        LocalizationDef oldDef = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(def.filename));
        oldDef.filename = def.filename;
        oldDef.Merge(def,false);
        def = oldDef;
      }
      saveCSV(def, merge);
      saveXLSX(def, merge);
      File.WriteAllText(def.filename, JsonConvert.SerializeObject(def, Formatting.Indented));
      Core.ProcessLocalizationDefinition(def);
    }
    public static void InitIndexes(string basePath, bool loadFromFile = true) {
      if ((loadFromFile == false)||(File.Exists(Path.Combine(basePath, "index.json")) == false)) {
        indexRoot = new LocalizationIndexDirectory();
        indexRoot.path = Core.ModsRootDirectory;
        indexRoot.name = "root";
      } else {
        indexRoot = JsonConvert.DeserializeObject<LocalizationIndexDirectory>(File.ReadAllText(Path.Combine(basePath, "index.json")));
        indexRoot.path = Core.ModsRootDirectory;
        indexRoot.calcPath();
      }
    }
    public static void InitRU(string BasePath) {
      CSVReader RU_CSV_reader = new CSVReader(BasePath.PA("StreamingAssets").PA("data").PA("localization").PA("strings_ru-RU.csv"));
      BTLocalization.CSVStringsProvider localizationSource = new BTLocalization.CSVStringsProvider();
      localizationSource.SupportedCultures = new List<Strings.Culture>();
      localizationSource.SupportedCultures.Add(Strings.Culture.CULTURE_RU_RU);
      localizationSource.readers.Add(Strings.Culture.CULTURE_RU_RU, RU_CSV_reader);
      localizationSource.LoadCulture(Strings.Culture.CULTURE_RU_RU);
      //localizationSource.LoadCultureFromReader(RU_CSV_reader);
      BTLocalization.Strings.Initialize(localizationSource, new FontLocalizationManager());
    }
  }
}
