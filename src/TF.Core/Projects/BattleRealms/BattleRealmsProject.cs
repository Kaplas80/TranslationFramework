﻿using System.IO;
using System.Linq;
using TF.Core.Entities;
using TF.Core.Persistence;
using TF.Core.Projects.BattleRealms.Files;

namespace TF.Core.Projects.BattleRealms
{
    public class BattleRealmsProject : Project
    {
        public override string CompatibleFilesFilter => "Archivos de Texto de BattleRealms|*.lt?";

        public static string ReadingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\n", "\\n");
            
            return result;
        }

        public static string WritingReplacements(string input)
        {
            var result = input;

            result = result.Replace("\\n", "\n");
            
            return result;
        }

        protected BattleRealmsProject(Repository repository)
        {
            _repository = repository;
        }

        public static BattleRealmsProject GetProject(string path)
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
                repository.InsertConfig("OUTPUT_ENCODING", "UTF-16");
            }

            var result = new BattleRealmsProject(repository) {Path = path};
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
