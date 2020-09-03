namespace CustomLocalizationPrepare {
  partial class LangSelectForm {
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
      this.comboBox = new System.Windows.Forms.ComboBox();
      this.button = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // comboBox
      // 
      this.comboBox.FormattingEnabled = true;
      this.comboBox.Location = new System.Drawing.Point(5, 12);
      this.comboBox.Name = "comboBox";
      this.comboBox.Size = new System.Drawing.Size(437, 21);
      this.comboBox.TabIndex = 0;
      // 
      // button
      // 
      this.button.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button.Location = new System.Drawing.Point(367, 39);
      this.button.Name = "button";
      this.button.Size = new System.Drawing.Size(75, 23);
      this.button.TabIndex = 1;
      this.button.Text = "Ok";
      this.button.UseVisualStyleBackColor = true;
      this.button.Click += new System.EventHandler(this.button1_Click);
      // 
      // LangSelectForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(449, 67);
      this.Controls.Add(this.button);
      this.Controls.Add(this.comboBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "LangSelectForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Select Language";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LangSelectForm_FormClosed);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBox;
    private System.Windows.Forms.Button button;
  }
}