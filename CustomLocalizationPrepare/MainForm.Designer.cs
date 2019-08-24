namespace CustormLocalizationPrepare {
  partial class MainForm {
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
      this.tbGamePath = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.openMods = new System.Windows.Forms.Button();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.modsList = new System.Windows.Forms.CheckedListBox();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.partsList = new System.Windows.Forms.CheckedListBox();
      this.langsList = new System.Windows.Forms.CheckedListBox();
      this.PrepareMods = new System.Windows.Forms.Button();
      this.GameBaseSelector = new System.Windows.Forms.FolderBrowserDialog();
      this.backgroundParse = new System.ComponentModel.BackgroundWorker();
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbGamePath
      // 
      this.tbGamePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbGamePath.Location = new System.Drawing.Point(2, 19);
      this.tbGamePath.Name = "tbGamePath";
      this.tbGamePath.ReadOnly = true;
      this.tbGamePath.Size = new System.Drawing.Size(576, 20);
      this.tbGamePath.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(-1, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(63, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Game\'s root";
      // 
      // openMods
      // 
      this.openMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.openMods.Location = new System.Drawing.Point(584, 17);
      this.openMods.Name = "openMods";
      this.openMods.Size = new System.Drawing.Size(21, 23);
      this.openMods.TabIndex = 2;
      this.openMods.Text = ">";
      this.openMods.UseVisualStyleBackColor = true;
      this.openMods.Click += new System.EventHandler(this.openMods_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer1.Location = new System.Drawing.Point(2, 45);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.modsList);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(603, 402);
      this.splitContainer1.SplitterDistance = 201;
      this.splitContainer1.TabIndex = 9;
      // 
      // modsList
      // 
      this.modsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.modsList.FormattingEnabled = true;
      this.modsList.Location = new System.Drawing.Point(3, 3);
      this.modsList.Name = "modsList";
      this.modsList.Size = new System.Drawing.Size(195, 394);
      this.modsList.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.partsList);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.langsList);
      this.splitContainer2.Size = new System.Drawing.Size(398, 402);
      this.splitContainer2.SplitterDistance = 200;
      this.splitContainer2.TabIndex = 0;
      // 
      // partsList
      // 
      this.partsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.partsList.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.partsList.FormattingEnabled = true;
      this.partsList.Location = new System.Drawing.Point(3, 3);
      this.partsList.Name = "partsList";
      this.partsList.Size = new System.Drawing.Size(194, 394);
      this.partsList.TabIndex = 0;
      // 
      // langsList
      // 
      this.langsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.langsList.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.langsList.FormattingEnabled = true;
      this.langsList.Location = new System.Drawing.Point(3, 2);
      this.langsList.Name = "langsList";
      this.langsList.Size = new System.Drawing.Size(188, 394);
      this.langsList.TabIndex = 0;
      // 
      // PrepareMods
      // 
      this.PrepareMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.PrepareMods.Location = new System.Drawing.Point(472, 453);
      this.PrepareMods.Name = "PrepareMods";
      this.PrepareMods.Size = new System.Drawing.Size(133, 23);
      this.PrepareMods.TabIndex = 11;
      this.PrepareMods.Text = "Prepare selected mods";
      this.PrepareMods.UseVisualStyleBackColor = true;
      this.PrepareMods.Click += new System.EventHandler(this.PrepareMods_Click);
      // 
      // GameBaseSelector
      // 
      this.GameBaseSelector.Description = "Select game\'s directory";
      this.GameBaseSelector.RootFolder = System.Environment.SpecialFolder.MyComputer;
      this.GameBaseSelector.ShowNewFolderButton = false;
      // 
      // backgroundParse
      // 
      this.backgroundParse.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundParse_DoWork);
      this.backgroundParse.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundParse_ProgressChanged);
      // 
      // saveFileDialog
      // 
      this.saveFileDialog.DefaultExt = "json";
      this.saveFileDialog.FileName = "Locallization.json";
      this.saveFileDialog.Filter = "Json file|*.json";
      this.saveFileDialog.Title = "Save localization";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(608, 483);
      this.Controls.Add(this.PrepareMods);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.openMods);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tbGamePath);
      this.Name = "MainForm";
      this.Text = "Main";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox tbGamePath;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button openMods;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.CheckedListBox modsList;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.CheckedListBox partsList;
    private System.Windows.Forms.CheckedListBox langsList;
    private System.Windows.Forms.Button PrepareMods;
    private System.Windows.Forms.FolderBrowserDialog GameBaseSelector;
    private System.ComponentModel.BackgroundWorker backgroundParse;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
  }
}

