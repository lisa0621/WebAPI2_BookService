using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI2_BookService.Models
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
    }
}