using System;
using System.Collections.Generic;
using System.Text;

namespace TF.Core
{
    public class ExportOptions
    {
        public int CharReplacement { get; set; }
        public Encoding SelectedEncoding { get; set; }
        public IList<Tuple<string, string>> CharReplacementList { get; set; }
    }
}