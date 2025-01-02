using CustomTranslation;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
  public partial class ManifestForm : Form {
    public ManifestForm() {
      InitializeComponent();
      Log.M?.TWL(0, $"Manifest size:{ManifestHelper.localizable.Count}");
      //manifestPopulate.RunWorkerAsync();
      InitManifest();
      InitFiles();
    }

    private void ManifestForm_FormClosed(object sender, FormClosedEventArgs e) {
      Application.Exit();
      System.Environment.Exit(0);
    }
    private static NodeEntry manifestRootNode = null;
    private static NodeEntry filesRootNode = null;
    public static Dictionary<LocalizationDef, NodeEntry> filesNodeEnteries = new Dictionary<LocalizationDef, NodeEntry>();
    public static Dictionary<string, LocalizationDef> loadedFiles = new Dictionary<string, LocalizationDef>();
    public static Dictionary<LocalizationRecordDef, NodeEntry> locRecordsNodeEnteries = new Dictionary<LocalizationRecordDef, NodeEntry>();
    public void LocRecordToolTip(NodeEntry node) {
      if (node.node != null) {
        if (node.locRecord == null) {
          node.node.ToolTipText = $"Записей:{node.allCount}\nОбработано:{node.processedCount}\nОригинальный текст изменен:{node.errorCount}";
          if (node.errorCount != 0) {
            node.node.ForeColor = Color.Red;
          } else
          if (node.allCount > node.processedCount) {
            node.node.ForeColor = Color.DarkOrange;
          } else {
            node.node.ForeColor = Color.ForestGreen;
          }
        }
      }
      foreach (var child in node.childs) {
        LocRecordToolTip(child.Value);
      }
    }
    public void RecalculateLocalizationStatistic(NodeEntry node) {
      node.allCount = 0;
      node.processedCount = 0;
      node.errorCount = 0;
      if (node.locRecord != null) {
        if (node.namechanged) {
          //node.node.Text = $"{node.locRecord.id}:{node.locRecord.GetShortValue()}";
          node.namechanged = false;
        }
        node.add_allCounter();
        if (node.locRecord.parent != null) {
          node.add_processedCount();
          if (node.locRecord.original == node.locRecord.prevOriginal) {
            node.node.ForeColor = Color.ForestGreen;
          } else {
            node.node.ForeColor = Color.Red;
            node.add_errorCounter();
          }
        } else {
          node.node.ForeColor = Color.Black;
        }
      }
      foreach(var child in node.childs) {
        RecalculateLocalizationStatistic(child.Value);
      }
      LocRecordToolTip(node);
    }
    public void FileRecordToolTip(NodeEntry node) {
      if (node.node != null) {
        if (node.fileRecord != null) {
          node.node.ToolTipText = $"Записей:{node.fileRecord.content.Count}\nИзменено:{node.fileRecord.diffcounter}";
        }
      }
      foreach (var child in node.childs) {
        FileRecordToolTip(child.Value);
      }
    }
    public void InitManifest() {
      manifestRootNode = new NodeEntry(null, this.twManifest);
      int counter = 0;
      foreach (var locentry in ManifestHelper.localizable) {
        string path = locentry.Value.filename;
        List<string> nodes = new List<string>();
        while ((string.IsNullOrEmpty(path) == false) && (path != Core.ModsRootDirectory)) {
          string nodename = Path.GetFileName(path);
          path = Path.GetDirectoryName(path);
          nodes.Add(nodename);
        }
        nodes.Reverse();
        nodes.Add($"{locentry.Key}:{locentry.Value.GetShortValue()}");
        NodeEntry node = manifestRootNode;
        foreach (string nodename in nodes) {
          node = node.GetOrCreate(nodename, (collection, addnode) => {
            collection.Add(addnode);
            //manifestPopulate.ReportProgress(counter, new AddNodeRequest(collection, addnode));
          });
        }
        ++counter;
        node.add_allCounter();
        locRecordsNodeEnteries[locentry.Value] = node;
        if (locentry.Value.parent != null) {
          node.add_processedCount();
          node.node.ContextMenuStrip = this.manifestContextMenu;
          if (locentry.Value.original == locentry.Value.prevOriginal) {
            node.node.ForeColor = Color.ForestGreen;
          } else {
            node.node.ForeColor = Color.Red;
            node.add_errorCounter();
          }
        }
        if (node != null) node.locRecord = locentry.Value;
      }
      this.twManifest.Sort();
      this.LocRecordToolTip(manifestRootNode);
    }
    public NodeEntry addFile(LocalizationDef locfile) {
      string path = locfile.filename;
      List<string> nodes = new List<string>();
      while ((string.IsNullOrEmpty(path) == false) && (path != Core.ModsRootDirectory)) {
        string nodename = Path.GetFileName(path);
        path = Path.GetDirectoryName(path);
        nodes.Add(nodename);
      }
      nodes.Reverse();
      NodeEntry node = filesRootNode;
      foreach (string nodename in nodes) {
        node = node.GetOrCreate(nodename, (collection, addnode) => {
          collection.Add(addnode);
        });
        node.node.Expand();
      }
      if (node != null) {
        node.fileRecord = locfile;
        filesNodeEnteries[locfile] = node;
        loadedFiles[locfile.filename] = locfile;
        node.node.ContextMenuStrip = this.filesContextMenu;
      }
      if (locfile.diffcounter == 0) {
        node.node.ForeColor = Color.ForestGreen;
      } else {
        node.node.ForeColor = Color.Red;
      }
      return node;
    }
    public void InitFiles() {
      filesRootNode = new NodeEntry(null, this.twFiles);
      foreach (var locfile in ManifestHelper.localizationFiles) {
        this.addFile(locfile);
      }
      this.twManifest.Sort();
      FileRecordToolTip(filesRootNode);
    }
    // Updates all child tree nodes recursively.
    private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked) {
      foreach (TreeNode node in treeNode.Nodes) {
        node.Checked = nodeChecked;
        if (node.Nodes.Count > 0) {
          // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
          this.CheckAllChildNodes(node, nodeChecked);
        }
      }
    }

    // NOTE   This code can be added to the BeforeCheck event handler instead of the AfterCheck event.
    // After a tree node's Checked property is changed, all its child nodes are updated to the same value.
    private void node_AfterCheck(object sender, TreeViewEventArgs e) {
      // The code only executes if the user caused the checked state to change.
      if (e.Action != TreeViewAction.Unknown) {
        if (e.Node.Nodes.Count > 0) {
          /* Calls the CheckAllChildNodes method, passing in the current 
          Checked value of the TreeNode whose checked state changed. */
          this.CheckAllChildNodes(e.Node, e.Node.Checked);
        }
      }
    }

    private void manifestPopulate_DoWork(object sender, DoWorkEventArgs e) {
      this.InitManifest();
    }

    private void manifestPopulate_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      (e.UserState as AddNodeRequest).nodes.Add((e.UserState as AddNodeRequest).node);
    }

    private void twManifest_AfterSelect(object sender, TreeViewEventArgs e) {
      NodeEntry entry = e.Node.Tag as NodeEntry;
      if (entry == null) { this.localizationRecordToolTipLabel.Text = ""; return; }
      this.localizationRecordToolTipLabel.Text = $"Записей:{entry.allCount} Обработано:{entry.processedCount} Изменено:{entry.errorCount}";
    }

    private void twFiles_AfterSelect(object sender, TreeViewEventArgs e) {
      NodeEntry entry = e.Node.Tag as NodeEntry;
      if (entry == null) { this.fileStatusToolTipLabel.Text = "no node"; return; }
      if (entry.fileRecord == null) { this.fileStatusToolTipLabel.Text = "no file"; return; }
      this.fileStatusToolTipLabel.Text = $"Записей:{entry.fileRecord.content.Count} Изменено:{entry.fileRecord.diffcounter}";
    }

    private void twManifest_MouseDoubleClick(object sender, MouseEventArgs e) {

    }

    private void twManifest_DoubleClick(object sender, EventArgs e) {

    }

    private void twManifest_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
      NodeEntry entry = e.Node.Tag as NodeEntry;
      Console.WriteLine("Node double click");
      if (entry == null) { return; }
      if (entry.locRecord == null) { return; }
      if (entry.locRecord.parent == null) { return; }
      if (filesNodeEnteries.TryGetValue(entry.locRecord.parent, out var locFileNode)) {
        Console.WriteLine("localization file found");
        if (locFileNode.node == null) { return; }
        Console.WriteLine("node not null");
        locFileNode.node.EnsureVisible();
        twFiles.SelectedNode = locFileNode.node;
        this.tabControlMain.SelectedIndex = 1;
      }
    }

    private void exportXLSXToolStripMenuItem_Click(object sender, EventArgs e) {
      if (twFiles.SelectedNode == null) { return; }
      NodeEntry node = twFiles.SelectedNode.Tag as NodeEntry;
      node.ExportToXLSX();
    }

    private void twFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
      (sender as TreeView).SelectedNode = e.Node;
    }

    private void importXLSXToolStripMenuItem_Click(object sender, EventArgs e) {
      if (twFiles.SelectedNode == null) { return; }
      NodeEntry node = twFiles.SelectedNode.Tag as NodeEntry;
      if (node.ImportXLSX()) {
        this.RecalculateLocalizationStatistic(manifestRootNode);
      }
    }

    public void populateselected(NodeEntry node, List<NodeEntry> nodes, ref string possible_file) {
      if (node.node != null) {
        if (node.node.Checked) {
          if (node.locRecord != null) {
            if (node.locRecord.parent == null) {
              nodes.Add(node);
            }else if (string.IsNullOrEmpty(possible_file)) {
              possible_file = node.locRecord.parent.filename;
            }
          }
        }
      };
      foreach (var child in node.childs) { populateselected(child.Value, nodes, ref possible_file); }
    }

    private void btn_exportManifest_Click(object sender, EventArgs e) {
      List<NodeEntry> selected = new List<NodeEntry>();
      string possible_file = string.Empty;
      populateselected(manifestRootNode, selected, ref possible_file);
      if (selected.Count == 0) {
        MessageBox.Show("Ничего не выбрано, или выбранные строки уже включены в какой-либо локализационный файл");
        return;
      }
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      if (string.IsNullOrEmpty(possible_file)) {
        saveFileDialog.InitialDirectory = Path.GetDirectoryName(Log.BaseDirectory);
      } else {
        saveFileDialog.InitialDirectory = Path.GetDirectoryName(possible_file);
      }
      saveFileDialog.Filter = "LocalizationDef.json|LocalizationDef.json";
      saveFileDialog.FilterIndex = 1;
      saveFileDialog.RestoreDirectory = true;
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        LocalizationDef saveFile = null;
        LocalizationDef memoryFile = null;
        if (File.Exists(saveFileDialog.FileName)) {
          saveFile = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(saveFileDialog.FileName));
        } else {
          saveFile = new LocalizationDef(saveFileDialog.FileName, Core.defaultCulture);
        }
        saveFile.filename = saveFileDialog.FileName;
        saveFile.reindex();
        if(loadedFiles.TryGetValue(saveFileDialog.FileName, out memoryFile) == false) {
          memoryFile = new LocalizationDef(saveFileDialog.FileName, Core.defaultCulture);
        }
        foreach(var node in selected) {
          node.locRecord.parent = memoryFile;
          memoryFile.content.Add(node.locRecord);
          var saverecord = new LocalizationRecordDef();
          saverecord.id = node.locRecord.id;
          saverecord.filename = Path.GetFileNameWithoutExtension(node.locRecord.filename);
          saverecord.processor = node.locRecord.processor;
          saverecord.original = node.locRecord.original;
          saverecord.prevOriginal = node.locRecord.original;
          saverecord.content = node.locRecord.content;
          saveFile.content.Add(saverecord);
        }
        File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(saveFile, Formatting.Indented));
        if (filesNodeEnteries.TryGetValue(memoryFile, out var filenode) == false) {
          filenode = this.addFile(memoryFile);
        }
        this.FileRecordToolTip(filenode);
      }
    }

    private void twManifest_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
      (sender as TreeView).SelectedNode = e.Node;
    }

    private void locrecIdToClipboard_Click(object sender, EventArgs e) {
      if (twManifest.SelectedNode == null) { return; }
      NodeEntry node = twManifest.SelectedNode.Tag as NodeEntry;
      if (node.locRecord == null) { return; }
      Clipboard.SetText(node.locRecord.id, TextDataFormat.Text);
    }

    private void checkdirsToolStripMenuItem_Click(object sender, EventArgs e) {
      if (twFiles.SelectedNode == null) { return; }
      NodeEntry node = twFiles.SelectedNode.Tag as NodeEntry;
    }
  }
  public class AddNodeRequest {
    public TreeNodeCollection nodes;
    public TreeNode node;
    public AddNodeRequest(TreeNodeCollection nodes, TreeNode node) {
      this.node = node;
      this.nodes = nodes;
    }
  }
  public class NodeEntry {
    public TreeNode node { get; set; } = null;
    public TreeView treeview { get; set; } = null;
    public NodeEntry parent { get; set; } = null;
    public LocalizationRecordDef locRecord { get; set; } = null;
    public LocalizationDef fileRecord { get; set; } = null;
    public Dictionary<string, NodeEntry> childs { get; set; } = new Dictionary<string, NodeEntry>();
    public int errorCount { get; set; } = 0;
    public int allCount { get; set; } = 0;
    public int processedCount { get; set; } = 0;
    public bool namechanged { get; set; } = false;
    private bool f_edited = false;
    public bool edited { get { return f_edited; }
      set {
        if (value == true) {
          if (f_edited == false) {
            if(locRecord != null) {
              node.Text = $"*{this.locRecord.id}:{this.locRecord.GetShortValue()}";
              if (locRecord.parent != null) {
                if (ManifestForm.filesNodeEnteries.TryGetValue(locRecord.parent, out var fileNode)) {
                  fileNode.edited = true;
                }
              }
            } else if(fileRecord != null) {
              node.Text = $"*{Path.GetFileName(this.fileRecord.filename)}";
            }
          }
        } else {
          if (f_edited == true) {
            if (locRecord != null) {
              node.Text = $"{this.locRecord.id}:{this.locRecord.GetShortValue()}";
            } else if (fileRecord != null) {
              node.Text = $"{Path.GetFileName(this.fileRecord.filename)}";
            }
          }
        }
      }
    }
    public NodeEntry(TreeNode node, TreeView treeview) {
      this.node = node;
      this.treeview = treeview;
    }
    public void add_allCounter() {
      this.allCount += 1;
      NodeEntry pr = this.parent;
      while(pr != null) { pr.allCount += 1; pr = pr.parent; }
    }
    public void add_errorCounter() {
      this.errorCount += 1;
      NodeEntry pr = this.parent;
      while (pr != null) { pr.errorCount += 1; pr = pr.parent; }
    }
    public void add_processedCount() {
      this.errorCount += 1;
      NodeEntry pr = this.parent;
      while (pr != null) { pr.processedCount += 1; pr = pr.parent; }
    }
    public NodeEntry GetOrCreate(string nodename, Action<TreeNodeCollection,TreeNode> addNodeCallback) {
      if(childs.TryGetValue(nodename, out var child) == false) {
        TreeNode childnode = new TreeNode(nodename);
        child = new NodeEntry(childnode, null);
        this.childs.Add(nodename, child);
        child.parent = this;
        childnode.Checked = false;
        childnode.Tag = child;
        if (this.node != null) { addNodeCallback(this.node.Nodes, childnode); }else
        if (this.treeview != null) { addNodeCallback(this.treeview.Nodes, childnode); }
        //if (this.node != null) { this.node.Nodes.Add(childnode); }else
        //if (this.treeview != null) { this.treeview.Nodes.Add(childnode); }
      }
      return child;
    }
    public void ExportToXLSX() {
      if (this.fileRecord == null) { return; }
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.InitialDirectory = Path.GetDirectoryName(fileRecord.filename);
      saveFileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx";
      saveFileDialog.FilterIndex = 1;
      saveFileDialog.RestoreDirectory = true;
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        Dictionary<string, int> existing_index = new Dictionary<string, int>();
        FileInfo existingFile = new FileInfo(saveFileDialog.FileName);
        ExcelPackage package = null;
        ExcelWorksheet worksheet = null;
        if (existingFile.Exists) {
          package = new ExcelPackage(existingFile);
          worksheet = package.Workbook.Worksheets.First();
        } else {
          package = new ExcelPackage();
          worksheet = package.Workbook.Worksheets.Add("Localization");
        }
        int lines_count = 1;
        int index = 1;
        this.fileRecord.reindex();
        while (worksheet.Cells[index, 1].Value != null) {
          string id = worksheet.Cells[index, 1].Value as string;
          if (string.IsNullOrEmpty(id)) { continue; }
          if (this.fileRecord.index.ContainsKey(id)) { existing_index[id] = index; }
          ++index;
        }
        foreach (var locRec in this.fileRecord.content) {
          int row_id = index;
          if (existing_index.TryGetValue(locRec.id, out row_id) == false) {
            row_id = index;
            ++index;
          }
          worksheet.Cells[row_id, 1].Value = locRec.id;
          worksheet.Cells[row_id, 2].Value = JsonConvert.ToString(locRec.original);
          worksheet.Cells[row_id, 3].Value = JsonConvert.ToString(locRec.content);
          if(locRec.original != locRec.prevOriginal) {
            worksheet.Cells[row_id, 5].Value = "Оригинальное значение обновлено. Возможно, более не соответствует переводу. После устранения несоответствия удалите значения ЭТОЙ ячейки и импортируйте данный XLSX файл. Так вы дадите программе знать, что перевод вновь соотвествует оригиналу.";
            worksheet.Row(row_id).Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Row(row_id).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF0000"));
          } else {
            worksheet.Cells[row_id, 5].Value = "";
          }
        }
        if (existingFile.Exists) {
          package.Save();
        } else {
          package.SaveAs(existingFile);
        }
        MessageBox.Show("Успешный успех");
      }
    }
    public bool ImportXLSX() {
      if (this.fileRecord == null) { return false; }
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.InitialDirectory = Path.GetDirectoryName(fileRecord.filename);
      openFileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx";
      openFileDialog.FilterIndex = 1;
      openFileDialog.RestoreDirectory = true;
      if (openFileDialog.ShowDialog() == DialogResult.OK) {
        Dictionary<string, int> existing_index = new Dictionary<string, int>();
        FileInfo existingFile = new FileInfo(openFileDialog.FileName);
        if (existingFile.Exists == false) { return false; }
        var package = new ExcelPackage(existingFile);
        var worksheet = package.Workbook.Worksheets.First();
        int index = 1;
        LocalizationDef saveFile = JsonConvert.DeserializeObject<LocalizationDef>(File.ReadAllText(this.fileRecord.filename));
        this.fileRecord.reindex();
        saveFile.reindex();
        while (worksheet.Cells[index, 1].Value != null) {
          string id = worksheet.Cells[index, 1].Value as string;
          if (string.IsNullOrEmpty(id)) { continue; }
          if (saveFile.index.ContainsKey(id)) { existing_index[id] = index; }
          ++index;
        }
        bool format_error = false;
        int updated = 0;
        foreach (var locRec in saveFile.content) {
          if(existing_index.TryGetValue(locRec.id, out var row_id) == false) { continue; }
          if (this.fileRecord.index.TryGetValue(locRec.id, out var exlocRec) == false) { continue; }
          string content = string.Empty;
          try {
            content = JsonConvert.DeserializeObject<string>(worksheet.Cells[row_id, 3].Value as string);
          } catch (Exception e) {
            format_error = true;
            MessageBox.Show($"Ошибка в строке {row_id} в столбце C. Значение не соотвествует формату JSON {e.ToString()}. Обновление прервано");
            break;
          }
          bool is_updated = false;
          if (content != locRec.content) {
            is_updated = true;
            locRec.content = content;
          }
          if(locRec.original != exlocRec.original) {
            string error = worksheet.Cells[row_id, 5].Value as string;
            if (string.IsNullOrWhiteSpace(error)) {
              if (is_updated == false) { is_updated = true; }
              locRec.prevOriginal = locRec.original;
              locRec.original = exlocRec.original;
            }
          }
          if (is_updated) { ++updated; }
        }
        if(format_error == false) {
          File.WriteAllText(this.fileRecord.filename, JsonConvert.SerializeObject(saveFile, Formatting.Indented));
          foreach (var locRec in saveFile.content) {
            if (this.fileRecord.index.TryGetValue(locRec.id, out var exlocRec) == false) { continue; }
            exlocRec.prevOriginal = locRec.original;
            if(exlocRec.content != locRec.content) {
              exlocRec.content = locRec.content;
              if (ManifestForm.locRecordsNodeEnteries.TryGetValue(exlocRec, out var node)) {
                node.namechanged = true;
                node.node.Text = node.locRecord.id + ":" + node.locRecord.GetShortValue();
              }
            }
          }
          MessageBox.Show($"Обновлено записей {updated}");
        }
        return !format_error;
      }
      return false;
    }
  }
}
