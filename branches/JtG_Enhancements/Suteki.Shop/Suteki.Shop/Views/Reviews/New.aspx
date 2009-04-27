<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Suteki.Shop.ViewPage<ReviewViewData>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>Add a Review</h2>
	
	<% using (Html.BeginForm()) { %>
		<%= this.TextArea(x => x.Review.Text).Rows(10).Columns(40) %>
		<%= this.Hidden(x => x.ProductId).Name("id") %>
		Rating: <br />
		<input name="<%= this.NameFor(x => x.Review.Rating) %>" type="radio" value="5" /> <%= Html.Stars(5) %><br />
		<input name="<%= this.NameFor(x => x.Review.Rating) %>" type="radio" value="4" /> <%= Html.Stars(4) %><br />
		<input name="<%= this.NameFor(x => x.Review.Rating) %>" type="radio" value="3" checked="checked" /> <%= Html.Stars(3) %><br />
		<input name="<%= this.NameFor(x => x.Review.Rating) %>" type="radio" value="2" /><%= Html.Stars(2) %><br />
		<input name="<%= this.NameFor(x => x.Review.Rating) %>" type="radio" value="1" /><%= Html.Stars(1) %>
		
		
		<input type="submit" value="Submit Review" />
	<% } %>

</asp:Content>
