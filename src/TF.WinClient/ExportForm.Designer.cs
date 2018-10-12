using System.ComponentModel;
using System.Windows.Forms;

namespace TF.WinClient
{
    partial class ExportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CharReplacementDataGrid = new System.Windows.Forms.DataGridView();
            this.colString1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colString2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.CharReplacementCombo = new System.Windows.Forms.ComboBox();
            this.EncodingCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.CharReplacementDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // CharReplacementDataGrid
            // 
            this.CharReplacementDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CharReplacementDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colString1,
            this.colString2});
            this.CharReplacementDataGrid.Location = new System.Drawing.Point(15, 39);
            this.CharReplacementDataGrid.Name = "CharReplacementDataGrid";
            this.CharReplacementDataGrid.Size = new System.Drawing.Size(345, 150);
            this.CharReplacementDataGrid.TabIndex = 0;
            // 
            // colString1
            // 
            this.colString1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colString1.HeaderText = "Original";
            this.colString1.Name = "colString1";
            this.colString1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colString2
            // 
            this.colString2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colString2.HeaderText = "Sustituir por...";
            this.colString2.Name = "colString2";
            this.colString2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sustitución de caracteres:";
            // 
            // CharReplacementCombo
            // 
            this.CharReplacementCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CharReplacementCombo.FormattingEnabled = true;
            this.CharReplacementCombo.Items.AddRange(new object[] {
            "Sin sustitución",
            "Usar la matriz"});
            this.CharReplacementCombo.Location = new System.Drawing.Point(148, 12);
            this.CharReplacementCombo.Name = "CharReplacementCombo";
            this.CharReplacementCombo.Size = new System.Drawing.Size(212, 21);
            this.CharReplacementCombo.TabIndex = 2;
            this.CharReplacementCombo.SelectedIndexChanged += new System.EventHandler(this.CharReplacementCombo_SelectedIndexChanged);
            // 
            // EncodingCombo
            // 
            this.EncodingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EncodingCombo.FormattingEnabled = true;
            this.EncodingCombo.Items.AddRange(new object[] {
            "ISO-8859-1",
            "UTF-8",
            "UTF-16"});
            this.EncodingCombo.Location = new System.Drawing.Point(148, 204);
            this.EncodingCombo.Name = "EncodingCombo";
            this.EncodingCombo.Size = new System.Drawing.Size(212, 21);
            this.EncodingCombo.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 207);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Codificación de salida:";
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(285, 238);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "Cancelar";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // OkBtn
            // 
            this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Location = new System.Drawing.Point(204, 238);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(75, 23);
            this.OkBtn.TabIndex = 6;
            this.OkBtn.Text = "Aceptar";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // ExportForm
            // 
            this.AcceptButton = this.OkBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(372, 273);
            this.ControlBox = false;
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.EncodingCombo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CharReplacementCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CharReplacementDataGrid);
            this.Name = "ExportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Opciones de exportación";
            ((System.ComponentModel.ISupportInitialize)(this.CharReplacementDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataGridView CharReplacementDataGrid;
        private Label label1;
        private ComboBox CharReplacementCombo;
        private ComboBox EncodingCombo;
        private Label label2;
        private Button CancelBtn;
        private Button OkBtn;
        private DataGridViewTextBoxColumn colString1;
        private DataGridViewTextBoxColumn colString2;
    }
}