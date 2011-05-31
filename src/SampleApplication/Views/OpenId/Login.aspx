<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Snooze.Authentication.LoginViewModel>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form action="<%= Model.Login.Url %>" method="post">
        <% if (Model.LoginFailed) { %>
            <div class="error">Error logging in.</div>
        <% } %>
        <input type="text" name="openid" /><button type="submit">Login</button>
    </form>
</body>
</html>
