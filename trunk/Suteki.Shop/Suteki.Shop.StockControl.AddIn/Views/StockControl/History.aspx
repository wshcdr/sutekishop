<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Shop.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Suteki.Shop.StockControl.AddIn.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<% var stockItem = (Suteki.Shop.StockControl.AddIn.Models.StockItem)Model; %>

    <h1>History for <%= stockItem.ProductName %> - <%= stockItem.SizeName %></h1>

    <table>
        <tr>
            <th class="thin">When</th>
            <th class="thin">What</th>
            <th class="thin">Stock Level</th>
            <th class="thin">Who</th>
        </tr>
    <% foreach (var stockItemHistory in stockItem.History) {%>
        <tr>
           <td class="thin"><%= stockItemHistory.DateTime %></td> 
           <td class="thin"><%= stockItemHistory.Description %></td> 
           <td class="thin"><%= stockItemHistory.Level %></td> 
           <td class="thin"><%= stockItemHistory.User %></td> 
        </tr>
        <% if (!string.IsNullOrEmpty(stockItemHistory.Comment))
           { %>
        <tr>
            <td colspan="4"><%= stockItemHistory.Comment%></td>
        </tr>
        <% } %>
    <% } %>
    </table>

</asp:Content>