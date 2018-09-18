using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TF.Core;

namespace TF.WinClient
{
    public partial class ExportForm : Form
    {
        public int CharReplacement => CharReplacementCombo.SelectedIndex;
        public string SelectedEncoding => EncodingCombo.SelectedItem.ToString();

        public IList<Tuple<string, string>> CharReplacementList
        {
            get
            {
                var result = new List<Tuple<string, string>>();

                for (var i = 0; i < CharReplacementDataGrid.RowCount - 1; i++)
                {
                    var row = CharReplacementDataGrid.Rows[i];
                    var string1 = row.Cells["colString1"].Value.ToString();
                    var string2 = row.Cells["colString2"].Value.ToString();

                    if (!string.IsNullOrEmpty(string1))
                    {
                        result.Add(new Tuple<string, string>(string1, string2));
                    }
                }

                return result;
            }
        }
        
        public ExportForm()
        {
            InitializeComponent();
            
            var topLeftHeaderCell = CharReplacementDataGrid.TopLeftHeaderCell;

            CharReplacementCombo.SelectedIndex = 0;
            EncodingCombo.SelectedIndex = 0;
            CharReplacementDataGrid.Rows.Clear();
        }

        public void LoadConfig(ExportOptions config)
        {
            CharReplacementCombo.SelectedIndex = config.CharReplacement;
            EncodingCombo.SelectedItem = config.SelectedEncoding.HeaderName.ToUpperInvariant();

            foreach (var tuple in config.CharReplacementList)
            {
                CharReplacementDataGrid.Rows.Add(tuple.Item1, tuple.Item2);
            }
        }

        private void CharReplacementCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CharReplacementDataGrid.Enabled = CharReplacementCombo.SelectedIndex > 0;
        }
    }
}
