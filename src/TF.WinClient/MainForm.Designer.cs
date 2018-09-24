using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TF.WinClient
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            this.StatusBar = new StatusStrip();
            this.UsedCharLabel = new ToolStripStatusLabel();
            this.MainMenuBar = new MenuStrip();
            this.FileMenuItem = new ToolStripMenuItem();
            this.FileNewMenuItem = new ToolStripMenuItem();
            this.FileOpenMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.FileSaveMenuItem = new ToolStripMenuItem();
            this.FileExportMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.FileExitMenuItem = new ToolStripMenuItem();
            this.ToolsMenuItem = new ToolStripMenuItem();
            this.ImportToolMenuItem = new ToolStripMenuItem();
            this.ImportTFMenuItem = new ToolStripMenuItem();
            this.ImportExcelMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.SearchToolMenuItem = new ToolStripMenuItem();
            this.SearchNextMenuItem = new ToolStripMenuItem();
            this.OpenProjectFileDialog = new OpenFileDialog();
            this.ImportFileDialog = new OpenFileDialog();
            this.SaveProjectFileDialog = new SaveFileDialog();
            this.ExportProjectFolderBrowserDialog = new FolderBrowserDialog();
            this.StringsDataGrid = new TFDataGridView();
            this.colID = new DataGridViewTextBoxColumn();
            this.colFile = new DataGridViewTextBoxColumn();
            this.colGroup = new DataGridViewTextBoxColumn();
            this.colOffset = new DataGridViewTextBoxColumn();
            this.colOriginal = new DataGridViewTextBoxColumn();
            this.colTranslation = new DataGridViewTextBoxColumn();
            this.StatusBar.SuspendLayout();
            this.MainMenuBar.SuspendLayout();
            ((ISupportInitialize)(this.StringsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // StatusBar
            // 
            this.StatusBar.Items.AddRange(new ToolStripItem[] {
            this.UsedCharLabel});
            this.StatusBar.Location = new Point(0, 604);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new Size(1088, 22);
            this.StatusBar.TabIndex = 0;
            // 
            // UsedCharLabel
            // 
            this.UsedCharLabel.Name = "UsedCharLabel";
            this.UsedCharLabel.Size = new Size(0, 17);
            // 
            // MainMenuBar
            // 
            this.MainMenuBar.Items.AddRange(new ToolStripItem[] {
            this.FileMenuItem,
            this.ToolsMenuItem});
            this.MainMenuBar.Location = new Point(0, 0);
            this.MainMenuBar.Name = "MainMenuBar";
            this.MainMenuBar.Size = new Size(1088, 24);
            this.MainMenuBar.TabIndex = 1;
            this.MainMenuBar.Text = "menuStrip1";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.FileNewMenuItem,
            this.FileOpenMenuItem,
            this.toolStripSeparator1,
            this.FileSaveMenuItem,
            this.FileExportMenuItem,
            this.toolStripSeparator2,
            this.FileExitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new Size(60, 20);
            this.FileMenuItem.Text = "&Archivo";
            // 
            // FileNewMenuItem
            // 
            this.FileNewMenuItem.Image = ((Image)(resources.GetObject("FileNewMenuItem.Image")));
            this.FileNewMenuItem.Name = "FileNewMenuItem";
            this.FileNewMenuItem.Size = new Size(180, 22);
            this.FileNewMenuItem.Text = "&Nueva traducción";
            this.FileNewMenuItem.Click += new EventHandler(this.FileNewMenuItem_Click);
            // 
            // FileOpenMenuItem
            // 
            this.FileOpenMenuItem.Image = ((Image)(resources.GetObject("FileOpenMenuItem.Image")));
            this.FileOpenMenuItem.Name = "FileOpenMenuItem";
            this.FileOpenMenuItem.Size = new Size(180, 22);
            this.FileOpenMenuItem.Text = "&Abrir traducción";
            this.FileOpenMenuItem.Click += new EventHandler(this.FileOpenMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(177, 6);
            // 
            // FileSaveMenuItem
            // 
            this.FileSaveMenuItem.Image = ((Image)(resources.GetObject("FileSaveMenuItem.Image")));
            this.FileSaveMenuItem.Name = "FileSaveMenuItem";
            this.FileSaveMenuItem.Size = new Size(180, 22);
            this.FileSaveMenuItem.Text = "&Guardar";
            this.FileSaveMenuItem.Click += new EventHandler(this.FileSaveMenuItem_Click);
            // 
            // FileExportMenuItem
            // 
            this.FileExportMenuItem.Image = ((Image)(resources.GetObject("FileExportMenuItem.Image")));
            this.FileExportMenuItem.Name = "FileExportMenuItem";
            this.FileExportMenuItem.Size = new Size(180, 22);
            this.FileExportMenuItem.Text = "Exportar...";
            this.FileExportMenuItem.Click += new EventHandler(this.FileExportMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(177, 6);
            // 
            // FileExitMenuItem
            // 
            this.FileExitMenuItem.Name = "FileExitMenuItem";
            this.FileExitMenuItem.Size = new Size(180, 22);
            this.FileExitMenuItem.Text = "Salir";
            this.FileExitMenuItem.Click += new EventHandler(this.FileExitMenuItem_Click);
            // 
            // ToolsMenuItem
            // 
            this.ToolsMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.ImportToolMenuItem,
            this.toolStripSeparator3,
            this.SearchToolMenuItem,
            this.SearchNextMenuItem});
            this.ToolsMenuItem.Name = "ToolsMenuItem";
            this.ToolsMenuItem.Size = new Size(90, 20);
            this.ToolsMenuItem.Text = "Herramientas";
            // 
            // ImportToolMenuItem
            // 
            this.ImportToolMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.ImportTFMenuItem,
            this.ImportExcelMenuItem});
            this.ImportToolMenuItem.Name = "ImportToolMenuItem";
            this.ImportToolMenuItem.Size = new Size(180, 22);
            this.ImportToolMenuItem.Text = "Importar traduccion";
            // 
            // ImportTFMenuItem
            // 
            this.ImportTFMenuItem.Name = "ImportTFMenuItem";
            this.ImportTFMenuItem.Size = new Size(135, 22);
            this.ImportTFMenuItem.Text = "Desde tf_*";
            this.ImportTFMenuItem.Click += new EventHandler(this.ImportTFMenuItem_Click);
            // 
            // ImportExcelMenuItem
            // 
            this.ImportExcelMenuItem.Name = "ImportExcelMenuItem";
            this.ImportExcelMenuItem.Size = new Size(135, 22);
            this.ImportExcelMenuItem.Text = "Desde Excel";
            this.ImportExcelMenuItem.Click += new EventHandler(this.ImportExcelMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(177, 6);
            // 
            // SearchToolMenuItem
            // 
            this.SearchToolMenuItem.Name = "SearchToolMenuItem";
            this.SearchToolMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.F)));
            this.SearchToolMenuItem.Size = new Size(180, 22);
            this.SearchToolMenuItem.Text = "Buscar...";
            this.SearchToolMenuItem.Click += new EventHandler(this.SearchToolMenuItem_Click);
            // 
            // SearchNextMenuItem
            // 
            this.SearchNextMenuItem.Name = "SearchNextMenuItem";
            this.SearchNextMenuItem.ShortcutKeys = Keys.F3;
            this.SearchNextMenuItem.Size = new Size(180, 22);
            this.SearchNextMenuItem.Text = "Buscar siguiente";
            this.SearchNextMenuItem.Click += new EventHandler(this.SearchNextMenuItem_Click);
            // 
            // ImportFileDialog
            // 
            this.ImportFileDialog.Multiselect = true;
            // 
            // ExportProjectFolderBrowserDialog
            // 
            this.ExportProjectFolderBrowserDialog.Description = "Selecciona la carpeta donde guardar el fichero.\\r\\nSe guardará con el mismo nombr" +
    "e que tenía el fichero original.";
            this.ExportProjectFolderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            // 
            // StringsDataGrid
            // 
            this.StringsDataGrid.AllowUserToAddRows = false;
            this.StringsDataGrid.AllowUserToDeleteRows = false;
            this.StringsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.StringsDataGrid.Columns.AddRange(new DataGridViewColumn[] {
            this.colID,
            this.colFile,
            this.colGroup,
            this.colOffset,
            this.colOriginal,
            this.colTranslation});
            this.StringsDataGrid.Dock = DockStyle.Fill;
            this.StringsDataGrid.Location = new Point(0, 24);
            this.StringsDataGrid.MultiSelect = false;
            this.StringsDataGrid.Name = "StringsDataGrid";
            this.StringsDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.StringsDataGrid.Size = new Size(1088, 580);
            this.StringsDataGrid.TabIndex = 2;
            this.StringsDataGrid.CellEndEdit += new DataGridViewCellEventHandler(this.StringsDataGrid_CellEndEdit);
            this.StringsDataGrid.CellMouseDoubleClick += new DataGridViewCellMouseEventHandler(this.StringsDataGrid_CellMouseDoubleClick);
            this.StringsDataGrid.CellPainting += new DataGridViewCellPaintingEventHandler(this.StringsDataGrid_CellPainting);
            this.StringsDataGrid.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(this.StringsDataGrid_EditingControlShowing);
            // 
            // colID
            // 
            this.colID.HeaderText = "ID";
            this.colID.Name = "colID";
            this.colID.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.colID.Visible = false;
            // 
            // colFile
            // 
            this.colFile.HeaderText = "Fichero";
            this.colFile.Name = "colFile";
            this.colFile.ReadOnly = true;
            // 
            // colGroup
            // 
            this.colGroup.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colGroup.DefaultCellStyle = dataGridViewCellStyle1;
            this.colGroup.FillWeight = 25F;
            this.colGroup.HeaderText = "Grupo";
            this.colGroup.Name = "colGroup";
            this.colGroup.ReadOnly = true;
            this.colGroup.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colOffset
            // 
            this.colOffset.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colOffset.DefaultCellStyle = dataGridViewCellStyle2;
            this.colOffset.FillWeight = 15F;
            this.colOffset.HeaderText = "Offset";
            this.colOffset.Name = "colOffset";
            this.colOffset.ReadOnly = true;
            this.colOffset.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colOriginal
            // 
            this.colOriginal.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colOriginal.DefaultCellStyle = dataGridViewCellStyle3;
            this.colOriginal.HeaderText = "Original";
            this.colOriginal.Name = "colOriginal";
            this.colOriginal.ReadOnly = true;
            this.colOriginal.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colTranslation
            // 
            this.colTranslation.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colTranslation.HeaderText = "Traducción";
            this.colTranslation.Name = "colTranslation";
            this.colTranslation.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1088, 626);
            this.Controls.Add(this.StringsDataGrid);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.MainMenuBar);
            this.MainMenuStrip = this.MainMenuBar;
            this.Name = "MainForm";
            this.Text = "Translation Framework";
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.MainMenuBar.ResumeLayout(false);
            this.MainMenuBar.PerformLayout();
            ((ISupportInitialize)(this.StringsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private StatusStrip StatusBar;
        private MenuStrip MainMenuBar;
        private ToolStripMenuItem FileMenuItem;
        private ToolStripMenuItem FileNewMenuItem;
        private ToolStripMenuItem FileOpenMenuItem;
        private OpenFileDialog OpenProjectFileDialog;
        private ToolStripSeparator toolStripSeparator1;
        private OpenFileDialog ImportFileDialog;
        private ToolStripMenuItem FileSaveMenuItem;
        private ToolStripMenuItem FileExportMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem FileExitMenuItem;
        private SaveFileDialog SaveProjectFileDialog;
        private TFDataGridView StringsDataGrid;
        private FolderBrowserDialog ExportProjectFolderBrowserDialog;
        private ToolStripStatusLabel UsedCharLabel;
        private ToolStripMenuItem ToolsMenuItem;
        private ToolStripMenuItem ImportToolMenuItem;
        private ToolStripMenuItem ImportTFMenuItem;
        private ToolStripMenuItem ImportExcelMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem SearchToolMenuItem;
        private ToolStripMenuItem SearchNextMenuItem;
        private DataGridViewTextBoxColumn colID;
        private DataGridViewTextBoxColumn colFile;
        private DataGridViewTextBoxColumn colGroup;
        private DataGridViewTextBoxColumn colOffset;
        private DataGridViewTextBoxColumn colOriginal;
        private DataGridViewTextBoxColumn colTranslation;
    }
}

