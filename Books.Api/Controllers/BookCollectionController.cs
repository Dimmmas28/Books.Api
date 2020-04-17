using AutoMapper;
using Books.Api.Filters;
using Books.Api.Models;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Controllers
{
    [Route("api/bookcollection")]
    [ApiController]
    [BooksFilter]
    public class BookCollectionController : ControllerBase
    {
        private readonly IBooksRepository _booksRepozitory;
        private readonly IMapper _mapper;


        public BookCollectionController(IBooksRepository booksRepozitory, IMapper mapper)
        {
            _booksRepozitory = booksRepozitory ?? throw new ArgumentNullException(nameof(booksRepozitory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBookCollection(
            [FromBody] IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);

            foreach (var bookEntity in bookEntities)
            {
                _booksRepozitory.AddBook(bookEntity);
            }

            await _booksRepozitory.SaveChangesAsync();

            var booksToReturn = await _booksRepozitory.GetBooksAsync(
                                    bookEntities.Select(b => b.Id).ToList());

            var bookIds = string.Join(",", booksToReturn.Select(b => b.Id));

            return CreatedAtRoute("GetBookCollection",
                new { bookIds },
                booksToReturn);
        }

        [HttpGet("({bookIds})", Name = "GetBookCollection")]
        public async Task<IActionResult> GetBookCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> bookIds)
        {
            var bookEntities = await _booksRepozitory.GetBooksAsync(bookIds);

            if (bookIds.Count() != bookEntities.Count())
            {
                return NotFound();
            }

            return Ok(bookEntities);
        }
    }
}
