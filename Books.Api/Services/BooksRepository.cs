using Books.Api.Context;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Books.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<BooksRepository> _logger;

        public BooksRepository(BooksContext context, IHttpClientFactory httpClientFactory,
            ILogger<BooksRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ?? 
                throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Book> GetBookAsync(Guid id)
        {
            _logger.LogInformation($"ThreadId when entering GetBookAsync: {Thread.CurrentThread.ManagedThreadId}");
            var amountOfPages = await GetBookPages();

            return await _context.Book.Include(b => b.Author).FirstOrDefaultAsync<Book>();
        }

        private Task<int> GetBookPages()
        {
            _logger.LogInformation($"ThreadId when calculating amount of pages: {Thread.CurrentThread.ManagedThreadId}");

            return Task.Run(() =>
            {
                var pageCalculator = new ComplicatedPageCalculator();
                return pageCalculator.CalculateBookPages();
            });
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            await _context.Database.ExecuteSqlCommandAsync("WAITFOR DELAY '00:00:02';");
            return await _context.Book.Include(b => b.Author).ToListAsync<Book>();
        }

        public IEnumerable<Book> GetBooks()
        {
            _context.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");

            return _context.Book.Include(b => b.Author).ToList<Book>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _context.Book.Where(b => bookIds.Contains(b.Id))
                .Include(b => b.Author).ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            var client = new HttpClient(clientHandler);
            var response = await client
                .GetAsync($"http://localhost:52644/api/bookcovers/{coverId}");


            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBooksCoverAsync(Guid bookId)
        {
            //   HttpClientHandler clientHandler = new HttpClientHandler();
            //clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            var client = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();

            _cancellationTokenSource = new CancellationTokenSource();

            var bookCoverUrls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
           //     $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
            //    $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

            var downloadBookCoverTasksQuery = 
                from bookCoverUrl
                in bookCoverUrls
                select DownloadBookCoverAsync(client, bookCoverUrl, _cancellationTokenSource.Token);

            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();

            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException operationCanceledExeption)
            {
                
                _logger.LogInformation($"{operationCanceledExeption.Message}");
                foreach (var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message}");
                throw exception;
            }
            
        }

        private async Task<BookCover> DownloadBookCoverAsync(
            HttpClient httpClient, string bookCoverUrl, CancellationToken cancellationToken)
        {
         //   throw new Exception("fsdsd");
            var response = await httpClient
                .GetAsync(bookCoverUrl, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());

                return bookCover;
            }

            _cancellationTokenSource.Cancel();

            return null;
        }
    }
}
