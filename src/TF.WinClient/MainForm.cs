using System;
using System.Drawing;
using System.Windows.Forms;

namespace TF.WinClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            var topLeftHeaderCell = StringsDataGrid.TopLeftHeaderCell;
        }

        private void FileNewMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void FileOpenMenuItem_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_openProject != null)
            {
                var result = MessageBox.Show("¿Quieres guardar la traducción antes de salir?", "Salir",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    case DialogResult.Yes:
                        SaveProject();
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
            CloseProject();
        }

        private void StringsDataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            UpdateString(e.RowIndex);

            UpdateProcessedStringsLabel();
        }

        private void FileSaveMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void FileExportMenuItem_Click(object sender, EventArgs e)
        {
            ExportProject();
        }

        private void FileExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StringsDataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            if (e.Value == null)
            {
                return;
            }

            var strValue = e.Value.ToString();
            var strSplit1 = strValue.Split(new[] {"\\r\\n"}, StringSplitOptions.None);
            var strSplit2 = strValue.Split(new[] {"\\n"}, StringSplitOptions.None);
            
            if (strSplit1.Length <= 1 && strSplit2.Length <= 1)
            {
                return;
            }

            var strSplit = strSplit1;
            var separator = "\\r\\n";
            if (strSplit1.Length <= 1 && strSplit2.Length > 1)
            {
                strSplit = strSplit2;
                separator = "\\n";
            }

            var defaultColor = e.State.HasFlag(DataGridViewElementStates.Selected) ? e.CellStyle.SelectionForeColor : e.CellStyle.ForeColor;
            var alternateColor = Color.Red;

            var rect = new Rectangle(e.CellBounds.X + 3, e.CellBounds.Y - 1, e.CellBounds.Width - 6, e.CellBounds.Height);
            var x = rect.X;

            var proposedSize = new Size(int.MaxValue, int.MaxValue);
            const TextFormatFlags formatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.Focus | DataGridViewPaintParts.SelectionBackground);

            var i = 0;
            for (; i < strSplit.Length - 1; i++)
            {
                TextRenderer.DrawText(e.Graphics, strSplit[i], 
                    e.CellStyle.Font, rect, defaultColor, formatFlags);

                x += TextRenderer.MeasureText(e.Graphics, strSplit[i], e.CellStyle.Font, proposedSize, formatFlags).Width;

                rect.Width = rect.Width - (x - rect.X);
                rect.X = x;
                        
                TextRenderer.DrawText(e.Graphics, separator, 
                    e.CellStyle.Font, rect, alternateColor, formatFlags);

                x += TextRenderer.MeasureText(e.Graphics, separator, e.CellStyle.Font, proposedSize, formatFlags).Width;
                rect.Width = rect.Width - (x - rect.X);
                rect.X = x;
            }

            TextRenderer.DrawText(e.Graphics, strSplit[i], 
                e.CellStyle.Font, rect, defaultColor, formatFlags);

            e.Handled = true;
        }

        private void ImportTFMenuItem_Click(object sender, EventArgs e)
        {
            ImportTF();
        }

        private void ImportExcelMenuItem_Click(object sender, EventArgs e)
        {
            ImportExcel();
        }

        private void StringsDataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control.GetType()== typeof(DataGridViewTextBoxEditingControl)) // "System.Windows.Forms.DataGridViewTextBoxEditingControl")
            {
                SendKeys.Send("{RIGHT}");
            }
        }

        private void StringsDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            StringsDataGrid.BeginEdit(false);
        }

        private void SearchToolMenuItem_Click(object sender, EventArgs e)
        {
            SearchText();
        }

        private void SearchNextMenuItem_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void ExportToExcelMenuItem_Click(object sender, EventArgs e)
        {
            ExportExcel();
        }

        private void CheckGrammarFromStartMenuItem_Click(object sender, EventArgs e)
        {
            CheckGrammarFromStart();
        }

        private void CheckGrammarFromCurrentMenuItem_Click(object sender, EventArgs e)
        {
            CheckGrammarFromCurrentPosition();
        }
    }
}