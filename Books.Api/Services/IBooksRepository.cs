using Books.Api.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    interface IBooksRepository
    {
        //IEnumerable<Book> GetBooks();
        //Book GetBook();

        Task<IEnumerable<Book>> GetBooksAsync();

        Task<Book> GetBookAsync();
    }
}
