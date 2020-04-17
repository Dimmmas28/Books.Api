using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Books.Api.Controllers
{
    [Route("api/synchronous")]
    [ApiController]
    public class SynchronousBookController : ControllerBase
    {
        private IBooksRepository _booksRepozitory;

        public SynchronousBookController(IBooksRepository booksRepository)
        {
            _booksRepozitory = booksRepository ??
                throw new ArgumentNullException(nameof(booksRepository));
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            // var booksEntities = _booksRepozitory.GetBooks();
            var booksEntities = _booksRepozitory.GetBooksAsync().Result;

            return Ok(booksEntities);
        }
    }
}
