using CustomTranslation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomLocalizationPrepare {
  public partial class LangSelectForm : Form {
    public static string AssemblyDirectory {
      get {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }
    public LangSelectForm() {
      //string[] arguments = Environment.GetCommandLineArgs();
      //MessageBox.Show(arguments[1]);
      CustomTranslation.Core.InitStandalone(AssemblyDirectory, Program.ModsFolder, AssemblyDirectory);
      InitializeComponent();
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_RU_RU);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_DE_DE);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_ZH_CN);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_ES_ES);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_FR_FR);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_IT_IT);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_PT_PT);
      comboBox.Items.Add(Localize.Strings.Culture.CULTURE_PT_BR);
      comboBox.SelectedIndex = 0;
    }

    private void button1_Click(object sender, EventArgs e) {
      Core.defaultCulture = (Localize.Strings.Culture)comboBox.SelectedItem;
      this.Hide();
      (new CustomLocalizationPrepare.GatherManifestForm()).Show();
    }

    private void LangSelectForm_FormClosed(object sender, FormClosedEventArgs e) {
      Application.Exit();
      System.Environment.Exit(0);
    }
  }
}
