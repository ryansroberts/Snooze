<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SampleApplication.Controllers.BookViewModel>" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Book</title>
</head>
<body>
    <h1><%= Model.Title %></h1>
    <h2>by <%= Model.Author %></h2>
    <% if(Model.Comments.Length > 0) { %>
    <ul>
        <% foreach (var comment in Model.Comments) { %>
        <li><%= comment%></li>
        <% } %>
    </ul>
    <% } %>
    <form method="<%= Model.AddComment.Method %>" action="<%= Model.AddComment.Url %>">
        <input type="text" name="comment"/>
        <button type="submit">Add Comment</button>
    </form>
</body>
</html>
