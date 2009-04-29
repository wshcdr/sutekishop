<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/CmsSubMenu.master" Inherits="Suteki.Shop.ViewPage<CmsViewData>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <% if (ViewData.Model.Content.CanEdit((User)User)) { %>
        <ul id="admin-submenu">
			<li><%= Html.ActionLink<CmsController>(c => c.Edit(Model.Content.ContentId), "Edit")%></li>
        </ul>
    <% } %>
    <div id="cms-content">
		<%= Model.TextContent.Text%>
    </div>
</asp:Content>
