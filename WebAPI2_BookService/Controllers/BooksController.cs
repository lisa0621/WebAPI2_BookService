using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using WebAPI2_BookService.Models;

namespace WebAPI2_BookService.Controllers
{
    /*
    WebApiConfig 類別可能需要其他變更以新增此控制器的路由，請將這些陳述式合併到 WebApiConfig 類別的 Register 方法。注意 OData URL 有區分大小寫。

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using WebAPI2_BookService.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Book>("Books");
    builder.EntitySet<Author>("Authors"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    [ApiExplorerSettings(IgnoreApi = false)]
    public class BooksController : ODataController
    {
        private WebAPI2_BookServiceContext db = new WebAPI2_BookServiceContext();

        /// <summary>
        /// GetBooks
        /// </summary>
        /// <returns></returns>
        // GET: odata/Books
        [EnableQuery]
        public IQueryable<BookDTO> GetBooks()
        {
            var books = from b in db.Books
                        select new BookDTO()
                        {
                            Id = b.Id,
                            Title = b.Title,
                            AuthorName = b.Author.Name
                        };

            return books;
        }


        // GET api/Books/5
        [ResponseType(typeof(BookDetailDTO))]
        public async Task<IHttpActionResult> GetBook(int id)
        {
            var book = await db.Books.Include(b => b.Author).Select(b =>
                new BookDetailDTO()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Year = b.Year,
                    Price = b.Price,
                    AuthorName = b.Author.Name,
                    Genre = b.Genre
                }).SingleOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        // PUT: odata/Books(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Delta<Book> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Book book = await db.Books.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }

            patch.Put(book);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(book);
        }

        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> PostBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Books.Add(book);
            await db.SaveChangesAsync();

            // New code:
            // Load author name
            db.Entry(book).Reference(x => x.Author).Load();

            var dto = new BookDTO()
            {
                Id = book.Id,
                Title = book.Title,
                AuthorName = book.Author.Name
            };

            return CreatedAtRoute("DefaultApi", new { id = book.Id }, dto);
        }

        // PATCH: odata/Books(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Book> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Book book = await db.Books.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }

            patch.Patch(book);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(book);
        }

        // DELETE: odata/Books(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Book book = await db.Books.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Books(5)/Author
        [EnableQuery]
        public SingleResult<Author> GetAuthor([FromODataUri] int key)
        {
            return SingleResult.Create(db.Books.Where(m => m.Id == key).Select(m => m.Author));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookExists(int key)
        {
            return db.Books.Count(e => e.Id == key) > 0;
        }
    }
}
