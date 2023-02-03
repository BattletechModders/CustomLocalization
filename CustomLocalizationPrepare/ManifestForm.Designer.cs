namespace CustomLocalizationPrepare {
  partial class ManifestForm {
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
      this.components = new System.ComponentModel.Container();
      this.tabControlMain = new System.Windows.Forms.TabControl();
      this.ManifestTab = new System.Windows.Forms.TabPage();
      this.manifestStatusStrip = new System.Windows.Forms.StatusStrip();
      this.localizationRecordToolTipLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.btn_exportManifest = new System.Windows.Forms.Button();
      this.twManifest = new System.Windows.Forms.TreeView();
      this.tabPageFiles = new System.Windows.Forms.TabPage();
      this.fileStatusStrip = new System.Windows.Forms.StatusStrip();
      this.fileStatusToolTipLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.twFiles = new System.Windows.Forms.TreeView();
      this.manifestPopulate = new System.ComponentModel.BackgroundWorker();
      this.filesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.exportXLSXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.importXLSXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.manifestContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.locrecIdToClipboard = new System.Windows.Forms.ToolStripMenuItem();
      this.dirsCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.tabControlMain.SuspendLayout();
      this.ManifestTab.SuspendLayout();
      this.manifestStatusStrip.SuspendLayout();
      this.tabPageFiles.SuspendLayout();
      this.fileStatusStrip.SuspendLayout();
      this.filesContextMenu.SuspendLayout();
      this.manifestContextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlMain
      // 
      this.tabControlMain.Controls.Add(this.ManifestTab);
      this.tabControlMain.Controls.Add(this.tabPageFiles);
      this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControlMain.Location = new System.Drawing.Point(0, 0);
      this.tabControlMain.Name = "tabControlMain";
      this.tabControlMain.SelectedIndex = 0;
      this.tabControlMain.Size = new System.Drawing.Size(522, 724);
      this.tabControlMain.TabIndex = 0;
      // 
      // ManifestTab
      // 
      this.ManifestTab.Controls.Add(this.manifestStatusStrip);
      this.ManifestTab.Controls.Add(this.btn_exportManifest);
      this.ManifestTab.Controls.Add(this.twManifest);
      this.ManifestTab.Location = new System.Drawing.Point(4, 22);
      this.ManifestTab.Name = "ManifestTab";
      this.ManifestTab.Padding = new System.Windows.Forms.Padding(3);
      this.ManifestTab.Size = new System.Drawing.Size(514, 698);
      this.ManifestTab.TabIndex = 0;
      this.ManifestTab.Text = "Манифест";
      this.ManifestTab.UseVisualStyleBackColor = true;
      // 
      // manifestStatusStrip
      // 
      this.manifestStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localizationRecordToolTipLabel});
      this.manifestStatusStrip.Location = new System.Drawing.Point(3, 673);
      this.manifestStatusStrip.Name = "manifestStatusStrip";
      this.manifestStatusStrip.Size = new System.Drawing.Size(508, 22);
      this.manifestStatusStrip.TabIndex = 3;
      this.manifestStatusStrip.Text = "manifestStatusStrip";
      // 
      // localizationRecordToolTipLabel
      // 
      this.localizationRecordToolTipLabel.Name = "localizationRecordToolTipLabel";
      this.localizationRecordToolTipLabel.Size = new System.Drawing.Size(0, 17);
      // 
      // btn_exportManifest
      // 
      this.btn_exportManifest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_exportManifest.Location = new System.Drawing.Point(294, 647);
      this.btn_exportManifest.Name = "btn_exportManifest";
      this.btn_exportManifest.Size = new System.Drawing.Size(220, 23);
      this.btn_exportManifest.TabIndex = 2;
      this.btn_exportManifest.Text = "Экспорт выбраного в JSON файл";
      this.btn_exportManifest.UseVisualStyleBackColor = true;
      this.btn_exportManifest.Click += new System.EventHandler(this.btn_exportManifest_Click);
      // 
      // twManifest
      // 
      this.twManifest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.twManifest.CheckBoxes = true;
      this.twManifest.FullRowSelect = true;
      this.twManifest.Location = new System.Drawing.Point(0, 0);
      this.twManifest.Name = "twManifest";
      this.twManifest.ShowNodeToolTips = true;
      this.twManifest.Size = new System.Drawing.Size(518, 641);
      this.twManifest.TabIndex = 1;
      this.twManifest.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.node_AfterCheck);
      this.twManifest.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.twManifest_AfterSelect);
      this.twManifest.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.twManifest_NodeMouseClick);
      this.twManifest.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.twManifest_NodeMouseDoubleClick);
      this.twManifest.DoubleClick += new System.EventHandler(this.twManifest_DoubleClick);
      this.twManifest.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.twManifest_MouseDoubleClick);
      // 
      // tabPageFiles
      // 
      this.tabPageFiles.Controls.Add(this.fileStatusStrip);
      this.tabPageFiles.Controls.Add(this.twFiles);
      this.tabPageFiles.Location = new System.Drawing.Point(4, 22);
      this.tabPageFiles.Name = "tabPageFiles";
      this.tabPageFiles.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFiles.Size = new System.Drawing.Size(514, 698);
      this.tabPageFiles.TabIndex = 1;
      this.tabPageFiles.Text = "Файлы";
      this.tabPageFiles.UseVisualStyleBackColor = true;
      // 
      // fileStatusStrip
      // 
      this.fileStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileStatusToolTipLabel});
      this.fileStatusStrip.Location = new System.Drawing.Point(3, 673);
      this.fileStatusStrip.Name = "fileStatusStrip";
      this.fileStatusStrip.Size = new System.Drawing.Size(508, 22);
      this.fileStatusStrip.TabIndex = 1;
      this.fileStatusStrip.Text = "statusStrip1";
      // 
      // fileStatusToolTipLabel
      // 
      this.fileStatusToolTipLabel.Name = "fileStatusToolTipLabel";
      this.fileStatusToolTipLabel.Size = new System.Drawing.Size(0, 17);
      // 
      // twFiles
      // 
      this.twFiles.Dock = System.Windows.Forms.DockStyle.Top;
      this.twFiles.Location = new System.Drawing.Point(3, 3);
      this.twFiles.Name = "twFiles";
      this.twFiles.Size = new System.Drawing.Size(508, 667);
      this.twFiles.TabIndex = 0;
      this.twFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.twFiles_AfterSelect);
      this.twFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.twFiles_NodeMouseClick);
      // 
      // manifestPopulate
      // 
      this.manifestPopulate.WorkerReportsProgress = true;
      this.manifestPopulate.DoWork += new System.ComponentModel.DoWorkEventHandler(this.manifestPopulate_DoWork);
      this.manifestPopulate.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.manifestPopulate_ProgressChanged);
      // 
      // filesContextMenu
      // 
      this.filesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportXLSXToolStripMenuItem,
            this.importXLSXToolStripMenuItem,
            this.dirsCheckToolStripMenuItem});
      this.filesContextMenu.Name = "filesContextMenu";
      this.filesContextMenu.Size = new System.Drawing.Size(195, 92);
      // 
      // exportXLSXToolStripMenuItem
      // 
      this.exportXLSXToolStripMenuItem.Name = "exportXLSXToolStripMenuItem";
      this.exportXLSXToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
      this.exportXLSXToolStripMenuItem.Text = "Экспорт в XLSX";
      this.exportXLSXToolStripMenuItem.Click += new System.EventHandler(this.exportXLSXToolStripMenuItem_Click);
      // 
      // importXLSXToolStripMenuItem
      // 
      this.importXLSXToolStripMenuItem.Name = "importXLSXToolStripMenuItem";
      this.importXLSXToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
      this.importXLSXToolStripMenuItem.Text = "Импорт из XLSX";
      this.importXLSXToolStripMenuItem.Click += new System.EventHandler(this.importXLSXToolStripMenuItem_Click);
      // 
      // manifestContextMenu
      // 
      this.manifestContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.locrecIdToClipboard});
      this.manifestContextMenu.Name = "manifestContextMenu";
      this.manifestContextMenu.Size = new System.Drawing.Size(255, 26);
      // 
      // locrecIdToClipboard
      // 
      this.locrecIdToClipboard.Name = "locrecIdToClipboard";
      this.locrecIdToClipboard.Size = new System.Drawing.Size(254, 22);
      this.locrecIdToClipboard.Text = "Идентификатор->Буфер обмена";
      this.locrecIdToClipboard.Click += new System.EventHandler(this.locrecIdToClipboard_Click);
      // 
      // dirsCheckToolStripMenuItem
      // 
      this.dirsCheckToolStripMenuItem.Name = "dirsCheckToolStripMenuItem";
      this.dirsCheckToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
      this.dirsCheckToolStripMenuItem.Text = "Отметить директории";
      this.dirsCheckToolStripMenuItem.Click += new System.EventHandler(this.checkdirsToolStripMenuItem_Click);
      // 
      // ManifestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(522, 724);
      this.Controls.Add(this.tabControlMain);
      this.Name = "ManifestForm";
      this.Text = "Локализация";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ManifestForm_FormClosed);
      this.tabControlMain.ResumeLayout(false);
      this.ManifestTab.ResumeLayout(false);
      this.ManifestTab.PerformLayout();
      this.manifestStatusStrip.ResumeLayout(false);
      this.manifestStatusStrip.PerformLayout();
      this.tabPageFiles.ResumeLayout(false);
      this.tabPageFiles.PerformLayout();
      this.fileStatusStrip.ResumeLayout(false);
      this.fileStatusStrip.PerformLayout();
      this.filesContextMenu.ResumeLayout(false);
      this.manifestContextMenu.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControlMain;
    private System.Windows.Forms.TabPage ManifestTab;
    private System.Windows.Forms.Button btn_exportManifest;
    private System.Windows.Forms.TreeView twManifest;
    private System.ComponentModel.BackgroundWorker manifestPopulate;
    private System.Windows.Forms.TabPage tabPageFiles;
    private System.Windows.Forms.TreeView twFiles;
    private System.Windows.Forms.StatusStrip manifestStatusStrip;
    private System.Windows.Forms.ToolStripStatusLabel localizationRecordToolTipLabel;
    private System.Windows.Forms.StatusStrip fileStatusStrip;
    private System.Windows.Forms.ToolStripStatusLabel fileStatusToolTipLabel;
    private System.Windows.Forms.ContextMenuStrip filesContextMenu;
    private System.Windows.Forms.ToolStripMenuItem exportXLSXToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem importXLSXToolStripMenuItem;
    private System.Windows.Forms.ContextMenuStrip manifestContextMenu;
    private System.Windows.Forms.ToolStripMenuItem locrecIdToClipboard;
    private System.Windows.Forms.ToolStripMenuItem dirsCheckToolStripMenuItem;
  }
}