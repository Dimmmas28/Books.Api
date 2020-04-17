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
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private IBooksRepository _booksRepozitory;
        private readonly IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepozitory = booksRepository ??
                throw new ArgumentNullException(nameof(booksRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksFilter]
        public async Task<IActionResult> GetBooks()
        {
            var booksEntities = await _booksRepozitory.GetBooksAsync();
            return Ok(booksEntities);
        }

        [HttpGet]
        [BookWithCoversResultFilter]
        [Route("{id}", Name = "GetBook")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookEntity = await _booksRepozitory.GetBookAsync(id);

            if (bookEntity == null)
            {
                return NotFound();
            }

            var bookCovers = await _booksRepozitory.GetBooksCoverAsync(id);

            //(Entities.Book book, IEnumerable<ExternalModels.BookCover> bookCover) propertyBag = (bookEntity, bookCovers);

            return Ok((bookEntity, bookCovers));
        }

        [HttpPost]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook([FromBody] BookForCreation book)
        {
            var bookEntity = _mapper.Map<Entities.Book>(book);
            _booksRepozitory.AddBook(bookEntity);

            await _booksRepozitory.SaveChangesAsync();

            await _booksRepozitory.GetBookAsync(bookEntity.Id);

            return CreatedAtRoute("GetBook",
                new { id = bookEntity.Id },
                bookEntity);
        }
    }
}
