using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.Yakuza0.Files;

namespace TF.Core.Projects.Yakuza0
{
    public class Yakuza0Project : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de Yakuza 0|*.bin_?;*.msg;cmn.bin;Yakuza0.exe";

        public static string ReadingReplacements(string input)
        {
            var result = input;

            result = result.Replace('\u007F', '\u00AE');
            result = result.Replace("~", "™");
            //result = result.Replace("%", "^");
            result = result.Replace("\\", "¥");
            result = result.Replace("\r\n", "\\r\\n");
            result = result.Replace("\n", "\\n");

            return result;
        }

        public static string WritingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\\r\\n", "\r\n");
            result = result.Replace("\\n", "\n");
            
            result = result.Replace("™", "\u007F");

            return result;
        }

        protected Yakuza0Project(Repository repository)
        {
            _repository = repository;
        }

        public static Yakuza0Project GetProject(string path)
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

            var result = new Yakuza0Project(repository) {Path = path};
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
            _repository.InsertConfig("OUTPUT_REPLACEMENT", "0");
            _repository.InsertConfig("OUTPUT_ENCODING", "ISO-8859-1");

            _repository.InsertReplacement("á", "a");
            _repository.InsertReplacement("é", "e");
            _repository.InsertReplacement("í", "i");
            _repository.InsertReplacement("ó", "o");
            _repository.InsertReplacement("ú", "u");
            _repository.InsertReplacement("ü", "u");

            _repository.InsertReplacement("Á", "A");
            _repository.InsertReplacement("É", "E");
            _repository.InsertReplacement("Í", "I");
            _repository.InsertReplacement("Ó", "O");
            _repository.InsertReplacement("Ú", "U");
            _repository.InsertReplacement("Ü", "U");

            _repository.InsertReplacement("ñ", "n");
            _repository.InsertReplacement("Ñ", "N");

            _repository.InsertReplacement("¡", "!");
            _repository.InsertReplacement("¿", "?");

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
