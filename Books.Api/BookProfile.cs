using AutoMapper;
using System.Collections.Generic;

namespace Books.Api
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<Entities.Book, Models.Book>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                    $"{src.Author.FirstName} {src.Author.LastName}"));

            CreateMap<Models.BookForCreation, Entities.Book>();

            CreateMap<IEnumerable<ExternalModels.BookCover>, Models.BookWithCovers>()
               .ForMember(dest => dest.BookCovers, opt => opt.MapFrom(src =>
                  src));

            CreateMap<Entities.Book, Models.BookWithCovers>()
               .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                   $"{src.Author.FirstName} {src.Author.LastName}"));
        }
    }
}
