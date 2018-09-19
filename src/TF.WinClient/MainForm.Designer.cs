namespace TF.WinClient
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.UsedCharLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainMenuBar = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.FileSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.FileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportTFMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenProjectFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ImportFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveProjectFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.StringsDataGrid = new TF.WinClient.TFDataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOriginal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTranslation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExportProjectFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.StatusBar.SuspendLayout();
            this.MainMenuBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StringsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // StatusBar
            // 
            this.StatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UsedCharLabel});
            this.StatusBar.Location = new System.Drawing.Point(0, 604);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(1088, 22);
            this.StatusBar.TabIndex = 0;
            // 
            // UsedCharLabel
            // 
            this.UsedCharLabel.Name = "UsedCharLabel";
            this.UsedCharLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // MainMenuBar
            // 
            this.MainMenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.ToolsMenuItem});
            this.MainMenuBar.Location = new System.Drawing.Point(0, 0);
            this.MainMenuBar.Name = "MainMenuBar";
            this.MainMenuBar.Size = new System.Drawing.Size(1088, 24);
            this.MainMenuBar.TabIndex = 1;
            this.MainMenuBar.Text = "menuStrip1";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileNewMenuItem,
            this.FileOpenMenuItem,
            this.toolStripSeparator1,
            this.FileSaveMenuItem,
            this.FileExportMenuItem,
            this.toolStripSeparator2,
            this.FileExitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(60, 20);
            this.FileMenuItem.Text = "&Archivo";
            // 
            // FileNewMenuItem
            // 
            this.FileNewMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FileNewMenuItem.Image")));
            this.FileNewMenuItem.Name = "FileNewMenuItem";
            this.FileNewMenuItem.Size = new System.Drawing.Size(168, 22);
            this.FileNewMenuItem.Text = "&Nueva traducción";
            this.FileNewMenuItem.Click += new System.EventHandler(this.FileNewMenuItem_Click);
            // 
            // FileOpenMenuItem
            // 
            this.FileOpenMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FileOpenMenuItem.Image")));
            this.FileOpenMenuItem.Name = "FileOpenMenuItem";
            this.FileOpenMenuItem.Size = new System.Drawing.Size(168, 22);
            this.FileOpenMenuItem.Text = "&Abrir traducción";
            this.FileOpenMenuItem.Click += new System.EventHandler(this.FileOpenMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // FileSaveMenuItem
            // 
            this.FileSaveMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FileSaveMenuItem.Image")));
            this.FileSaveMenuItem.Name = "FileSaveMenuItem";
            this.FileSaveMenuItem.Size = new System.Drawing.Size(168, 22);
            this.FileSaveMenuItem.Text = "&Guardar";
            this.FileSaveMenuItem.Click += new System.EventHandler(this.FileSaveMenuItem_Click);
            // 
            // FileExportMenuItem
            // 
            this.FileExportMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FileExportMenuItem.Image")));
            this.FileExportMenuItem.Name = "FileExportMenuItem";
            this.FileExportMenuItem.Size = new System.Drawing.Size(168, 22);
            this.FileExportMenuItem.Text = "Exportar...";
            this.FileExportMenuItem.Click += new System.EventHandler(this.FileExportMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // FileExitMenuItem
            // 
            this.FileExitMenuItem.Name = "FileExitMenuItem";
            this.FileExitMenuItem.Size = new System.Drawing.Size(168, 22);
            this.FileExitMenuItem.Text = "Salir";
            this.FileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
            // 
            // ToolsMenuItem
            // 
            this.ToolsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ImportToolMenuItem});
            this.ToolsMenuItem.Name = "ToolsMenuItem";
            this.ToolsMenuItem.Size = new System.Drawing.Size(90, 20);
            this.ToolsMenuItem.Text = "Herramientas";
            // 
            // ImportToolMenuItem
            // 
            this.ImportToolMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ImportTFMenuItem,
            this.ImportExcelMenuItem});
            this.ImportToolMenuItem.Name = "ImportToolMenuItem";
            this.ImportToolMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ImportToolMenuItem.Text = "Importar traduccion";
            // 
            // ImportTFMenuItem
            // 
            this.ImportTFMenuItem.Name = "ImportTFMenuItem";
            this.ImportTFMenuItem.Size = new System.Drawing.Size(135, 22);
            this.ImportTFMenuItem.Text = "Desde tf_*";
            this.ImportTFMenuItem.Click += new System.EventHandler(this.ImportTFMenuItem_Click);
            // 
            // ImportExcelMenuItem
            // 
            this.ImportExcelMenuItem.Name = "ImportExcelMenuItem";
            this.ImportExcelMenuItem.Size = new System.Drawing.Size(135, 22);
            this.ImportExcelMenuItem.Text = "Desde Excel";
            this.ImportExcelMenuItem.Click += new System.EventHandler(this.ImportExcelMenuItem_Click);
            // 
            // StringsDataGrid
            // 
            this.StringsDataGrid.AllowUserToAddRows = false;
            this.StringsDataGrid.AllowUserToDeleteRows = false;
            this.StringsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.StringsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colGroup,
            this.colOffset,
            this.colOriginal,
            this.colTranslation});
            this.StringsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StringsDataGrid.Location = new System.Drawing.Point(0, 24);
            this.StringsDataGrid.MultiSelect = false;
            this.StringsDataGrid.Name = "StringsDataGrid";
            this.StringsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.StringsDataGrid.Size = new System.Drawing.Size(1088, 580);
            this.StringsDataGrid.TabIndex = 2;
            this.StringsDataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.StringsDataGrid_CellEndEdit);
            this.StringsDataGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.StringsDataGrid_CellMouseDoubleClick);
            this.StringsDataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.StringsDataGrid_CellPainting);
            this.StringsDataGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.StringsDataGrid_EditingControlShowing);
            // 
            // colID
            // 
            this.colID.HeaderText = "ID";
            this.colID.Name = "colID";
            this.colID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colID.Visible = false;
            // 
            // colGroup
            // 
            this.colGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colGroup.DefaultCellStyle = dataGridViewCellStyle1;
            this.colGroup.FillWeight = 25F;
            this.colGroup.HeaderText = "Grupo";
            this.colGroup.Name = "colGroup";
            this.colGroup.ReadOnly = true;
            this.colGroup.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colOffset
            // 
            this.colOffset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colOffset.DefaultCellStyle = dataGridViewCellStyle2;
            this.colOffset.FillWeight = 15F;
            this.colOffset.HeaderText = "Offset";
            this.colOffset.Name = "colOffset";
            this.colOffset.ReadOnly = true;
            this.colOffset.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colOriginal
            // 
            this.colOriginal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.colOriginal.DefaultCellStyle = dataGridViewCellStyle3;
            this.colOriginal.HeaderText = "Original";
            this.colOriginal.Name = "colOriginal";
            this.colOriginal.ReadOnly = true;
            this.colOriginal.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colTranslation
            // 
            this.colTranslation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTranslation.HeaderText = "Traducción";
            this.colTranslation.Name = "colTranslation";
            this.colTranslation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ExportProjectFolderBrowserDialog
            // 
            this.ExportProjectFolderBrowserDialog.Description = "Selecciona la carpeta donde guardar el fichero.\\r\\nSe guardará con el mismo nombr" +
    "e que tenía el fichero original.";
            this.ExportProjectFolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1088, 626);
            this.Controls.Add(this.StringsDataGrid);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.MainMenuBar);
            this.MainMenuStrip = this.MainMenuBar;
            this.Name = "MainForm";
            this.Text = "Translation Framework";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.MainMenuBar.ResumeLayout(false);
            this.MainMenuBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StringsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.MenuStrip MainMenuBar;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenuItem;
        private System.Windows.Forms.OpenFileDialog OpenProjectFileDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog ImportFileDialog;
        private System.Windows.Forms.ToolStripMenuItem FileSaveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExportMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem FileExitMenuItem;
        private System.Windows.Forms.SaveFileDialog SaveProjectFileDialog;
        private TFDataGridView StringsDataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOriginal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTranslation;
        private System.Windows.Forms.FolderBrowserDialog ExportProjectFolderBrowserDialog;
        private System.Windows.Forms.ToolStripStatusLabel UsedCharLabel;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportTFMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelMenuItem;
    }
}

