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
using System.Data.SqlTypes;

using CMS.GlobalHelper;
using CMS.WebAnalytics;
using CMS.CMSHelper;
using CMS.UIControls;
using CMS.LicenseProvider;
using CMS.SettingsProvider;
using CMS.FormControls;

public partial class CMSModules_Reporting_WebAnalytics_Analytics_ManageData : CMSToolsModalPage
{
    #region "Variables"

    const string VISIT_CODE_NAME = "visitors";
    const string MULTILINGUAL_SUFFIX = ".multilingual";
    string statCodeName = String.Empty;

    FormEngineUserControl ucABTests = null;
    FormEngineUserControl ucMVTests = null;

    #endregion 

    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Check license
        if (DataHelper.GetNotEmpty(URLHelper.GetCurrentDomain(), "") != "")
        {
            LicenseHelper.CheckFeatureAndRedirect(URLHelper.GetCurrentDomain(), FeatureEnum.WebAnalytics);
        }

        ucABTests = LoadControl("~/CMSModules/OnlineMarketing/FormControls/SelectABTest.ascx") as FormEngineUserControl;
        ucMVTests = LoadControl("~/CMSModules/OnlineMarketing/FormControls/SelectMVTest.ascx") as FormEngineUserControl;

        if (ucABTests != null)
        {
            ucABTests.ID = "abTestSelector";
            pnlABTests.Controls.Add(ucABTests);
        }

        if (ucMVTests != null)
        {
            ucMVTests.ID = "mvtSelector";
            pnlMVTests.Controls.Add(ucMVTests);
        }

        if (!RequestHelper.IsPostBack() && (ucABTests != null) && (ucMVTests != null))
        {
            string[,] fields = new string[2, 2];
            fields[0, 0] = GetString("general.pleaseselect");
            fields[0, 1] = "pleaseselect";

            fields[1, 0] = "(" + GetString("general.all") + ")";
            fields[1, 1] = ValidationHelper.GetString(ucABTests.GetValue("AllRecordValue"), String.Empty);

            ucABTests.SetValue("SpecialFields", fields);
            ucABTests.Value = "pleaseselect";
            ucABTests.SetValue("AllowEmpty", false);
            ucABTests.SetValue("ReturnColumnName", "ABTestName");            

            ucMVTests.SetValue("SpecialFields", fields);
            ucMVTests.Value = "pleaseselect";
            ucMVTests.SetValue("AllowEmpty", false);
            ucMVTests.SetValue("ReturnColumnName", "MVTestName");

            usCampaigns.UniSelector.SpecialFields = fields;
            usCampaigns.Value = "pleaseselect";
        }

        CurrentUserInfo user = CMSContext.CurrentUser;

        // Check permissions for CMS Desk -> Tools -> Web analytics tab
        if (!user.IsAuthorizedPerUIElement("CMS.Tools", "WebAnalytics"))
        {
            RedirectToCMSDeskUIElementAccessDenied("CMS.Tools", "WebAnalytics");
        }

        // Check 'Read' permission
        if (!user.IsAuthorizedPerResource("CMS.WebAnalytics", "Read"))
        {
            RedirectToCMSDeskAccessDenied("CMS.WebAnalytics", "Read");
        }

        string title = GetString("AnayticsManageData.ManageData");
        this.Page.Title = title;
        this.CurrentMaster.Title.TitleText = title;
        this.CurrentMaster.Title.TitleImage = GetImageUrl("CMSModules/CMS_Reporting/managedata.png");

        // Confirmation message for deleting
        string deleteFromToMessage = ScriptHelper.GetString(GetString("webanal.deletefromtomsg"));
        deleteFromToMessage = deleteFromToMessage.Replace("##FROM##", "' + elemFromStr + '");
        deleteFromToMessage = deleteFromToMessage.Replace("##TO##", "' + elemToStr + '");

        string script =
            " var elemTo = document.getElementById('" + pickerTo.ClientID + "_txtDateTime'); " +
            " var elemFrom = document.getElementById('" + pickerFrom.ClientID + "_txtDateTime'); " +
            " var elemToStr = " + ScriptHelper.GetString(GetString("webanal.now")) + "; " +
            " var elemFromStr = " + ScriptHelper.GetString(GetString("webanal.beginning")) + "; " +
            " var deleteAll = 1; " +
            " if (elemTo.value != '') { deleteAll = 0; elemToStr = elemTo.value; }; " +
            " if (elemFrom.value != '') { deleteAll = 0; elemFromStr = elemFrom.value; }; " +
            " if (deleteAll == 1) { return confirm(" + ScriptHelper.GetString(GetString("webanal.deleteall")) + "); } " +
            " else { return confirm(" + deleteFromToMessage + "); }; ";
        btnDelete.OnClientClick = script + ";  return false;";

        statCodeName = QueryHelper.GetString("statCodeName", String.Empty);

        if (statCodeName == "abtest")
        {
            pnlABTestSelector.Visible = true;
        }

        if (statCodeName == "mvtest")
        {
            pnlMVTSelector.Visible = true;
        }

        if (statCodeName == "campaigns")
        {
            pnlCampaigns.Visible = true;
        }
    }


    public void btnDelete_Click(object sender, EventArgs e)
    {
        // Check 'ManageData' permission
        if (!CMSContext.CurrentUser.IsAuthorizedPerResource("CMS.WebAnalytics", "ManageData"))
        {
            RedirectToCMSDeskAccessDenied("CMS.WebAnalytics", "ManageData");
        }

        if (statCodeName == String.Empty)
        {
            return;
        }

        DateTime fromDate = pickerFrom.SelectedDateTime;
        DateTime toDate = pickerTo.SelectedDateTime;
        bool invalidFromDate = false;
        bool invalidToDate = false;

        // Use min/max values if not selected
        if (fromDate == DateTimeHelper.ZERO_TIME)
        {
            fromDate = (DateTime)SqlDateTime.MinValue;
            // Because of operations with datetime add year to minimum
            fromDate = fromDate.AddYears(1);
            // Check if user typed invalid date
            invalidFromDate = pickerFrom.DateTimeTextBox.Text.Trim() != "";
        }

        if (toDate == DateTimeHelper.ZERO_TIME)
        {
            toDate = (DateTime)SqlDateTime.MaxValue;
            // Because of operations in query substract year to prevent overflow
            toDate = toDate.AddYears(-1);
            // Check if user typed invalid date
            invalidToDate = pickerTo.DateTimeTextBox.Text.Trim() != "";
        }

        if (!pickerFrom.IsValidRange() || !pickerTo.IsValidRange())
        {
            lblError.Text = GetString("general.errorinvaliddatetimerange");
            lblError.Visible = true;
            return;
        }

        if (invalidFromDate)
        {
            lblError.Visible = true;
            lblError.Text = GetString("analt.invalidfromdate");
            return;
        }

        if (invalidToDate)
        {
            lblError.Visible = true;
            lblError.Text = GetString("analt.invalidtodate");
            return;
        }

        if (fromDate > toDate)
        {
            lblError.Visible = true;
            lblError.Text = GetString("analt.invalidinterval");
            return;
        }

        String where = String.Empty;
        // Manage A/B test selector
        if ((statCodeName == "abtest") && (ucABTests != null))
        {
            string abTest = ValidationHelper.GetString(ucABTests.Value, String.Empty);
            if ((abTest == String.Empty) || (abTest == "pleaseselect"))
            {
                lblError.Visible = true;
                lblError.Text = GetString("abtest.pleaseselect");
                return;
            }

            String codeName = (abTest == ValidationHelper.GetString(ucABTests.GetValue("AllRecordValue"), String.Empty)) ? "'abconversion;%'" : "'abconversion;" + abTest + ";%'";
            where = "StatisticsCode LIKE " + codeName;
        }

        // Manage MV test selector
        if ((statCodeName == "mvtest") && (ucMVTests != null))
        {
            string mvTest = ValidationHelper.GetString(ucMVTests.Value, String.Empty);
            if ((mvTest == String.Empty) || (mvTest == "pleaseselect"))
            {
                lblError.Visible = true;
                lblError.Text = GetString("mvtest.pleaseselect");
                return;
            }

            String codeName = (mvTest == ValidationHelper.GetString(ucMVTests.GetValue("AllRecordValue"), String.Empty)) ? "'mvtconversion;%'" : "'mvtconversion;" + mvTest + ";%'";
            where = "StatisticsCode LIKE " + codeName;
        }

        // Manage campaigns
        if (statCodeName == "campaigns")
        {
            string campaign = ValidationHelper.GetString(usCampaigns.Value, String.Empty);
            if ((campaign == String.Empty) || (campaign == "pleaseselect"))
            {
                lblError.Visible = true;
                lblError.Text = GetString("campaigns.pleaseselect");
                return;
            }

            if (campaign == usCampaigns.AllRecordValue)
            {
                where = "StatisticsCode='campaign' OR StatisticsCode LIKE 'campconversion;%'";
            }
            else
            {
                where = "(StatisticsCode='campaign' AND StatisticsObjectName ='" + campaign + "') OR StatisticsCode LIKE 'campconversion;" + campaign + "'";
            }
        }

        // Delete one campaign (set from url)
        if (statCodeName.StartsWith("singlecampaign"))
        {
            string[] arr = statCodeName.Split(';');
            if (arr.Length == 2)
            {
                String campaign = arr[1];
                where = "(StatisticsCode='campaign' AND StatisticsObjectName ='" + campaign + "') OR StatisticsCode LIKE 'campconversion;" + campaign + "'";
            }
        }

        // Add closing script
        ltlScript.Text = ScriptHelper.GetScript(" window.close(); wopener.RefreshPage(); ");

        // Ingore multilingual suffix (multilingual stats use the same data as "base" stats)
        if (statCodeName.ToLower().EndsWith(MULTILINGUAL_SUFFIX))
        {
            statCodeName = statCodeName.Remove(statCodeName.Length - MULTILINGUAL_SUFFIX.Length);
        }

        // Add where condition based on stat code name
        if (where == String.Empty)
        {
            where = "StatisticsCode='" + statCodeName + "'";
        }

        // In case of any error - (this page don't allow deleting all statistics)
        if (where == String.Empty)
        {
            return;
        }

        // Stats for visitors needs special manipulation (it consist of two types
        // of statistics with different code names - new visitor and returning visitor)
        if (statCodeName.ToLower() != HitLogProvider.VISITORS_FIRST)
        {
            StatisticsInfoProvider.RemoveAnalyticsData(fromDate, toDate, CMSContext.CurrentSiteID, where);
        }
        else
        {
            where = "StatisticsCode = '" + HitLogProvider.VISITORS_FIRST + "' OR StatisticsCode ='" + HitLogProvider.VISITORS_RETURNING + "'";
            StatisticsInfoProvider.RemoveAnalyticsData(fromDate, toDate, CMSContext.CurrentSiteID, where);
        }
    }

    #endregion
}

