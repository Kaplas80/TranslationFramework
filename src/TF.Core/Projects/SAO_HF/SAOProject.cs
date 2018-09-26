using System.IO;
using System.Linq;
using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.SAO_HF.Files;

namespace TF.Core.Projects.SAO_HF
{
    public class SAOProject : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de SAO_HF|*.*";

        public static string ReadingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\n", "\\n");
            
            result = result.Replace("\u0010", "#0x10#");
            result = result.Replace("\u0011", "#0x11#");
            
            result = result.Replace("\u0013", "#0x13#");
            result = result.Replace("\u0014", "#0x14#");

            return result;
        }

        public static string WritingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\\n", "\n");
            
            result = result.Replace("#0x10#", "\u0010");
            result = result.Replace("#0x11#", "\u0011");
            
            result = result.Replace("#0x13#", "\u0013");
            result = result.Replace("#0x14#", "\u0014");
            
            return result;
        }

        protected SAOProject(Repository repository)
        {
            _repository = repository;
        }

        public static SAOProject GetProject(string path)
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
                repository.InsertConfig("OUTPUT_ENCODING", "UTF-8");
            }

            var result = new SAOProject(repository) {Path = path};
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
