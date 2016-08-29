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
    public class BooksController : ODataController
    {
        private WebAPI2_BookServiceContext db = new WebAPI2_BookServiceContext();

        // GET: odata/Books
        [EnableQuery]
        public IQueryable<Book> GetBooks()
        {
            return db.Books;
        }

        // GET: odata/Books(5)
        [EnableQuery]
        public SingleResult<Book> GetBook([FromODataUri] int key)
        {
            return SingleResult.Create(db.Books.Where(book => book.Id == key));
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

        // POST: odata/Books
        public async Task<IHttpActionResult> Post(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Books.Add(book);
            await db.SaveChangesAsync();

            return Created(book);
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
