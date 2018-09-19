using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.Shenmue.Files;

namespace TF.Core.Projects.Shenmue
{
    public class ShenmueProject : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de Shenmue|*.sub";

        public static string ReadingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\ufffd\ufffd", "\\r\\n");

            result = result.Replace("Ê", "¿");
            result = result.Replace("Î", "¡");

            result = result.Replace("à", "á");
            result = result.Replace("ï", "í");
            result = result.Replace("ô", "ó");
            result = result.Replace("ù", "ú");
            result = result.Replace("û", "Ú");

            result = result.Replace("ë", "ü");
            result = result.Replace("â", "ñ");

            return result;
        }

        public static string WritingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\\r\\n", "\r\n");

            return result;
        }

        protected ShenmueProject(Repository repository)
        {
            _repository = repository;
        }

        public static ShenmueProject GetProject(string path)
        {
            Repository repository;

            if (System.IO.File.Exists(path))
            {
                repository = Repository.Open(path);
            }
            else
            {
                repository = Repository.Create(path);
            }

            var result = new ShenmueProject(repository) {Path = path};
            return result;
        }

        public override void LoadFile()
        {
            var fileName = _repository.GetConfigValue("PATH");

            var file = FileFactory.GetFile(fileName);

            if (file == null)
            {
                throw new TFUnknownFileTypeException("No se reconoce el tipo del fichero");
            }

            var storedHash = _repository.GetConfigValue("SHA1");
            var hash = Utils.CalculateHash(fileName);

            if (string.Compare(storedHash, hash) != 0)
            {
                throw new TFChangedFileException($"El fichero {fileName} ha cambiado. Debes crear una traducción nueva.");
            }

            file.Read();

            File = file;
        }

        public override void SetFile(string fileName)
        {
            var file = FileFactory.GetFile(fileName);

            if (file == null)
            {
                throw new TFUnknownFileTypeException("No se reconoce el tipo del fichero");
            }

            _repository.InsertConfig("PATH", fileName);
            _repository.InsertConfig("SHA1", Utils.CalculateHash(fileName));
            _repository.InsertConfig("OUTPUT_REPLACEMENT", "1");
            _repository.InsertConfig("OUTPUT_ENCODING", "UTF-8");

            _repository.InsertReplacement("á", "à");
            _repository.InsertReplacement("í", "ï");
            _repository.InsertReplacement("ó", "ô");
            _repository.InsertReplacement("ú", "ù");
            _repository.InsertReplacement("ü", "ë");

            _repository.InsertReplacement("Ú", "û");

            _repository.InsertReplacement("ñ", "â");

            _repository.InsertReplacement("¡", "Î");
            _repository.InsertReplacement("¿", "Ê");

            file.Read();

            File = file;
        }

        public override void Export(string selectedPath, ExportOptions options)
        {
            var originalFilename = System.IO.Path.GetFileName(File.Path);
            var destFilename = System.IO.Path.Combine(selectedPath, originalFilename);

            var fileName = System.IO.Path.GetTempFileName();

            File.Save(fileName, Strings, options);

            if (System.IO.File.Exists(destFilename))
            {
                System.IO.File.Delete(destFilename);
            }

            System.IO.File.Move(fileName, destFilename);

            _repository.UpdateConfigValue("OUTPUT_REPLACEMENT", options.CharReplacement.ToString());
            _repository.UpdateConfigValue("OUTPUT_ENCODING", options.SelectedEncoding.HeaderName.ToUpperInvariant());

            _repository.DeleteReplacements();
            _repository.InsertReplacements(options.CharReplacementList);
        }
    }
}
