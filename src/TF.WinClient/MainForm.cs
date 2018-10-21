using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TF.Core.Entities;

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

            if (e.ColumnIndex == 5)
            {
                var tfString = (TFString) StringsDataGrid.Rows[e.RowIndex].Tag;

                if (!e.State.HasFlag(DataGridViewElementStates.Selected) && (tfString.Original != tfString.Translation) && (!string.IsNullOrEmpty(tfString.Translation)))
                {
                    e.CellStyle.BackColor = Color.AntiqueWhite;
                }
            }

            var lbPattern = @"(\\r\\n)|(\\n)";
            var pausePattern = @"(\^\^)";
            var tagPattern = @"(<[^>]*>)";
            
            var strSplit = Regex.Split(e.Value.ToString(), $@"{lbPattern}|{pausePattern}|{tagPattern}");

            if (strSplit.Length <= 1)
            {
                return;
            }

            var defaultColor = e.State.HasFlag(DataGridViewElementStates.Selected) ? e.CellStyle.SelectionForeColor : e.CellStyle.ForeColor;
            var lineBreakColor = Color.Red;
            var pauseColor = Color.Brown;
            var tagColor = e.State.HasFlag(DataGridViewElementStates.Selected) ? Color.Azure : Color.Blue;

            var rect = new Rectangle(e.CellBounds.X + 3, e.CellBounds.Y - 1, e.CellBounds.Width - 6, e.CellBounds.Height);
            var x = rect.X;

            var proposedSize = new Size(int.MaxValue, int.MaxValue);
            const TextFormatFlags formatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.Focus | DataGridViewPaintParts.SelectionBackground);

            foreach (var s in strSplit)
            {
                Color c;
                if (Regex.IsMatch(s, lbPattern))
                {
                    c = lineBreakColor;
                }
                else if (Regex.IsMatch(s, tagPattern))
                {
                    c = tagColor;
                }
                else if (Regex.IsMatch(s, pausePattern))
                {
                    c = pauseColor;
                }
                else
                {
                    c = defaultColor;
                }
                
                TextRenderer.DrawText(e.Graphics, s,
                    e.CellStyle.Font, rect, c, formatFlags);

                x += TextRenderer.MeasureText(e.Graphics, s, e.CellStyle.Font, proposedSize, formatFlags).Width;

                rect.Width = rect.Width - (x - rect.X);
                rect.X = x;
            }

            e.Handled = true;
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

        private void CheckGrammarFromStartMenuItem_Click(object sender, EventArgs e)
        {
            CheckGrammarFromStart();
        }

        private void CheckGrammarFromCurrentMenuItem_Click(object sender, EventArgs e)
        {
            CheckGrammarFromCurrentPosition();
        }

        private void ImportSimpleExcelMenuItem_Click(object sender, EventArgs e)
        {
            ImportSimpleExcel();
        }

        private void ImportCompleteExcelMenuItem_Click(object sender, EventArgs e)
        {
            ImportCompleteExcel();
        }

        private void ExportToSimpleExcelMenuItem_Click(object sender, EventArgs e)
        {
            ExportSimpleExcel();
        }

        private void ExportToFullExcelMenuItem_Click(object sender, EventArgs e)
        {
            ExportCompleteExcel();
        }

        private void ImportSimpleTFMenuItem_Click(object sender, EventArgs e)
        {
            ImportSimpleTF();
        }

        private void ImportCompleteTFMenuItem_Click(object sender, EventArgs e)
        {
            ImportCompleteTF();
        }
    }
}