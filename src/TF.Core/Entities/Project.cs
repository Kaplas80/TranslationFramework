using System;
using System.Collections.Generic;
using System.Linq;
using TF.Core.Persistence;

namespace TF.Core.Entities
{
    public abstract class Project
    {
        private List<TFString> _strings;

        public virtual string Path { get; set; }

        public abstract string CompatibleFilesFilter { get; }

        public IList<ITFFile> Files { get; private set; }

        public IList<TFString> Strings
        {
            get
            {
                if (_strings != null)
                {
                    return _strings;
                }

                _strings = new List<TFString>();
                foreach (var file in Files)
                {
                    _strings.AddRange(file.Strings);
                }

                return _strings;
            }
            set
            {
                if (_strings == null)
                {
                    _strings = new List<TFString>();
                }
                else
                {
                    _strings.Clear();
                }

                _strings.AddRange(value);
            }
        }
    
        protected Repository _repository;

        protected Project()
        {
            Files = new List<ITFFile>();
        }

        public virtual string GetConfigValue(string field)
        {
            return _repository.GetConfigValue(field);
        }

        public abstract void SetFile(string fileName);

        public void SaveStrings()
        {
            _repository.InsertStrings(Strings);
        }

        public void UpdateStrings()
        {
            _repository.UpdateStrings(Strings);
        }

        public void LoadStrings()
        {
            Strings = _repository.GetStrings();
        }

        public void Close()
        {
            _repository.Close();
        }

        public IList<Tuple<string, string>> GetCharReplacements()
        {
            return _repository.GetReplacements();
        }

        public virtual void Export(string selectedPath, ExportOptions options)
        {
            foreach (var file in Files)
            {
                var originalFilename = System.IO.Path.GetFileName(file.Path);
                var destFilename = System.IO.Path.Combine(selectedPath, originalFilename);

                var fileName = System.IO.Path.GetTempFileName();

                var strings = Strings.Where(x => x.FileId == file.Id).ToList();

                file.Save(fileName, strings, options);

                if (System.IO.File.Exists(destFilename))
                {
                    System.IO.File.Delete(destFilename);
                }

                System.IO.File.Move(fileName, destFilename);
            }

            _repository.UpdateConfigValue("OUTPUT_REPLACEMENT", options.CharReplacement.ToString());
            _repository.UpdateConfigValue("OUTPUT_ENCODING", options.SelectedEncoding.HeaderName.ToUpperInvariant());

            _repository.DeleteReplacements();
            _repository.InsertReplacements(options.CharReplacementList);
        }

        public abstract void LoadFiles();
    }
}
