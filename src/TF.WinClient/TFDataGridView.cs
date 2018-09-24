using System.Security.Permissions;
using System.Windows.Forms;

namespace TF.WinClient
{
    public class TFDataGridView : DataGridView
    {
        [SecurityPermission(
            SecurityAction.LinkDemand, Flags = 
                SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                return true;
            }
            return base.ProcessDataGridViewKey(e);
        }
    }
}
