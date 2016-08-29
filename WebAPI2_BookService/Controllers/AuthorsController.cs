﻿using System;
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
    builder.EntitySet<Author>("Authors");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    [ApiExplorerSettings(IgnoreApi = false)]
    public class AuthorsController : ODataController
    {
        private WebAPI2_BookServiceContext db = new WebAPI2_BookServiceContext();

        // GET: odata/Authors
        [EnableQuery]
        public IQueryable<Author> GetAuthors()
        {
            return db.Authors;
        }

        // GET: odata/Authors(5)
        [EnableQuery]
        public SingleResult<Author> GetAuthor([FromODataUri] int key)
        {
            return SingleResult.Create(db.Authors.Where(author => author.Id == key));
        }

        // PUT: odata/Authors(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Delta<Author> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Author author = await db.Authors.FindAsync(key);
            if (author == null)
            {
                return NotFound();
            }

            patch.Put(author);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(author);
        }

        // POST: odata/Authors
        public async Task<IHttpActionResult> Post(Author author)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Authors.Add(author);
            await db.SaveChangesAsync();

            return Created(author);
        }

        // PATCH: odata/Authors(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Author> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Author author = await db.Authors.FindAsync(key);
            if (author == null)
            {
                return NotFound();
            }

            patch.Patch(author);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(author);
        }

        // DELETE: odata/Authors(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Author author = await db.Authors.FindAsync(key);
            if (author == null)
            {
                return NotFound();
            }

            db.Authors.Remove(author);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AuthorExists(int key)
        {
            return db.Authors.Count(e => e.Id == key) > 0;
        }
    }
}
