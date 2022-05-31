using AutoMapper;
using ImageGallery.API.Services;
using ImageGallery.Model;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGallery.API.Images.Queries
{
    public class GetImagesQuery : IRequest<IEnumerable<Image>>
    {

    }

    public class GetImagesQueryHandler : IRequestHandler<GetImagesQuery, IEnumerable<Image>>
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly HttpContext _httpContext;

        public GetImagesQueryHandler(
            HttpContext httpContext,
            IGalleryRepository galleryRepository,
            IWebHostEnvironment hostingEnvironment,
            IMapper mapper)
        {
            _galleryRepository = galleryRepository ??
                    throw new ArgumentNullException(nameof(galleryRepository));
            _hostingEnvironment = hostingEnvironment ??
                throw new ArgumentNullException(nameof(hostingEnvironment));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));

            _httpContext = httpContext ??
                throw new ArgumentNullException(nameof(httpContext));
        }

        public Task<IEnumerable<Image>> Handle(GetImagesQuery request, CancellationToken cancellationToken)
        {
            var ownerId = _httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            // get from repo
            var imagesFromRepo = _galleryRepository.GetImagesByUserId(ownerId);

            // map to model
            var imagesToReturn = _mapper.Map<IEnumerable<Model.Image>>(imagesFromRepo);

            return Task.FromResult(imagesToReturn);
        }
    }

}
