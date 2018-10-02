using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.Yakuza0.Files;
using DbFile = TF.Core.Entities.DbFile;

namespace TF.Core.Projects.Yakuza0
{
    public class Yakuza0Project : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de Yakuza 0|*.bin_c;*.msg;cmn.bin;Yakuza0.exe;*.mfp";

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

            if (File.Exists(path))
            {
                repository = Repository.Open(path);
            }
            else
            {
                repository = Repository.Create(path);

                repository.InsertConfig("OUTPUT_REPLACEMENT", "0");
                repository.InsertConfig("OUTPUT_ENCODING", "ISO-8859-1");

                repository.InsertReplacement("á", "a");
                repository.InsertReplacement("é", "e");
                repository.InsertReplacement("í", "i");
                repository.InsertReplacement("ó", "o");
                repository.InsertReplacement("ú", "u");
                repository.InsertReplacement("ü", "u");

                repository.InsertReplacement("Á", "A");
                repository.InsertReplacement("É", "E");
                repository.InsertReplacement("Í", "I");
                repository.InsertReplacement("Ó", "O");
                repository.InsertReplacement("Ú", "U");
                repository.InsertReplacement("Ü", "U");

                repository.InsertReplacement("ñ", "n");
                repository.InsertReplacement("Ñ", "N");

                repository.InsertReplacement("¡", "!");
                repository.InsertReplacement("¿", "?");
            }

            var result = new Yakuza0Project(repository) {Path = path};
            return result;
        }

        public override void LoadFiles()
        {
            var files = _repository.GetFiles();

            foreach (var dbFile in files)
            {
                var fileContent = dbFile.Content;
                var fileName = dbFile.Path;

                var file = FileFactory.GetFile(fileName, fileContent);

                file.Id = dbFile.Id;
                
                using (var ms = new MemoryStream(fileContent))
                {
                    file.Read(ms);
                }

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

            var dbFile = new DbFile
            {
                Path = fileName, Hash = Utils.CalculateHash(fileName), Content = File.ReadAllBytes(fileName)
            };

            _repository.InsertFile(dbFile);

            file.Id = dbFile.Id;

            try
            {
                using (var ms = new MemoryStream(dbFile.Content))
                {
                    file.Read(ms);
                }
            }
            catch (Exception )
            {
                _repository.DeleteFile(file.Id);
                throw;
            }
            

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
