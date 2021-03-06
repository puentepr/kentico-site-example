<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="CMSModules_MessageBoards_Controls_Boards_BoardUserSubscriptions" CodeFile="BoardUserSubscriptions.ascx.cs" %>
<%@ Register src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" tagname="UniGrid" tagprefix="cms" %>
<asp:Label runat="server" ID="lblError" CssClass="ErrorLabel" EnableViewState="false"
    Visible="false" />
<cms:LocalizedLabel ID="lblMessage" runat="server" CssClass="InfoLabel" EnableViewState="false"
    ResourceString="boardsubscripitons.userissubscribed" />
<cms:UniGrid ID="boardSubscriptions" runat="server" FilterLimit="10"
    GridName="~/CMSModules/MessageBoards/Tools/Boards/BoardUserSubscriptions.xml" />
