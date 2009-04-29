<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ShopViewData>" %>
<h3>Payment Details</h3>
<div class="columnContainer">
    <div class="contentLeftColumn">

        <% if(Model.Order.PayByTelephone) { %>
        
        <p>Pay By Telephone</p>
        
        <% } else { %>

            <dt>Card Type</dt><dd><%= Model.Order.Card.CardType.Name %>&nbsp;</dd>
            <dt>Card Holder</dt><dd><%= Model.Order.Card.Holder %>&nbsp;</dd>
        
            <% if(ViewContext.HttpContext.User.IsAdministrator()) { %>

                <%= Html.ErrorBox(Model) %>

                <% if (Model.Card == null) { %>

                    <% using (Html.BeginForm("ShowCard", "Order", FormMethod.Post,
                           new Dictionary<string, object> { { "onsubmit", "submitHandler();" } }))
                       { %>
                        
                        <%= Html.Hidden("orderId", Model.Order.OrderId.ToString()) %>
                        
                        <label for="privateKey">Private Key</label>
                        <%= Html.TextBox("privateKey")%>
                        
                        <%= Html.SubmitButton("cardDetailsSubmit", "Get Card Details")%>

                    <% } %>
                
                <% } else { %>
                
                    <dl>
                        <dt>Card Number</dt><dd><%= Model.Card.Number %></dd>
                        <dt>Issue Number</dt><dd><%= Model.Card.IssueNumber %></dd>
                        <dt>Security Code</dt><dd><%= Model.Card.SecurityCode %></dd>
                        <dt>Start Date</dt><dd><%= Model.Card.StartDateAsString %></dd>
                        <dt>Expiry Date</dt><dd><%= Model.Card.ExpiryDateAsString %></dd>
                    </dl>
                
                <% } %>

            <% } %>        
        
        <% } %>
        
    </div>
    <div class="contentRightColumn">

    </div>
</div>

