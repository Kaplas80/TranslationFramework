using System.Windows.Forms;

namespace TF.WinClient
{
    public class TFDataGridView : DataGridView
    {
        [System.Security.Permissions.SecurityPermission(
            System.Security.Permissions.SecurityAction.LinkDemand, Flags = 
                System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
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
