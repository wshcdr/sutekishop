<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.IEnumerable<Suteki.Shop.IComment>>" MasterPageFile="~/Views/Shared/Shop.Master" %>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContentPlaceHolder">
    <h2>Customer Comments</h2>
    <p><%= Html.ActionLink<CommentController>(c => c.New(), "Make a general comment about us.") %></p>
    <% foreach(var comment in Model) { %>
    <hr />		
    <div>
		<p>
        <%
            var review = comment.CastAs<Review>();
            if (review != null) { %>
			<strong><%= Html.ActionLink<ProductController>(c => c.Item(review.Product.UrlName), review.Product.Name)%></strong> reviewed by 
        <%} %>
            <strong><%= Html.Encode(comment.Reviewer) %></strong> 
		</p>
		<p>
			<%= Html.Encode(comment.Text) %>
		</p>
	</div>
    <% } %>
    
    <% if (Model.Count() == 0) { %>
		<p>Be the first to write a review of one our products.</p>
    <% } %>
</asp:Content>
