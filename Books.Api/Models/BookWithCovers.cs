using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Models
{
    public class BookWithCovers : Book
    {
        public IEnumerable<ExternalModels.BookCover> BookCovers { get; set; } = new List<ExternalModels.BookCover>();
    }
}
