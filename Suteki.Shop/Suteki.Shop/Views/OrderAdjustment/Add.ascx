<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<OrderAdjustment>" %>
<div>
    <h3>Add Adjustment</h3>
    <% using(Html.BeginForm<OrderAdjustmentController>(c => c.Add(0))) { %>
        <%= Html.HiddenFor(x => x.Order.Id) %>
        <label for="Description">Description</label>
        <%= Html.TextBoxFor(x => x.Description) %>
        <label for="Amount">Amount</label>
        <%= Html.TextBoxFor(x => x.Amount) %>
        <input type="submit" value="Add Adjustment" />
    <% } %>
</div>