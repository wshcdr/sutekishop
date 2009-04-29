<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Cms.master" Inherits="Suteki.Shop.ViewPage<CmsViewData>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2 id="cms-header"><%= Model.Content.Name%></h2>
    <% if(User.IsAdministrator()) { %>
        <ul id="admin-submenu">
			<li><%= Html.ActionLink<CmsController>(c => c.Edit(Model.Content.ContentId), "Edit")%></li>
        </ul>
    <% } %>
    <div id="cms-content">
		<%= Model.TextContent.Text%>
    </div>
</asp:Content>
