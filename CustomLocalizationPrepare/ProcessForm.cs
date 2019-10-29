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
    public List<PrepareRecord> prepareRecords;
    public bool DeleteOtherTranslations;
    public ProcessForm() {
      InitializeComponent();
    }

    private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
      int counter = 0;
      foreach (PrepareRecord prepareRecord in prepareRecords) {
        prepareRecord.process();
        ++counter;
        backgroundWorker.ReportProgress(counter);
      };
      Dictionary<string, string> jsonUpdatedContent = new Dictionary<string, string>();
      Dictionary<string, ConversationFile> convUpdatedContent = new Dictionary<string, ConversationFile>();
      foreach (PrepareRecord prepareRecord in prepareRecords) {
        if (prepareRecord.updated) {
          if (Path.GetExtension(prepareRecord.jsonPath).ToUpper() == ".JSON") {
            JObject json = prepareRecord.content as JObject;
            if (json != null) { jsonUpdatedContent.Add(prepareRecord.jsonPath, json.ToString(Formatting.Indented)); };
          } else if (Path.GetExtension(prepareRecord.jsonPath).ToUpper() == ".BYTES") {
            ConversationFile cfile = prepareRecord.content as ConversationFile;
            if (cfile != null) { convUpdatedContent.Add(prepareRecord.jsonPath, cfile); };
          }
        };
        ++counter;
        backgroundWorker.ReportProgress(counter);
      }
      foreach (var uJsons in jsonUpdatedContent) {
        File.WriteAllText(uJsons.Key, uJsons.Value);
      }
      foreach (var uConv in convUpdatedContent) {
        uConv.Value.Save();
      }
    }

    private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      this.progressBar.Value = e.ProgressPercentage;
    }

    private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      this.DialogResult = DialogResult.OK;
    }
  }
}
