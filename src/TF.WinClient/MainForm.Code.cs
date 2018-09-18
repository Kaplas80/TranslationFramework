using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TF.Core;
using TF.Core.Entities;

namespace TF.WinClient
{
    public partial class MainForm
    {
        private Project _openProject;

        private string GetProjectFilesFilter()
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

        private void CreateNewProject()
        {
            CloseProject();

            SaveProjectFileDialog.Filter = GetProjectFilesFilter();

            var result = SaveProjectFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                try
                {
                    var i = 0;
                    while (System.IO.File.Exists(SaveProjectFileDialog.FileName) && i < 10)
                    {
                        try
                        {
                            System.IO.File.Delete(SaveProjectFileDialog.FileName);
                        }
                        catch (Exception e)
                        {
                            i++;
                        }
                    }

                    if (System.IO.File.Exists(SaveProjectFileDialog.FileName))
                    {
                        MessageBox.Show($"No se ha podido sobreescribir el fichero", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    var project = ProjectFactory.GetProject(SaveProjectFileDialog.FileName);

                    if (AddFileToProject(project))
                    {
                        project.SaveStrings();

                        _openProject = project;

                        LoadDataGrid();

                        Text = $"Translation Framework - {_openProject.Path}";
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido crear el proyecto.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenProject()
        {
            CloseProject();
            
            OpenProjectFileDialog.Filter = GetProjectFilesFilter();

            var result = OpenProjectFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                try
                {
                    var project = ProjectFactory.GetProject(OpenProjectFileDialog.FileName);

                    project.LoadFile();

                    project.LoadStrings();

                    _openProject = project;

                    LoadDataGrid();

                    Text = $"Translation Framework - {_openProject.Path}";
                }
                catch (Exception e)
                {
                    MessageBox.Show($"No se ha podido abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            Text = $"Translation Framework";
        }

        private bool AddFileToProject(Project p)
        {
            var fileFilter = p.CompatibleFilesFilter;

            AddFileToProjectDialog.Filter = fileFilter;

            var result = AddFileToProjectDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                try
                {
                    p.SetFile(AddFileToProjectDialog.FileName);
                }
                catch (TFUnknownFileTypeException e)
                {
                    MessageBox.Show($"Error al abrir el fichero.\r\n{e.GetType()}: {e.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void LoadDataGrid()
        {
            foreach (var tfString in _openProject.Strings)
            {
                if (tfString.Visible)
                {
                    var row = StringsDataGrid.Rows.Add(tfString.Id, tfString.Section, tfString.Offset.ToString("X8"),
                        tfString.Original, tfString.Translation);

                    StringsDataGrid.Rows[row].Tag = tfString;
                }
            }
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
                        _openProject.Export(ExportProjectFolderBrowserDialog.SelectedPath, options);
                    }
                }
            }
        }
    }
}
