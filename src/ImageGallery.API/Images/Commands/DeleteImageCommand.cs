using AutoMapper;
using ImageGallery.API.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGallery.API.Images.Commands
{
    public class DeleteImageCommand : IRequest
    {
        public Guid Id { get; set; }


    }

    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand>
    {
        private readonly IGalleryRepository _galleryRepository;

        public DeleteImageCommandHandler(
            HttpContext httpContext,
            IGalleryRepository galleryRepository,
            IWebHostEnvironment hostingEnvironment,
            IMapper mapper)
        {
            _galleryRepository = galleryRepository ??
                throw new ArgumentNullException(nameof(galleryRepository));
        }
        public Task<Unit> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            var imageFromRepo = _galleryRepository.GetImage(request.Id);

            if (imageFromRepo == null)
            {
                throw new KeyNotFoundException();
            }

            _galleryRepository.DeleteImage(imageFromRepo);

            _galleryRepository.Save();

            return Unit.Task;
        }
    }
}
