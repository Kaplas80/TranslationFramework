namespace TF.WinClient
{
    partial class GrammarForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.CheckedTextBox = new System.Windows.Forms.RichTextBox();
            this.InfoTextBox = new System.Windows.Forms.RichTextBox();
            this.UndoButton = new System.Windows.Forms.Button();
            this.CommitButton = new System.Windows.Forms.Button();
            this.CancelProcessButton = new System.Windows.Forms.Button();
            this.CheckAgainButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CheckedTextBox
            // 
            this.CheckedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedTextBox.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckedTextBox.Location = new System.Drawing.Point(12, 12);
            this.CheckedTextBox.Name = "CheckedTextBox";
            this.CheckedTextBox.Size = new System.Drawing.Size(674, 142);
            this.CheckedTextBox.TabIndex = 0;
            this.CheckedTextBox.Text = "";
            this.CheckedTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CheckedTextBox_MouseUp);
            // 
            // InfoTextBox
            // 
            this.InfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoTextBox.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoTextBox.Location = new System.Drawing.Point(12, 160);
            this.InfoTextBox.Name = "InfoTextBox";
            this.InfoTextBox.ReadOnly = true;
            this.InfoTextBox.Size = new System.Drawing.Size(674, 223);
            this.InfoTextBox.TabIndex = 1;
            this.InfoTextBox.Text = "";
            // 
            // UndoButton
            // 
            this.UndoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.UndoButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.UndoButton.Location = new System.Drawing.Point(573, 389);
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(113, 23);
            this.UndoButton.TabIndex = 2;
            this.UndoButton.Text = "Rechazar cambios";
            this.UndoButton.UseVisualStyleBackColor = true;
            // 
            // CommitButton
            // 
            this.CommitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CommitButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.CommitButton.Location = new System.Drawing.Point(454, 389);
            this.CommitButton.Name = "CommitButton";
            this.CommitButton.Size = new System.Drawing.Size(113, 23);
            this.CommitButton.TabIndex = 3;
            this.CommitButton.Text = "Aceptar cambios";
            this.CommitButton.UseVisualStyleBackColor = true;
            // 
            // CancelProcessButton
            // 
            this.CancelProcessButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CancelProcessButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelProcessButton.Location = new System.Drawing.Point(12, 389);
            this.CancelProcessButton.Name = "CancelProcessButton";
            this.CancelProcessButton.Size = new System.Drawing.Size(113, 23);
            this.CancelProcessButton.TabIndex = 4;
            this.CancelProcessButton.Text = "Cancelar Revisión";
            this.CancelProcessButton.UseVisualStyleBackColor = true;
            // 
            // CheckAgainButton
            // 
            this.CheckAgainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckAgainButton.Location = new System.Drawing.Point(335, 389);
            this.CheckAgainButton.Name = "CheckAgainButton";
            this.CheckAgainButton.Size = new System.Drawing.Size(113, 23);
            this.CheckAgainButton.TabIndex = 5;
            this.CheckAgainButton.Text = "Volver a revisar";
            this.CheckAgainButton.UseVisualStyleBackColor = true;
            this.CheckAgainButton.Click += new System.EventHandler(this.CheckAgainButton_Click);
            // 
            // GrammarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 424);
            this.Controls.Add(this.CheckAgainButton);
            this.Controls.Add(this.CancelProcessButton);
            this.Controls.Add(this.CommitButton);
            this.Controls.Add(this.UndoButton);
            this.Controls.Add(this.InfoTextBox);
            this.Controls.Add(this.CheckedTextBox);
            this.Name = "GrammarForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ortografía y Gramática";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox CheckedTextBox;
        private System.Windows.Forms.RichTextBox InfoTextBox;
        private System.Windows.Forms.Button UndoButton;
        private System.Windows.Forms.Button CommitButton;
        private System.Windows.Forms.Button CancelProcessButton;
        private System.Windows.Forms.Button CheckAgainButton;
    }
}