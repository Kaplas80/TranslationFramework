using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using SpreadsheetLight;
using TF.Core;
using TF.Core.Entities;
using RestSharp;

namespace TF.WinClient
{
    public partial class MainForm
    {
        private class Search
        {
            public string Text;
            public bool UseCapitalization;
            public int StartIndex;
        }

        private Search _search;
        private Project _openProject;

        private string GetSaveProjectFilesFilter()
        {
            var projects = Main.GetSupportedProjects();

            var sb = new StringBuilder();
            foreach (var project in projects)
            {
                sb.Append(project.SaveProjectFilter);
                sb.Append("|");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private string GetOpenProjectFilesFilter()
        {
            var projects = Main.GetSupportedProjects();

            var sb = new StringBuilder();
            foreach (var project in projects)
            {
                sb.Append(project.OpenProjectFilter);
                sb.Append(";");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private void CreateNewProject()
        {
            CloseProject();

            SaveProjectFileDialog.Filter = GetSaveProjectFilesFilter();

            var result = SaveProjectFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                Project project = null;
#if !DEBUG
                try
                {
#endif
                    var i = 0;
                    while (File.Exists(SaveProjectFileDialog.FileName) && i < 10)
                    {
                        try
                        {
                            File.Delete(SaveProjectFileDialog.FileName);
                        }
                        catch (Exception e)
                        {
                            i++;
                        }
                    }

                    if (File.Exists(SaveProjectFileDialog.FileName))
                    {
                        MessageBox.Show("No se ha podido sobreescribir el fichero", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    project = ProjectFactory.GetProject(SaveProjectFileDialog.FileName);

                    if (AddFilesToProject(project))
                    {
                        project.SaveStrings();

                        _openProject = project;

                        LoadDataGrid();

                        Text = $"Translation Framework - {_openProject.Path}";
                    }
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido crear el proyecto.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    project?.Close();

                    Cursor.Current = Cursors.Default;
                    Enabled = true;
                }
#endif
            }
        }

        private void OpenProject()
        {
            CloseProject();
            
            OpenProjectFileDialog.Filter = "Traducciones|" + GetOpenProjectFilesFilter();

            var result = OpenProjectFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                Project project = null;
#if !DEBUG
                try
                {
#endif
                    Enabled = false;
                    Cursor.Current = Cursors.WaitCursor;
                    project = ProjectFactory.GetProject(OpenProjectFileDialog.FileName);

                    project.LoadFiles();

                    project.LoadStrings();
                    Enabled = true;
                    Cursor.Current = Cursors.Default;
                    _openProject = project;

                    LoadDataGrid();

                    Text = $"Translation Framework - {_openProject.Path}";
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    project?.Close();
                    
                    Cursor.Current = Cursors.Default;
                    Enabled = true;
                }
#endif
            }
        }

        private void CloseProject()
        {
            if (_openProject != null)
            {
                _openProject.Close();
                _openProject = null;
            }

            StringsDataGrid.Rows.Clear();
            Text = "Translation Framework";
        }

        private bool AddFilesToProject(Project p)
        {
            var fileFilter = p.CompatibleFilesFilter;

            ImportFileDialog.Filter = fileFilter;

            var result = ImportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                Enabled = false;

                foreach (var fileName in ImportFileDialog.FileNames)
                {
#if !DEBUG
                    try
                    {
#endif
                        p.SetFile(fileName);
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show($"Error al abrir el fichero {fileName}.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Cursor.Current = Cursors.WaitCursor;
                    }
#endif
                }
                Cursor.Current = Cursors.Default;
                Enabled = true;
            }

            return p.Files.Count > 0;
        }

        private void LoadDataGrid()
        {
            Cursor.Current = Cursors.WaitCursor;
            foreach (var tfString in _openProject.Strings)
            {
                if (tfString.Visible)
                {
                    var row = StringsDataGrid.Rows.Add(tfString.Id, tfString.FileId, tfString.Section, tfString.Offset.ToString("X8"),
                        tfString.Original, tfString.Translation);

                    StringsDataGrid.Rows[row].Tag = tfString;
                }
            }

            UpdateProcessedStringsLabel();
            Cursor.Current = Cursors.Default;
        }

        private void UpdateString(int row)
        {
            var tfString = (TFString) StringsDataGrid.Rows[row].Tag;
            tfString.Translation = StringsDataGrid.Rows[row].Cells["colTranslation"].Value.ToString();
        }

        private void SaveProject()
        {
            _openProject?.UpdateStrings();
        }

        private void ExportProject()
        {
            if (_openProject != null)
            {
                var form = new ExportForm();
                var options = new ExportOptions
                {
                    CharReplacement = Convert.ToInt32(_openProject.GetConfigValue("OUTPUT_REPLACEMENT")),
                    SelectedEncoding = Encoding.GetEncoding(_openProject.GetConfigValue("OUTPUT_ENCODING")),
                    CharReplacementList = _openProject.GetCharReplacements()
                };

                form.LoadConfig(options);

                var result = form.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    options.CharReplacement = form.CharReplacement;
                    options.SelectedEncoding = Encoding.GetEncoding(form.SelectedEncoding);
                    options.CharReplacementList = form.CharReplacementList;

                    var folderResult = ExportProjectFolderBrowserDialog.ShowDialog(this);
                    
                    if (folderResult == DialogResult.OK)
                    {
                        Enabled = false;
                        Cursor.Current = Cursors.WaitCursor;
                        _openProject.Export(ExportProjectFolderBrowserDialog.SelectedPath, options);
                        Cursor.Current = Cursors.Default;
                        Enabled = true;
                    }
                }
            }
        }

        private void UpdateProcessedStringsLabel()
        {
            var totalStrings = _openProject.Strings.Count(x => x.Visible);
            var modifiedStrings = _openProject.Strings.Count(x => x.Visible && (x.Original != x.Translation));
            UsedCharLabel.Text = $"Cadenas modificadas: {modifiedStrings} de {totalStrings}";
        }

        private void ImportTF()
        {
            if (_openProject == null)
            {
                return;
            }

            ImportFileDialog.Filter = "Traducciones|" + GetOpenProjectFilesFilter();

            var result = ImportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var strings = new Dictionary<string, string>();

                Project project = null;
                try
                {
                    project = ProjectFactory.GetProject(ImportFileDialog.FileName);

                    project.LoadFiles();

                    project.LoadStrings();

                    foreach (var tfString in project.Strings)
                    {
                        var key = tfString.Original;
                        var value = tfString.Translation;

                        if (!string.IsNullOrEmpty(key))
                        {
                            if (!strings.ContainsKey(key))
                            {
                                strings.Add(key, value);
                            }
                        }
                    }

                    project.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    project?.Close();

                    return;
                }

                foreach (var tfString in _openProject.Strings)
                {
                    var key = tfString.Original;
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (strings.ContainsKey(key))
                        {
                            tfString.Translation = strings[key];
                        }
                    }
                }

                StringsDataGrid.Rows.Clear();
                LoadDataGrid();
            }
        }

        private void ImportSimpleExcel()
        {
            if (_openProject == null)
            {
                return;
            }

            ImportFileDialog.Filter = "Archivos Excel|*.xls;*.xlsx";

            var result = ImportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var strings = new Dictionary<string, string>();
                try
                {
                    using (var stream = File.Open(ImportFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        // Auto-detect format, supports:
                        //  - Binary Excel files (2.0-2003 format; *.xls)
                        //  - OpenXml Excel files (2007 format; *.xlsx)
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {

                            var content = reader.AsDataSet();

                            var table = content.Tables[0];

                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                var key = table.Rows[i][0].ToString();
                                var value = table.Rows[i][1].ToString();

                                if (!string.IsNullOrEmpty(key) && !strings.ContainsKey(key))
                                {
                                    strings.Add(key, value);
                                }
                            }
                        }
                    }

                    foreach (var tfString in _openProject.Strings)
                    {
                        var key = tfString.Original;
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (strings.ContainsKey(key))
                            {
                                tfString.Translation = strings[key];
                            }
                        }
                    }

                    StringsDataGrid.Rows.Clear();
                    LoadDataGrid();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportCompleteExcel()
        {
            if (_openProject == null)
            {
                return;
            }

            ImportFileDialog.Filter = "Archivos Excel|*.xls;*.xlsx";

            var result = ImportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var strings = new Dictionary<string, string>();
                try
                {
                    using (var stream = File.Open(ImportFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        // Auto-detect format, supports:
                        //  - Binary Excel files (2.0-2003 format; *.xls)
                        //  - OpenXml Excel files (2007 format; *.xlsx)
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var content = reader.AsDataSet();

                            var table = content.Tables[0];

                            for (var i = 1; i < table.Rows.Count; i++)
                            {
                                var key = string.Concat(table.Rows[i][0].ToString(), "|", table.Rows[i][1].ToString(), "|", table.Rows[i][2].ToString(), "|", table.Rows[i][3].ToString());
                                var value = table.Rows[i][4].ToString();

                                if (!string.IsNullOrEmpty(key) && !strings.ContainsKey(key))
                                {
                                    strings.Add(key, value);
                                }
                            }
                        }
                    }

                    foreach (var tfString in _openProject.Strings)
                    {
                        var key = string.Concat(tfString.FileId, "|", tfString.Section, "|", tfString.Offset, "|", tfString.Original);
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (strings.ContainsKey(key))
                            {
                                tfString.Translation = strings[key];
                            }
                        }
                    }

                    StringsDataGrid.Rows.Clear();
                    LoadDataGrid();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SearchText()
        {
            var form = new SearchForm();
            var result = form.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                _search = new Search
                {
                    Text = form.SearchText,
                    UseCapitalization = form.UseCaps,
                    StartIndex = -1
                };

                DoSearch();
            }
        }

        private void DoSearch()
        {
            if (_search == null || _openProject == null)
            {
                return;
            }

            var i = _search.StartIndex + 1;
            var result = -1L;
            while (i < StringsDataGrid.RowCount)
            {
                var tfString = (TFString) StringsDataGrid.Rows[i].Tag;

                var original = tfString.Original;
                var translation = tfString.Translation;
                var textToSearch = _search.Text;

                if (!_search.UseCapitalization)
                {
                    original = original.ToLower();
                    translation = translation.ToLower();
                    textToSearch = textToSearch.ToLower();
                }

                if (original.Contains(textToSearch) || translation.Contains(textToSearch))
                {
                    result = i;
                    break;
                }

                i++;
            }

            if (result != -1)
            {
                StringsDataGrid.ClearSelection();
                StringsDataGrid.Rows[i].Cells["colTranslation"].Selected = true;
                StringsDataGrid.FirstDisplayedScrollingRowIndex = i;
                _search.StartIndex = i;
            }
            else
            {
                MessageBox.Show("No se ha encontrado el texto");
            }
        }

        private void ExportSimpleExcel()
        {
            if (_openProject == null)
            {
                return;
            }

            ExportFileDialog.Filter = "Archivos Excel|*.xlsx";

            var result = ExportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var sl = new SLDocument();

                var row = 1;
                foreach (var str in _openProject.Strings.Where(x => x.Visible))
                {
                    sl.SetCellValue(row, 1, str.Original);
                    sl.SetCellValue(row, 2, str.Translation);
                    row++;
                }

                sl.SaveAs(ExportFileDialog.FileName);
            }
        }

        private void ExportCompleteExcel()
        {
            if (_openProject == null)
            {
                return;
            }

            ExportFileDialog.Filter = "Archivos Excel|*.xlsx";

            var result = ExportFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var sl = new SLDocument();

                var row = 2;
                sl.SetCellValue(1, 1, "ID. FICHERO");
                sl.SetCellValue(1, 2, "SECCION");
                sl.SetCellValue(1, 3, "OFFSET");
                sl.SetCellValue(1, 4, "ORIGINAL");
                sl.SetCellValue(1, 5, "TRADUCCION");
                foreach (var str in _openProject.Strings.Where(x => x.Visible))
                {
                    sl.SetCellValue(row, 1, str.FileId);
                    sl.SetCellValue(row, 2, str.Section);
                    sl.SetCellValue(row, 3, str.Offset);
                    sl.SetCellValue(row, 4, str.Original);
                    sl.SetCellValue(row, 5, str.Translation);
                    row++;
                }

                sl.SaveAs(ExportFileDialog.FileName);
            }
        }

        private void CheckGrammarFromStart()
        {
            if (_openProject == null)
            {
                return;
            }

            CheckGrammar(0);
        }

        private void CheckGrammarFromCurrentPosition()
        {
            if (_openProject == null)
            {
                return;
            }

            var selectedRow = 0;
            if (StringsDataGrid.SelectedCells.Count > 0)
            {
                selectedRow = StringsDataGrid.SelectedCells[0].RowIndex;
            }

            CheckGrammar(selectedRow);
        }

        private void CheckGrammar(int startRow)
        {
            var client = new RestClient("http://localhost:8081/v2");

            if (!IsGrammarServerEnabled(client))
            {
                MessageBox.Show(
                    "No se ha podido conectar con el servidor de LanguageTool, comprueba que está funcionando en http://localhost:8081",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            var selectedRow = StringsDataGrid.SelectedCells[0].RowIndex;
            var i = startRow;
            while (i < StringsDataGrid.RowCount)
            {
                var tfString = (TFString) StringsDataGrid.Rows[i].Tag;

                var original = tfString.Original;
                var translation = tfString.Translation;

                if (original != translation)
                {
                    var numBeginSpaces = CountStartSpaces(translation);
                    translation = translation.TrimStart(' ');

                    var linebreak = "\\n";
                    if (translation.Contains("\\r\\n"))
                    {
                        linebreak = "\\r\\n";
                    }

                    translation = translation.Replace("\\r\\n", "\r\n");
                    translation = translation.Replace("\\n", "\n");

                    try
                    {
                        var lgResponse = CheckGrammar(client, translation);

                        if (lgResponse.Matches.Count > 0)
                        {
                            var grammarForm = new GrammarForm(translation, lgResponse, client);

                            var result = grammarForm.ShowDialog(this);

                            if (result == DialogResult.Yes)
                            {
                                var correctedTranslation = grammarForm.FinalText;
                                for (var j = 0; j < numBeginSpaces; j++)
                                {
                                    correctedTranslation = correctedTranslation.Insert(0, " ");
                                }
                                correctedTranslation = correctedTranslation.Replace("\n", linebreak);
                                tfString.Translation = correctedTranslation;
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                i = StringsDataGrid.RowCount;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                            $"Se ha producido un error al comprobar la ortografía\r\n{e.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        i = StringsDataGrid.RowCount;
                    }
                }

                i++;
            }

            StringsDataGrid.Rows.Clear();
            LoadDataGrid();

            StringsDataGrid.FirstDisplayedScrollingRowIndex = selectedRow;
        }

        private static int CountStartSpaces(string text)
        {
            var i = 0;
            while (text[i] == ' ')
            {
                i++;
            }

            return i;
        }

        private static bool IsGrammarServerEnabled(IRestClient client)
        {
            var request = new RestRequest("check", Method.POST);
            request.AddParameter("language", "es");
            request.AddParameter("text", "Texto de prueba");

            var response = client.Execute(request);

            return response.ResponseStatus == ResponseStatus.Completed;
        }

        private static LanguageToolResponse CheckGrammar(IRestClient client, string text)
        {
            var request = new RestRequest("check", Method.POST);
            request.AddParameter("language", "es");
            request.AddParameter("text", text);

            var response = client.Execute(request);

            return client.Deserialize<LanguageToolResponse>(response).Data;
        }
    }
}
