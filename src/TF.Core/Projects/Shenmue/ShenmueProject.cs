using System.Linq;
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

                repository.InsertConfig("OUTPUT_REPLACEMENT", "1");
                repository.InsertConfig("OUTPUT_ENCODING", "UTF-8");

                repository.InsertReplacement("á", "à");
                repository.InsertReplacement("í", "ï");
                repository.InsertReplacement("ó", "ô");
                repository.InsertReplacement("ú", "ù");
                repository.InsertReplacement("ü", "ë");

                repository.InsertReplacement("Ú", "û");

                repository.InsertReplacement("ñ", "â");

                repository.InsertReplacement("¡", "Î");
                repository.InsertReplacement("¿", "Ê");
            }

            var result = new ShenmueProject(repository) {Path = path};
            return result;
        }

        public override void LoadFiles()
        {
            var files = _repository.GetFiles();

            foreach (var dbFile in files)
            {
                var fileName = dbFile.Path;

                var file = FileFactory.GetFile(fileName);

                if (file == null)
                {
                    throw new TFUnknownFileTypeException("No se reconoce el tipo del fichero");
                }

                var storedHash = dbFile.Hash;
                var hash = Utils.CalculateHash(fileName);

                if (string.Compare(storedHash, hash) != 0)
                {
                    throw new TFChangedFileException($"El fichero {fileName} ha cambiado. Debes crear una traducción nueva.");
                }
                file.Id = dbFile.Id;
                file.Read();

                Files.Add(file);
            }
        }

        public override void SetFile(string fileName)
        {
            var file = FileFactory.GetFile(fileName);

            if (file == null)
            {
                throw new TFUnknownFileTypeException("No se reconoce el tipo del fichero");
            }

            var dbFile = new DbFile {Path = fileName, Hash = Utils.CalculateHash(fileName)};
            _repository.InsertFile(dbFile);

            file.Id = dbFile.Id;

            file.Read();

            if (file.Strings.Count(x => x.Visible) > 0)
            {
                Files.Add(file);
            }
            else
            {
                _repository.DeleteFile(file.Id);
            }
        }

    }
}
