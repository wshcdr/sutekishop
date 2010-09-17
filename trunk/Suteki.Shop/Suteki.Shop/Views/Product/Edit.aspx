<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Shop.Master" Inherits="Suteki.Shop.ViewPage<ShopViewData>" %>
<%@ Import Namespace="Suteki.Common.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h1>Product</h1>
    
	<%= Html.ValidationSummary() %>
    <%= Html.MessageBox(ViewData.Model) %>
    
    <% if(ViewData.Model.Product.Id > 0) { %>
        <%= Html.ActionLink<ProductController>(c => c.Item(ViewData.Model.Product.UrlName), "Preview") %>
    <% } %>

    <% using (Html.MultipartForm()) { %>
		<%= this.Hidden(x => x.Product.Id) %>
		<%= this.Hidden(x => x.Product.Position) %>

		<%= this.TextBox(x => x.Product.Name).Label("Name") %>
		<%= this.MultiSelect(x => x.Product.ProductCategories.Select(y => y.Category.Id)).Name("categories").Options(Model.Categories, x => x.Id, x => x.Name).Label("Categories (ctrl+click to select more than one)") %>
        <%= this.TextBox(x => x.Product.Weight).Label("Weight") %>
        <%= this.TextBox(x => x.Product.Price).Label("Price " + Money.Symbol) %>
        <%= this.CheckBox(x => x.Product.IsActive).Label("Active") %>
        
        <%= this.TextArea(x => x.Product.Description).Label("Description") %>
        
        <h3>Sizes</h3>
        
        <p>
        <% foreach(var size in ViewData.Model.Product.Sizes.Active()) { %>
            <%= size.Name %>&nbsp;
        <% } %>
        
        <%= Html.ActionLink<ProductController>(c => c.ClearSizes(ViewData.Model.Product.Id), "Clear all sizes")%>
        </p>
        <div class="sizeInput">
        <% for(int i=0; i<10; i++) { %>
            <%= Html.TextBox("size_" + i)%>
        <% } %>
        </div>
        
        <h3>Photos</h3>
        
        <div class="imageList">
        <% foreach(var productImage in ViewData.Model.Product.ProductImages.InOrder()) { %>
            <div class="imageEdit">
            <%= Html.Image("~/ProductPhotos/" + productImage.Image.ThumbFileName) %><br />
            <%= Html.UpArrowLink<ProductImageController>(c => c.MoveImageUp(ViewData.Model.Product.Id, productImage.Position)) %>
            <%= Html.DownArrowLink<ProductImageController>(c => c.MoveImageDown(ViewData.Model.Product.Id, productImage.Position)) %> &nbsp;&nbsp;
            <%= Html.CrossLink<ProductImageController>(c => c.DeleteImage(ViewData.Model.Product.Id, productImage.Id)) %>
            </div>
        <% } %>
        </div>
        
        <div class="clear" />
        
        <% for (int i = 0; i < 5; i++) { %>
            <input type="file" id="image_<%= i.ToString() %>" name="image_<%= i.ToString() %>" />
        <% } %>
        
        <input type="submit" value="Save Changes" />
    <% } %>
    
    <% Html.InitialiseRichTextEditor(); %>
    
</asp:Content>