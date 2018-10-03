using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;

namespace TF.WinClient
{
    public partial class GrammarForm : Form
    {
        private string _text;
        private LanguageToolResponse _lgResponse;
        private RestClient _client;

        public string FinalText => CheckedTextBox.Text;

        public GrammarForm()
        {
            InitializeComponent();
        }

        public GrammarForm(string text, LanguageToolResponse lgResponse, RestClient client)
        {
            InitializeComponent();

            _text = text;
            _lgResponse = lgResponse;
            _client = client;

            CheckedTextBox.Text = _text;

            HighLightMatches();
            UpdateInfo();
        }

        private LanguageToolResponse CheckGrammar()
        {
            var request = new RestRequest("check", Method.POST);
            request.AddParameter("language", "es");
            request.AddParameter("text", CheckedTextBox.Text);

            var response = _client.Execute(request);

            return _client.Deserialize<LanguageToolResponse>(response).Data;
        }

        private void CheckAgainButton_Click(object sender, EventArgs e)
        {
            _lgResponse = CheckGrammar();

            CheckedTextBox.Select(0, CheckedTextBox.TextLength);
            CheckedTextBox.SelectionBackColor = Color.Transparent;

            HighLightMatches();
            UpdateInfo();
        }

        private void HighLightMatches()
        {
            foreach (var match in _lgResponse.Matches)
            {
                CheckedTextBox.SelectionStart = match.Offset;
                CheckedTextBox.SelectionLength = match.Length;
                CheckedTextBox.SelectionBackColor = Color.Orange;
            }

            CheckedTextBox.SelectionStart = 0;
            CheckedTextBox.SelectionLength = 0;
        }

        private void UpdateInfo()
        {
            InfoTextBox.Clear();

            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi {\colortbl;\red0\green0\blue0;\red255\green0\blue0;\red255\green255\blue0;}");

            sb.Append(_lgResponse.Matches.Count == 1
                ? @"1 posible problema encontrado...\line\line "
                : $@"{_lgResponse.Matches.Count} posibles problemas encontrados...\line\line ");

            foreach (var match in _lgResponse.Matches)
            {
                sb.Append($@"\b {match.Message}\b0\line ");
                
                sb.Append(@"\i ");
                var context = match.Context;
                var temp = context.Text.Insert(context.Offset + context.Length, "\\highlight0 ");
                temp = temp.Insert(context.Offset, "\\highlight3 ");
                sb.Append(temp);
                sb.Append(@"\i0\line ");

                var suggestions = match.Replacements;
                if (suggestions.Count > 0)
                {
                    sb.Append("Sugerencias: ");
                    foreach (var suggestion in suggestions)
                    {
                        sb.Append(suggestion.Value);
                        sb.Append("; ");
                    }
                }
                else
                {
                    sb.Append("No hay sugerencias.");
                }
                sb.Append(@"\line\line ");
            }

            sb.Append("}");

            InfoTextBox.Rtf = sb.ToString();
        }

        private void CheckedTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            /*if (e.Button != MouseButtons.Right)
            {
                return;
            }

            if (_lgResponse.Matches.Count == 0)
            {
                return;
            }

            var clickPosition = CheckedTextBox.GetCharIndexFromPosition(e.Location);

            var clickedMatch =
                _lgResponse.Matches.FirstOrDefault(x =>
                    x.Offset <= clickPosition && x.Offset + x.Length >= clickPosition);

            if (clickedMatch == null)
            {
                return;
            }

            var cm = new ContextMenu();

            if (clickedMatch.Replacements.Count == 0)
            {
                var item = new MenuItem {Text = "No hay sugerencias", Enabled = false};
                cm.MenuItems.Add(item);
            }
            else
            {
                
            }*/
        }
    }
}
