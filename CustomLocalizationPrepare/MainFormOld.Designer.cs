namespace CustormLocalizationPrepare {
  partial class MainFormOld {
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.modsList = new System.Windows.Forms.TreeView();
      this.chAllMods = new System.Windows.Forms.Button();
      this.chAllParts = new System.Windows.Forms.Button();
      this.partsList = new System.Windows.Forms.CheckedListBox();
      this.PrepareMods = new System.Windows.Forms.Button();
      this.GameBaseSelector = new System.Windows.Forms.FolderBrowserDialog();
      this.backgroundParse = new System.ComponentModel.BackgroundWorker();
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.cbGoogleTranslate = new System.Windows.Forms.CheckBox();
      this.btnFromExcel = new System.Windows.Forms.Button();
      this.OpenJsonDialog = new System.Windows.Forms.OpenFileDialog();
      this.OpenXLSXDialog = new System.Windows.Forms.OpenFileDialog();
      this.isReverse = new System.Windows.Forms.CheckBox();
      this.btnToExcel = new System.Windows.Forms.Button();
      this.reindex = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbGamePath
      // 
      this.tbGamePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbGamePath.Location = new System.Drawing.Point(2, 19);
      this.tbGamePath.Name = "tbGamePath";
      this.tbGamePath.ReadOnly = true;
      this.tbGamePath.Size = new System.Drawing.Size(732, 20);
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
      this.splitContainer1.Panel1.Controls.Add(this.chAllMods);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.chAllParts);
      this.splitContainer1.Panel2.Controls.Add(this.partsList);
      this.splitContainer1.Size = new System.Drawing.Size(732, 541);
      this.splitContainer1.SplitterDistance = 244;
      this.splitContainer1.TabIndex = 9;
      // 
      // modsList
      // 
      this.modsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.modsList.CheckBoxes = true;
      this.modsList.Location = new System.Drawing.Point(3, 3);
      this.modsList.Name = "modsList";
      this.modsList.Size = new System.Drawing.Size(238, 503);
      this.modsList.TabIndex = 2;
      this.modsList.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.modsList_AfterCheck);
      // 
      // chAllMods
      // 
      this.chAllMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.chAllMods.Location = new System.Drawing.Point(3, 512);
      this.chAllMods.Name = "chAllMods";
      this.chAllMods.Size = new System.Drawing.Size(236, 23);
      this.chAllMods.TabIndex = 1;
      this.chAllMods.Text = "check all";
      this.chAllMods.UseVisualStyleBackColor = true;
      this.chAllMods.Click += new System.EventHandler(this.chAllMods_Click);
      // 
      // chAllParts
      // 
      this.chAllParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.chAllParts.Location = new System.Drawing.Point(3, 512);
      this.chAllParts.Name = "chAllParts";
      this.chAllParts.Size = new System.Drawing.Size(478, 23);
      this.chAllParts.TabIndex = 3;
      this.chAllParts.Text = "check all";
      this.chAllParts.UseVisualStyleBackColor = true;
      this.chAllParts.Click += new System.EventHandler(this.chAllParts_Click);
      // 
      // partsList
      // 
      this.partsList.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.partsList.FormattingEnabled = true;
      this.partsList.Location = new System.Drawing.Point(3, 3);
      this.partsList.Name = "partsList";
      this.partsList.Size = new System.Drawing.Size(478, 499);
      this.partsList.TabIndex = 2;
      // 
      // PrepareMods
      // 
      this.PrepareMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.PrepareMods.Location = new System.Drawing.Point(618, 592);
      this.PrepareMods.Name = "PrepareMods";
      this.PrepareMods.Size = new System.Drawing.Size(116, 23);
      this.PrepareMods.TabIndex = 11;
      this.PrepareMods.Text = "Create new definition";
      this.PrepareMods.UseVisualStyleBackColor = true;
      this.PrepareMods.Click += new System.EventHandler(this.PrepareMods1_Click);
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
      this.saveFileDialog.FileName = "LocalizationDef.json";
      this.saveFileDialog.Filter = "Json file|*.json";
      this.saveFileDialog.OverwritePrompt = false;
      this.saveFileDialog.Title = "Save localization";
      // 
      // cbGoogleTranslate
      // 
      this.cbGoogleTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cbGoogleTranslate.AutoSize = true;
      this.cbGoogleTranslate.Location = new System.Drawing.Point(5, 597);
      this.cbGoogleTranslate.Name = "cbGoogleTranslate";
      this.cbGoogleTranslate.Size = new System.Drawing.Size(107, 17);
      this.cbGoogleTranslate.TabIndex = 12;
      this.cbGoogleTranslate.Text = "Google Translate";
      this.cbGoogleTranslate.UseVisualStyleBackColor = true;
      this.cbGoogleTranslate.CheckedChanged += new System.EventHandler(this.cbGoogleTranslate_CheckedChanged);
      // 
      // btnFromExcel
      // 
      this.btnFromExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnFromExcel.Location = new System.Drawing.Point(233, 593);
      this.btnFromExcel.Name = "btnFromExcel";
      this.btnFromExcel.Size = new System.Drawing.Size(90, 23);
      this.btnFromExcel.TabIndex = 14;
      this.btnFromExcel.Text = "Merge XLSX";
      this.btnFromExcel.UseVisualStyleBackColor = true;
      this.btnFromExcel.Click += new System.EventHandler(this.btnFromExcel_Click);
      // 
      // OpenJsonDialog
      // 
      this.OpenJsonDialog.DefaultExt = "*.json";
      this.OpenJsonDialog.FileName = "Localization.json";
      this.OpenJsonDialog.Filter = "JSON|*.json";
      // 
      // OpenXLSXDialog
      // 
      this.OpenXLSXDialog.DefaultExt = "*.xlsx";
      this.OpenXLSXDialog.FileName = "Localization.xlsx";
      this.OpenXLSXDialog.Filter = "XLSX|*.xlsx";
      // 
      // isReverse
      // 
      this.isReverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.isReverse.AutoSize = true;
      this.isReverse.Location = new System.Drawing.Point(539, 597);
      this.isReverse.Name = "isReverse";
      this.isReverse.Size = new System.Drawing.Size(73, 17);
      this.isReverse.TabIndex = 15;
      this.isReverse.Text = "isReverse";
      this.isReverse.UseVisualStyleBackColor = true;
      // 
      // btnToExcel
      // 
      this.btnToExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnToExcel.Location = new System.Drawing.Point(118, 593);
      this.btnToExcel.Name = "btnToExcel";
      this.btnToExcel.Size = new System.Drawing.Size(109, 23);
      this.btnToExcel.TabIndex = 13;
      this.btnToExcel.Text = "old JSON To XLSX";
      this.btnToExcel.UseVisualStyleBackColor = true;
      this.btnToExcel.Click += new System.EventHandler(this.btnToExcel_Click);
      // 
      // reindex
      // 
      this.reindex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.reindex.Location = new System.Drawing.Point(329, 593);
      this.reindex.Name = "reindex";
      this.reindex.Size = new System.Drawing.Size(83, 23);
      this.reindex.TabIndex = 16;
      this.reindex.Text = "Update index";
      this.reindex.UseVisualStyleBackColor = true;
      this.reindex.Click += new System.EventHandler(this.reindex_Click);
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button1.Location = new System.Drawing.Point(418, 593);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(115, 23);
      this.button1.TabIndex = 17;
      this.button1.Text = "Update localization";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(737, 622);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.reindex);
      this.Controls.Add(this.isReverse);
      this.Controls.Add(this.btnFromExcel);
      this.Controls.Add(this.btnToExcel);
      this.Controls.Add(this.cbGoogleTranslate);
      this.Controls.Add(this.PrepareMods);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tbGamePath);
      this.Name = "MainForm";
      this.Text = "Main";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox tbGamePath;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Button PrepareMods;
    private System.Windows.Forms.FolderBrowserDialog GameBaseSelector;
    private System.ComponentModel.BackgroundWorker backgroundParse;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
    private System.Windows.Forms.Button chAllMods;
    private System.Windows.Forms.CheckBox cbGoogleTranslate;
    private System.Windows.Forms.Button btnFromExcel;
    private System.Windows.Forms.OpenFileDialog OpenJsonDialog;
    private System.Windows.Forms.OpenFileDialog OpenXLSXDialog;
    private System.Windows.Forms.TreeView modsList;
    private System.Windows.Forms.CheckBox isReverse;
    private System.Windows.Forms.Button chAllParts;
    private System.Windows.Forms.CheckedListBox partsList;
    private System.Windows.Forms.Button btnToExcel;
    private System.Windows.Forms.Button reindex;
    private System.Windows.Forms.Button button1;
  }
}

