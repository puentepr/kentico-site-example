using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using CMS.GlobalHelper;
using CMS.CMSImportExport;

public partial class CMSModules_ImportExport_Controls_Import_cms_resource : ImportExportControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (CheckVersion())
        {
            pnlWarning.Visible = true;
            lblWarning.Text = GetString("ImportObjects.WarningObjectVersion");
        }
    }
}
