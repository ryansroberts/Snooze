<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SampleApplication.Controllers.HomeViewModel>" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Snooze Sample Application</title>
    <link type="text/css" rel="stylesheet" href="<%= Url.StaticFile("styles/global.css") %>" />
</head>
<body>
    <h1>Snooze Sample Application</h1>
    <p>
    <a href="<%= Model.Login %>">Login</a>
    </p>
    <img src="<%= Url.StaticFile("images/logo.png") %>" alt="logo" />
    <p>
        <a href="<%= Model.BooksLink %>">List Books</a>
    </p>
</body>
</html>
