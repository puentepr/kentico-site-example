using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.CMSHelper;
using CMS.Community;
using CMS.EmailEngine;
using CMS.GlobalHelper;
using CMS.PortalEngine;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.ExtendedControls;

public partial class CMSModules_Groups_Controls_GroupInvite : CMSAdminControl
{
    #region "Private variables"

    private int mInvitedUserId = 0;
    private string mInviteText = null;
    private string mSuccessfulInviteText = null;
    private string mUnsuccessfulInviteText = null;
    private int mGroupId = 0;
    private GroupInfo mGroup = null;
    private UserInfo mInvitedUser = null;
    private bool mDisplayUserSelector = false;
    private bool mDisplayGroupSelector = true;
    private bool mAllowInviteNewUser = false;
    private bool mDisplayAdvancedOptions = false;
    private CMSButton mInvitationButton = null;
    private LocalizedButton mCancelButton = null;

    #endregion


    #region "Public properties"

    /// <summary>
    /// Gets or sets the text which should be displayed on invite dialog.
    /// </summary>
    public string InviteText
    {
        get
        {
            string username = Functions.GetFormattedUserName(UserInfoProvider.GetUserNameById(InvitedUserID), this.IsLiveSite);
            username = HTMLHelper.HTMLEncode(username);
            return DataHelper.GetNotEmpty(mInviteText, GetString("groupinvitation.invitationtext") + " " + username + "?");
        }
        set
        {
            mInviteText = value;
        }
    }


    /// <summary>
    /// Gets or sets the text which should be displayed on invite dialog after successful invite action.
    /// </summary>
    public string SuccessfulInviteText
    {
        get
        {
            string userNameText = InvitedUser == null ? GetString("general.user").ToLower() : HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(InvitedUser.UserName, this.IsLiveSite));
            return DataHelper.GetNotEmpty(mSuccessfulInviteText, GetString("groupinvitation.invitationsuccess").Replace("##GroupName##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##UserName##", userNameText));
        }
        set
        {
            mSuccessfulInviteText = value;
        }
    }


    /// <summary>
    /// Gets or sets the text which should be displayed on invite dialog if invite actin was unsuccessful.
    /// </summary>
    public string UnsuccessfulInviteText
    {
        get
        {
            return DataHelper.GetNotEmpty(mUnsuccessfulInviteText, GetString("groupinvitation.invitationunsuccess").Replace("##GroupName##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##UserName##", (InvitedUser != null) ? HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(InvitedUser.UserName, this.IsLiveSite)) : ""));
        }
        set
        {
            mUnsuccessfulInviteText = value;
        }
    }


    /// <summary>
    /// Gets or sets ID of the user to be invited.
    /// </summary>
    public int InvitedUserID
    {
        get
        {
            return mInvitedUserId;
        }
        set
        {
            mInvitedUserId = value;
        }
    }


    /// <summary>
    /// Gets or sets the group id.
    /// </summary>
    public int GroupID
    {
        get
        {
            return mGroupId;
        }
        set
        {
            mGroupId = value;
        }
    }


    /// <summary>
    /// Gets or sets whether display user selector.
    /// </summary>
    public bool DisplayUserSelector
    {
        get
        {
            return mDisplayUserSelector;
        }
        set
        {
            mDisplayUserSelector = value;
        }
    }


    /// <summary>
    /// Gets or sets whether display multiple user selector.
    /// </summary>
    public bool UseMultipleUserSelector
    {
        get
        {
            return (userSelector.SelectionMode == SelectionModeEnum.MultipleTextBox);
        }
        set
        {
            if (value)
            {
                userSelector.SelectionMode = SelectionModeEnum.MultipleTextBox;
            }
            else
            {
                userSelector.SelectionMode = SelectionModeEnum.SingleTextBox;
            }

        }
    }


    /// <summary>
    /// Gets or sets whether display group selector.
    /// </summary>
    public bool DisplayGroupSelector
    {
        get
        {
            return mDisplayGroupSelector;
        }
        set
        {
            mDisplayGroupSelector = value;
        }
    }


    /// <summary>
    /// Gets or sets whether allow invite new user.
    /// </summary>
    public bool AllowInviteNewUser
    {
        get
        {
            return mAllowInviteNewUser;
        }
        set
        {
            mAllowInviteNewUser = value;
        }
    }


    /// <summary>
    /// Gets or sets the group info object for destination group.
    /// </summary>
    public GroupInfo Group
    {
        get
        {
            return mGroup;
        }
        set
        {
            mGroup = value;
        }
    }


    /// <summary>
    /// Gets or sets the user info object for invited user.
    /// </summary>
    public UserInfo InvitedUser
    {
        get
        {
            return mInvitedUser;
        }
        set
        {
            mInvitedUser = value;
        }
    }


    /// <summary>
    /// Gets or sets whether display advanced options.
    /// </summary>
    public bool DisplayAdvancedOptions
    {
        get
        {
            return mDisplayAdvancedOptions;
        }
        set
        {
            mDisplayAdvancedOptions = value;
        }
    }


    /// <summary>
    /// Indicates if control buttons should be displayed.
    /// </summary>
    public bool DisplayButtons
    {
        get
        {
            return plcButtons.Visible;
        }
        set
        {
            plcButtons.Visible = value;
        }
    }


    /// <summary>
    /// Invitation button.
    /// </summary>
    public CMSButton InvitationButton
    {
        get
        {
            if (mInvitationButton == null)
            {
                mInvitationButton = btnInvite;
            }
            return mInvitationButton;
        }
        set
        {
            mInvitationButton = value;
        }
    }


    /// <summary>
    /// Cancel button.
    /// </summary>
    public LocalizedButton CancelButton
    {
        get
        {
            if (mCancelButton == null)
            {
                mCancelButton = btnCancel;
            }
            return mCancelButton;
        }
        set
        {
            mCancelButton = value;
        }
    }

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        ScriptHelper.RegisterWOpenerScript(Page);

        // Hide cancel button for administration interface
        if (!IsLiveSite)
        {
            CancelButton.Visible = false;
        }

        InvitationButton.Click += new EventHandler(btnInvite_Click);
        InvitationButton.Text = GetString("general.invite");
        CancelButton.OnClientClick = "Close();";
        lblInfo.Text = InviteText;

        if (!RequestHelper.IsPostBack())
        {
            InitializeGroupControl();
            lblInfo.Visible = DisplayGroupSelector;
        }
        revEmail.ErrorMessage = GetString("System_Email.ErrorEmail");
        rfvEmail.ErrorMessage = GetString("System_Email.EmptyEmail");
        revEmail.ValidationExpression = @"^([\w0-9_-]+(\.[\w0-9_-]+)*@[\w0-9_-]+(\.[\w0-9_-]+)+)*$";
        plcGroupSelector.Visible = DisplayGroupSelector;
        plcUserType.Visible = AllowInviteNewUser;
        plcEmail.Visible = radNewUser.Checked;
        plcUserSelector.Visible = DisplayUserSelector && radSiteMember.Checked;

        userSelector.IsLiveSite = this.IsLiveSite;
        userSelector.ShowSiteFilter = false;
        userSelector.HideHiddenUsers = true;
        userSelector.HideDisabledUsers = true;               
    }


    /// <summary>
    /// Invite button handling.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">EventArgs</param>
    protected void btnInvite_Click(object sender, EventArgs e)
    {
        if (!IsLiveSite)
        {
            if (!CheckPermissions("cms.groups", CMSAdminControl.PERMISSION_MANAGE, this.GroupID))
            {
                return;
            }
        }

        if (DisplayGroupSelector)
        {
            GroupID = ValidationHelper.GetInteger(groupSelector.Value, 0);
        }

        Group = GroupInfoProvider.GetGroupInfo(GroupID);
        string recipientEmail = string.Empty;

        if (Group != null)
        {
            // Check whether user is group member
            if (CMSContext.CurrentUser.IsGroupMember(GroupID))
            {
                // If user can select the user
                if (plcUserSelector.Visible)
                {
                    int userId = ValidationHelper.GetInteger(userSelector.Value, 0);
                    if (userId > 0)
                    {
                        InvitedUser = UserInfoProvider.GetUserInfo(userId);

                        bool userNotFound = true;

                        // Check if user is filtered
                        if (InvitedUser != null)
                        {
                            if (this.IsLiveSite)
                            {
                                userNotFound = InvitedUser.UserIsHidden || !InvitedUser.UserEnabled;
                            }
                            else
                            {
                                userNotFound = false;
                            }
                        }

                        if (!userNotFound)
                        {
                            // Create invitation info
                            InvitationInfo ii = InviteUser(Group, InvitedUser);
                            if (ii != null)
                            {
                                // Send e-mail
                                InvitationInfoProvider.SendInvitationEmail(ii, CMSContext.CurrentSiteName);
                            }
                            else
                            {
                                // User is member
                                GroupMemberInfo gmi = GroupMemberInfoProvider.GetGroupMemberInfo(InvitedUser.UserID, GroupID);
                                if (gmi != null)
                                {
                                    lblError.Text = GetString("groupinvitation.invitationunsuccessismember").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(InvitedUser.UserName, this.IsLiveSite)));
                                    lblError.Visible = true;
                                    return;
                                }

                                // User is invited
                                DataSet ds = InvitationInfoProvider.GetInvitations("InvitedUserID=" + InvitedUser.UserID + " AND InvitationGroupID=" + GroupID, "InvitationCreated", 1);
                                if (!DataHelper.DataSourceIsEmpty(ds))
                                {
                                    int invitedByuserId = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["InvitedByUserID"], 0);
                                    UserInfo ui = UserInfoProvider.GetUserInfo(invitedByuserId);
                                    if (ui != null)
                                    {
                                        lblError.Text = GetString("groupinvitation.invitationunsuccesinvexists").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", HTMLHelper.HTMLEncode(InvitedUser.UserName)).Replace("##INVITEDBY##", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(ui.UserName, this.IsLiveSite)));
                                        lblError.Visible = true;
                                    }
                                    return;
                                }

                                // General error
                                lblError.Text = GetString("groupinvitation.invitationunsuccessmult").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(InvitedUser.UserName, this.IsLiveSite)));
                                lblError.Visible = true;
                                return;
                            }
                        }
                        else
                        {
                            lblError.Text = GetString("general.usernotfound");
                            lblError.Visible = true;
                            return;
                        }
                    }
                    else
                    {
                        lblError.Text = GetString("groupinvitation.emptyusers");
                        lblError.Visible = true;
                        return;
                    }

                    // Succesfull invitation
                    lblInfo.Text = SuccessfulInviteText;
                    lblInfo.Visible = true;

                    DisableAfterSuccess();

                    if (DisplayAdvancedOptions)
                    {
                        CancelButton.ResourceString = "general.back";
                        CancelButton.PostBackUrl =
                            ResolveUrl("~/CMSModules/Groups/Tools/Members/Member_List.aspx?groupId=" + GroupID);
                    }
                    else
                    {
                        CancelButton.ResourceString = "general.close";
                        CancelButton.OnClientClick = "Close();";
                    }
                }
                // Single invite
                else
                {
                    // Check the email address if it is new user
                    if (radNewUser.Checked)
                    {
                        string result = new Validator().IsEmail(txtEmail.Text, rfvEmail.ErrorMessage).Result;
                        if (result != String.Empty)
                        {
                            lblError.Text = result;
                            lblError.Visible = true;
                            return;
                        }
                    }

                    InvitedUser = UserInfoProvider.GetUserInfo(InvitedUserID);

                    if ((GroupID != 0) || (InvitedUser != null) || (radNewUser.Checked))
                    {
                        // Create invitation info
                        InvitationInfo ii = InviteUser(Group, InvitedUser);

                        if (ii != null)
                        {
                            lblInfo.Text = SuccessfulInviteText;
                            DisableAfterSuccess();
                            if (DisplayAdvancedOptions)
                            {
                                CancelButton.ResourceString = "general.back";
                                CancelButton.PostBackUrl =
                                    ResolveUrl("~/CMSModules/Groups/Tools/Members/Member_List.aspx?groupId=" + GroupID);
                            }
                            else
                            {
                                CancelButton.ResourceString = "general.close";
                                if (UseMultipleUserSelector)
                                {
                                    CancelButton.OnClientClick = "Close();";
                                }
                                else
                                {
                                    CancelButton.OnClientClick = "CloseAndRefresh();";
                                }
                            }

                            InvitationInfoProvider.SendInvitationEmail(ii, CMSContext.CurrentSiteName);

                            // Succesfull invitation
                            lblInfo.Text = SuccessfulInviteText;
                            lblInfo.Visible = true;
                        }
                        else
                        {
                            if (InvitedUser != null)
                            {
                                string username = HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(InvitedUser.UserName, this.IsLiveSite));

                                // User is member
                                GroupMemberInfo gmi = GroupMemberInfoProvider.GetGroupMemberInfo(InvitedUser.UserID, GroupID);
                                if (gmi != null)
                                {
                                    lblError.Text = GetString("groupinvitation.invitationunsuccessismember").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", username);
                                    lblError.Visible = true;
                                    return;
                                }

                                // User is invited
                                DataSet ds = InvitationInfoProvider.GetInvitations("InvitedUserID=" + InvitedUser.UserID + " AND InvitationGroupID=" + GroupID, "InvitationCreated", 1);
                                if (!DataHelper.DataSourceIsEmpty(ds))
                                {
                                    int invitedByuserId = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["InvitedByUserID"], 0);
                                    UserInfo ui = UserInfoProvider.GetUserInfo(invitedByuserId);
                                    if (ui != null)
                                    {
                                        lblError.Text = GetString("groupinvitation.invitationunsuccesinvexists").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", username).Replace("##INVITEDBY##", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(ui.UserName, this.IsLiveSite)));
                                        lblError.Visible = true;
                                    }
                                    return;
                                }

                                // General error
                                lblError.Text = GetString("groupinvitation.invitationunsuccessmult").Replace("##GROUPNAME##", HTMLHelper.HTMLEncode(Group.GroupDisplayName)).Replace("##USERNAME##", username);
                                lblError.Visible = true;
                                return;
                            }
                            else
                            {
                                UnSuccess();
                            }
                        }
                    }
                    else
                    {
                        UnSuccess();
                    }
                }
            }
        }
    }


    /// <summary>
    /// Disables controls after successful invitation.
    /// </summary>
    private void DisableAfterSuccess()
    {
        // Disable controls
        InvitationButton.Enabled = false;
        userSelector.Enabled = false;
        txtComment.Enabled = false;
        txtEmail.Enabled = false;
        radNewUser.Enabled = false;
        radSiteMember.Enabled = false;
        groupSelector.CurrentSelector.Enabled = false;
    }


    /// <summary>
    /// Displays error message.
    /// </summary>
    private void UnSuccess()
    {
        // Display error message
        lblError.Text = UnsuccessfulInviteText;
        lblError.Visible = true;
        return;
    }


    /// <summary>
    /// Loads member's groups control.
    /// </summary>
    protected void InitializeGroupControl()
    {
        InvitationButton.Enabled = true;
        groupSelector.CurrentSelector.ReturnColumnName = "GroupID";

        if (CMSContext.CurrentSite != null)
        {
            CurrentUserInfo ui = CMSContext.CurrentUser;
            bool isAdmin = (ui.IsGlobalAdministrator || ui.IsAuthorizedPerResource("CMS.Groups", CMSAdminControl.PERMISSION_MANAGE));

            // Get dataset with members
            string where = "GroupSiteID = " + CMSContext.CurrentSite.SiteID.ToString();
            if (!isAdmin)
            {
                where += " AND GroupID IN (SELECT MemberGroupID FROM Community_GroupMember WHERE MemberUserID = " + ui.UserID + ")";
            }

            groupSelector.CurrentSelector.WhereCondition = where;

            if (!groupSelector.CurrentSelector.HasData)
            {
                // User is not member of any group
                lblInfo.Text = GetString("groupinvitation.nogroup");
                DisableAfterSuccess();
                CancelButton.ResourceString = "general.close";
                return;
            }
        }
    }


    /// <summary>
    /// Creates invitation for specified user.
    /// </summary>
    /// <param name="groupID">Inviting group</param>
    /// <param name="invitedUser">User info</param>    
    private InvitationInfo InviteUser(GroupInfo gi, UserInfo invitedUser)
    {
        // Check whether group exists        
        if (gi == null)
        {
            return null;
        }

        int userId = (invitedUser != null) ? invitedUser.UserID : 0;
        int currentUserId = CMSContext.CurrentUser.UserID;

        // User cannot invite herself
        if (userId == currentUserId)
        {
            return null;
        }

        // Check whether user is in group or is invited
        if ((!GroupMemberInfoProvider.IsMemberOfGroup(userId, Group.GroupID) && !InvitationInfoProvider.InvitationExists(userId, Group.GroupID)) || radNewUser.Checked)
        {
            // Create new info object
            InvitationInfo ii = new InvitationInfo();
            ii.InvitationComment = txtComment.Text;
            ii.InvitationCreated = DateTime.Now;
            ii.InvitationGroupID = GroupID;
            ii.InvitationGUID = new Guid();
            ii.InvitationLastModified = DateTime.Now;
            ii.InvitedByUserID = currentUserId;

            // Create 'Valid to' value if set in settings
            int validTo = SettingsKeyProvider.GetIntValue(CMSContext.CurrentSiteName + ".CMSInvitationValidity");
            if (validTo > 0)
            {
                ii.InvitationValidTo = DateTime.Now.AddDays(validTo);
            }

            if (radSiteMember.Checked)
            {
                ii.InvitedUserID = userId;
            }
            else
            {
                ii.InvitationUserEmail = txtEmail.Text;
            }
            InvitationInfoProvider.SetInvitationInfo(ii);
            return ii;
        }
        return null;
    }
}
