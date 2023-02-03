using CustomTranslation;
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
  public partial class GatherManifestForm : Form {
    public GatherManifestForm() {
      InitializeComponent();
      manifestGatherWorker.RunWorkerAsync();
    }

    private void GatherManifestForm_FormClosed(object sender, FormClosedEventArgs e) {
      Application.Exit();
      System.Environment.Exit(0);
    }
    private void manifestGatherWorker_DoWork(object sender, DoWorkEventArgs e) {
      ManifestHelper.ProcessManifest(Path.Combine(Core.ModsRootDirectory, ".modtek", "Manifest.csv"), (counter, str) => {
        manifestGatherWorker.ReportProgress((int)(counter * 1000f), str);
      });
      ManifestHelper.GatherLocalizationFiles(Core.ModsRootDirectory, (counter, str) => {
        manifestGatherWorker.ReportProgress((int)(counter * 1000f), str);
      });
    }

    private void manifestGatherWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      this.progressLabel.Text = (string)e.UserState;
      this.progressBar.Value = e.ProgressPercentage;
    }

    private void manifestGatherWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      new ManifestForm().Show();
      this.Hide();
    }
  }
}
