<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ReviewViewData>" %>

<%= Html.ActionLink<ReviewsController>(c => c.New(Model.ProductId), "Leave a Review") %>

<% if (Model.Reviews.Count() > 0) { %>

<a href="javascript:void(0)">Show Reviews</a>

<div id="reviews" style="display: none">
</div>

<script type="text/javascript">
	$(function() {
		$('#reviews').toggle();
	});
</script>

<% } %>