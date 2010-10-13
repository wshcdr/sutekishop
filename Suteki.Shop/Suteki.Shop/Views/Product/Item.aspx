<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Shop.Master" Inherits="Suteki.Shop.ViewPage<ShopViewData>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<script type="text/javascript">

function onThumbnailClick(img)
{
    var mainImage = document.getElementById("mainImage");
    var imgUrl = img.src.replace("thumb", "main");
    mainImage.src = imgUrl;
}

</script>

<div class="error"><%= TempData["message"] %></div>

<h1><%= Html.Encode(ViewData.Model.Product.Name) %>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<%= ViewData.Model.Product.Price.ToStringWithSymbol()%><%= ViewData.Model.Product.IsActiveAsString %></h1>

<% if(User.IsAdministrator()) { %>
    <%= Html.ActionLink<ProductController>(c => c.Edit(ViewData.Model.Product.Id), "Edit", new { @class = "linkButton" })%>
    <% Html.PostAction<ProductCopyController>(c => c.Copy(ViewData.Model.Product), "Copy");%>
<%
} %>

<% Html.RenderPartial("ProductDescription", Model.Product); %>
<% Html.RenderPartial("BasketOptions", Model.Product); %>

<p>If an item is out of stock, please email us at 
<a href="mailto:<%= ((Suteki.Shop.Controllers.ControllerBase)this.ViewContext.Controller).BaseControllerService.EmailAddress %>">
<%= ((Suteki.Shop.Controllers.ControllerBase)this.ViewContext.Controller).BaseControllerService.EmailAddress%>
</a>
 so that we can let you know when it will be available.</p>

<% Html.RenderAction<ReviewsController>(c => c.Show(Model.Product.Id)); %>

</asp:Content>
