<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SampleApplication.Controllers.Html5ViewModel>" %>
<%@ Import Namespace="SampleApplication.Controllers" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Snooze Sample Application</title>
    <link type="text/css" rel="stylesheet" href="<%= Url.StaticFile("styles/global.css") %>" />
</head>
<body>

    <h1>Snooze Sample Application</h1>
    <div>
   
   <form action="Html5.aspx" method="POST">
       <fieldset>
           <legend>here's a html5 form</legend>
           <ul>
               <li>
                   <label>Email<em>*</em><input type="email" required="required"/></label>
               </li>
               <li>
                   <label>Url<em>*</em><input type="url" required="required"/></label>
               </li>
           </ul>
       </fieldset>
   </form>
    </div>

</body>
</html>
