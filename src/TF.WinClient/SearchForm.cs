using System.Windows.Forms;

namespace TF.WinClient
{
    public partial class SearchForm : Form
    {
        public string SearchText => TextToSearch.Text;
        public bool UseCaps => UseCapitalization.Checked;

        public SearchForm()
        {
            InitializeComponent();
        }
    }
}
