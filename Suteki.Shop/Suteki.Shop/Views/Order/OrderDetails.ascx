
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ShopViewData>" %>
  
<table>
    <tr>
        <th class="wide">Product</th>
        <th class="thin number">Quantity</th>
        <th class="thin number">Unit Price</th>
        <th class="thin number">Total Price</th>
    </tr>
    
    <% foreach (var orderLine in ViewData.Model.Order.OrderLines)
       { %>
    
    <tr>
        <td><%= orderLine.ProductName %></td>
        <td class="number"><%= orderLine.Quantity%></td>
        <td class="number"><%= orderLine.Price.ToStringWithSymbol()%></td>
        <td class="number"><%= orderLine.Total.ToStringWithSymbol()%></td>
    </tr>
    
    <% } %>
    
    <tr class="total">
        <td>Total</td>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td class="number"><%= ViewData.Model.Order.Total.ToStringWithSymbol()%></td>
    </tr>

    <tr>
        <td>Postage</td>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td class="number"><%= ViewData.Model.Order.PostageTotal%></td>
    </tr>

    <tr>
        <td colspan="4">(<%= ViewData.Model.Order.PostageDescription %>)</td>
    </tr>

    <tr class="total">
        <td>Total With Postage</td>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td class="number"><%= ViewData.Model.Order.TotalWithPostage%></td>
    </tr>
    
</table>