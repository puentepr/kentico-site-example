<%@ Page Language="C#" AutoEventWireup="true"
    Inherits="CMSModules_SmartSearch_SearchIndex_Content_List"
    MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Title="Search Index - Content List" Theme="Default" CodeFile="SearchIndex_Content_List.aspx.cs" %>

<%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/SearchIndex_Content_List.ascx"
    TagName="ContentList" TagPrefix="cms" %>
    <%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/SearchIndex_Forum_List.ascx"
    TagName="ForumList" TagPrefix="cms" %>
    <%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/SearchIndex_User.ascx"
    TagName="UserList" TagPrefix="cms" %>
    <%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/SearchIndex_CustomTable_List.ascx"
    TagName="CustomTableList" TagPrefix="cms" %>
    <%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/General_List.ascx"
    TagName="GeneralList" TagPrefix="cms" %>
    <%@ Register Src="~/CMSModules/SmartSearch/Controls/UI/SearchIndex_Custom.ascx"
    TagName="CustomList" TagPrefix="cms" %>
    
<asp:Content ContentPlaceHolderID="plcContent" runat="server">
    <cms:ContentList ID="contentList" runat="server" Visible="false" StopProcessing="true"  />
    <cms:ForumList ID="forumList" runat="server" Visible="false" StopProcessing="true" />
    <cms:UserList ID="userList" runat="server" Visible="false" StopProcessing="true" />
    <cms:CustomTableList ID="customTableList" runat="server" Visible="false" StopProcessing="true" />
    <cms:GeneralList ID="generalList" runat="server" Visible="false" StopProcessing="true" />
    <cms:CustomList ID="customList" runat="server" Visible="false" StopProcessing="true" />
</asp:Content>
