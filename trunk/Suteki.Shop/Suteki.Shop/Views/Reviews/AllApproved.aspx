<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.IEnumerable<Suteki.Shop.Review>>" MasterPageFile="~/Views/Shared/Shop.Master" %>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContentPlaceHolder">
    <h2>Reviews of our products</h2>
    <% foreach(var review in Model) { %>
    <div>
		<p>
			<strong><%= Html.ActionLink<ProductController>(c => c.Item(review.Product.UrlName), review.Product.Name) %></strong> reviewed by 
			<strong><%= Html.Encode(review.Reviewer) %></strong> 
		</p>
		<p>
			<%= Html.Encode(review.Text) %>
		</p>
		
		<hr />
	</div>
    <% } %>
    
    <% if (Model.Count() == 0) { %>
		<p>Be the first to write a review of one our products.</p>
    <% } %>
</asp:Content>
