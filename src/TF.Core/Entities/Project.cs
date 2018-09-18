using System;
using System.Collections.Generic;
using TF.Core.Persistence;

namespace TF.Core.Entities
{
    public abstract class Project
    {
        private IList<TFString> _strings;

        public virtual string Path { get; set; }

        public abstract string CompatibleFilesFilter { get; }

        public virtual ITFFile File { get; set; }

        public virtual IList<TFString> Strings {
            get => _strings ?? (_strings = File == null ? new List<TFString>() : File.Strings);
            set => _strings = value;
        }
    
        protected Repository _repository;

        protected Project()
        {

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

        public abstract void Export(string selectedPath, ExportOptions options);
        public abstract void LoadFile();
    }
}
