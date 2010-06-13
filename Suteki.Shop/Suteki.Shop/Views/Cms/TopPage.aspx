<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Cms.master" Inherits="Suteki.Shop.ViewPage<CmsViewData>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <%= ViewData.Model.TextContent.Text%>

    <% if(User.IsAdministrator()) { %>
        <p><%= Html.ActionLink<CmsController>(c => c.EditTop(ViewData.Model.Content.Id), "Edit")%></p>
    <% } %>
</asp:Content>
