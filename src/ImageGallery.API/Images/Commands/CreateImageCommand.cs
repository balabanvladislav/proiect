using AutoMapper;
using ImageGallery.API.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageGallery.Model;
using Microsoft.AspNetCore.Http;

namespace ImageGallery.API.Images.Commands
{
    public class CreateImageCommand : IRequest<Guid>
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public byte[] Bytes { get; set; }

    }

    public class CreateImageCommandHandler : IRequestHandler<CreateImageCommand, Guid>
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly HttpContext _httpContext;

        public CreateImageCommandHandler(
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
        public Task<Guid> Handle(CreateImageCommand request, CancellationToken cancellationToken)
        {

            // Automapper maps only the Title in our configuration
            var imageEntity = _mapper.Map<Entities.Image>(request);

            // Create an image from the passed-in bytes (Base64), and
            // set the filename on the image

            // get this environment's web root path (the path
            // from which static content, like an image, is served)
            var webRootPath = _hostingEnvironment.WebRootPath;

            // create the filename
            string fileName = Guid.NewGuid().ToString() + ".jpg";

            // the full file path
            var filePath = Path.Combine($"{webRootPath}/images/{fileName}");

            // write bytes and auto-close stream
            System.IO.File.WriteAllBytes(filePath, request.Bytes);

            // fill out the filename
            imageEntity.FileName = fileName;
            imageEntity.Id = Guid.NewGuid();
            imageEntity.OwnerId = _httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            _galleryRepository.AddImage(imageEntity);

            _galleryRepository.Save();

            var imageToReturn = _mapper.Map<Image>(imageEntity);

            return Task.FromResult(imageEntity.Id);
        }
    }
}
