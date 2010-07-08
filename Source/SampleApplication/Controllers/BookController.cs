using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Snooze;

namespace SampleApplication.Controllers
{
    public class BooksUrl : Url { }

    public class BookUrl : Url
    {
        public string BookId { get; set; }
    }

    public class BookCommentsUrl : SubUrl<BookUrl> { }

    public class BookCommentUrl : SubUrl<BookCommentsUrl>
    {
        public int CommentId { get; set; }
    }

    public class BookController : ResourceController
    {
        public ActionResult Get(BooksUrl url)
        {
            var books = LoadBooksXml();

            // Notice how we can construct URLs directly...
            var links = from book in books.Element("catalog").Elements("book")
                        select new BookUrl { BookId = book.Attribute("id").Value };

            // The action method doesn't say how the view model should be formatted.
            // Snooze will determin this based on the UA's Accept header.
            return OK(new BooksViewModel
                {
                    BookLinks = links.ToArray()
                })
                .WithCache(c => c.SetExpires(DateTime.Now.AddSeconds(60)))
                .WithHeader("X-Snooze", "example header");
        }

        // This action method simply returns a view model.
        // Snooze will wrap this into a ResourceResult.
        public BookViewModel Get(BookUrl url)
        {
            var books = LoadBooksXml();
            var book = books.Element("catalog").Elements("book").FirstOrDefault(e => e.Attribute("id").Value == url.BookId);
            var commentsUrl = url.Concat(new BookCommentsUrl());
            return new BookViewModel
            {
                Author = book.Element("author").Value,
                Title = book.Element("title").Value,
                Comments = book.Elements("comment").Select(c => c.Value).ToArray(),
                AddComment = new FutureAction(() => Post(commentsUrl, new NewComment()))
            };
        }

        public ActionResult Post(BookCommentsUrl url, NewComment newComment)
        {
            var books = LoadBooksXml();
            var book = books.Element("catalog").Elements("book").FirstOrDefault(e => e.Attribute("id").Value == url.Parent.BookId);
            book.Add(new XElement("comment", newComment.Comment));
            books.Save(BooksXmlFilename());

            var id = book.Elements("comment").Count() - 1;

            var commentUrl = url.Concat(new BookCommentUrl { CommentId = id });
            return Created(
                commentUrl,
                new CommentCreatedViewModel
                {
                    BookUrl = url.Parent // so the user can go back to the book.
                }
            );
        }

        public ActionResult Delete(BookCommentUrl url)
        {
            return NoContent();
        }

        private XDocument LoadBooksXml()
        {
            return XDocument.Load(BooksXmlFilename());
        }

        private string BooksXmlFilename()
        {
            return Server.MapPath("~/App_Data/Books.xml");
        }
    }

    public class BooksViewModel
    {
        public BookUrl[] BookLinks { get; set; }
    }

    public class BookViewModel
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string[] Comments { get; set; }
        public FutureAction AddComment { get; set; }
    }

    public class NewComment
    {
        public string Comment { get; set; }
    }

    public class CommentCreatedViewModel
    {
        public Url BookUrl { get; set; }
    }
}
