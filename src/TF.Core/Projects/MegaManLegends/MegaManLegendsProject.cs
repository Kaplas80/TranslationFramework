using System.IO;
using System.Linq;
using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.MegaManLegends.Files;

namespace TF.Core.Projects.MegaManLegends
{
    public class MegaManLegendsProject : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de MegaMan Legends|*.bin";

        public static string ReadingReplacements(string input)
        {
            var result = input;

            //result = result.Replace("\n", "\\n");
            //No aplica, el cambio tiene que hacerse a nivel de bytes
            
            return result;
        }

        public static string WritingReplacements(string input)
        {
            var result = input;

            //result = result.Replace("\\n", "\n");
            //No aplica, el cambio tiene que hacerse a nivel de bytes
            
            return result;
        }

        protected MegaManLegendsProject(Repository repository)
        {
            _repository = repository;
        }

        public static MegaManLegendsProject GetProject(string path)
        {
            Repository repository;

            if (File.Exists(path))
            {
                repository = Repository.Open(path);
            }
            else
            {
                repository = Repository.Create(path);

                repository.InsertConfig("OUTPUT_REPLACEMENT", "1");
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

            var result = new MegaManLegendsProject(repository) {Path = path};
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
                
                using (var ms = new MemoryStream(dbFile.Content))
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

            using (var ms = new MemoryStream(dbFile.Content))
            {
                file.Read(ms);
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
