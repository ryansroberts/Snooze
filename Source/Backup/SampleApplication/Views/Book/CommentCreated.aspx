<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SampleApplication.Controllers.CommentCreatedViewModel>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Comment Created</title>
</head>
<body>
    <p>
    Thanks for your comment.<br />
    <a href="<%= Model.BookUrl %>">Back to book.</a>
    </p>
</body>
</html>
