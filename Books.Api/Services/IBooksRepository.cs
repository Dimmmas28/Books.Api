using Books.Api.Entities;
using Books.Api.ExternalModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public interface IBooksRepository
    {
        IEnumerable<Book> GetBooks();
        //Book GetBook();

        Task<IEnumerable<Book>> GetBooksAsync();

        Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds);

        Task<BookCover> GetBookCoverAsync(string coverId);

        Task<IEnumerable<BookCover>> GetBooksCoverAsync(Guid bookId);

        Task<Book> GetBookAsync(Guid id);

        void AddBook(Book bookToAdd);

        Task<bool> SaveChangesAsync();
    }
}
