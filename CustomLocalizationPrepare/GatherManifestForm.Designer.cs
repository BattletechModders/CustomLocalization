namespace CustomLocalizationPrepare {
  partial class GatherManifestForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.progressLabel = new System.Windows.Forms.Label();
      this.manifestGatherWorker = new System.ComponentModel.BackgroundWorker();
      this.SuspendLayout();
      // 
      // progressBar
      // 
      this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.Location = new System.Drawing.Point(1, 30);
      this.progressBar.Maximum = 1000;
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(429, 23);
      this.progressBar.TabIndex = 0;
      // 
      // progressLabel
      // 
      this.progressLabel.Location = new System.Drawing.Point(-2, 9);
      this.progressLabel.Name = "progressLabel";
      this.progressLabel.Size = new System.Drawing.Size(432, 13);
      this.progressLabel.TabIndex = 1;
      this.progressLabel.Text = "Обработка манифеста ...";
      // 
      // manifestGatherWorker
      // 
      this.manifestGatherWorker.WorkerReportsProgress = true;
      this.manifestGatherWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.manifestGatherWorker_DoWork);
      this.manifestGatherWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.manifestGatherWorker_ProgressChanged);
      this.manifestGatherWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.manifestGatherWorker_RunWorkerCompleted);
      // 
      // GatherManifestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(431, 54);
      this.Controls.Add(this.progressLabel);
      this.Controls.Add(this.progressBar);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "GatherManifestForm";
      this.Text = "Обработка манифеста";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GatherManifestForm_FormClosed);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label progressLabel;
    private System.ComponentModel.BackgroundWorker manifestGatherWorker;
  }
}