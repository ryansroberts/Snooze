<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SampleApplication.Controllers.BooksViewModel>" %>
<%@ Import Namespace="SampleApplication.Controllers" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Books</title>
</head>
<body>
    <h1>Books</h1>
    <p>
    <% if (User.Identity.IsAuthenticated == false) { %> <a href="<%= new LoginUrl { } %>">Log In</a> <% } %>
    </p>
    <ul>
        <% foreach (var book in Model.BookLinks)
           { %>
        <li><a href="<%= book %>"><%= book %></a></li>
        <%} %>
    </ul>
</body>
</html>
