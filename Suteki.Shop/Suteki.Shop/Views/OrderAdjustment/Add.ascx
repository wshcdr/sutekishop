<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<OrderAdjustment>" %>
<div class="hideForPrint">
    <h3>Add Adjustment</h3>
    <% using(Html.BeginForm<OrderAdjustmentController>(c => c.Add(0))) { %>
        <%= Html.HiddenFor(x => x.Order.Id) %>
        <label for="Description" class="inline">Description</label>
        <%= Html.TextBoxFor(x => x.Description, new { @class = "inline wide" }) %>
        <label for="Amount" class="inline">Amount</label>
        <%= Html.TextBoxFor(x => x.Amount, new { @class = "inline" })%>
        <input type="submit" value="Add Adjustment" />
    <% } %>
    <hr />
</div>