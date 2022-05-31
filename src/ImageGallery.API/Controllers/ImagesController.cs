using AutoMapper;
using ImageGallery.API.Images.Commands;
using ImageGallery.API.Images.Queries;
using ImageGallery.API.Services;
using ImageGallery.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.API.Controllers
{
    [Route("api/images")]
    [ApiController]
    [Authorize]
    public class ImagesController : ApiControllerBase
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        public ImagesController(
            IGalleryRepository galleryRepository,
            IWebHostEnvironment hostingEnvironment,
            IMapper mapper,
            IMediator mediator
            )
        {
            _galleryRepository = galleryRepository ??
                throw new ArgumentNullException(nameof(galleryRepository));
            _hostingEnvironment = hostingEnvironment ??
                throw new ArgumentNullException(nameof(hostingEnvironment));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ??
                throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet()]
        public async Task<IActionResult> GetImages()
        {
            var ownerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            // get from repo
            var imagesFromRepo = _galleryRepository.GetImagesByUserId(ownerId);

            // map to model
            var imagesToReturn = _mapper.Map<IEnumerable<Model.Image>>(imagesFromRepo);

            // return
            return Ok(imagesToReturn);

            //var result = _mediator.Send(new GetImagesQuery());
            //return Ok(result);
        }

        [HttpGet("{id}", Name = "GetImage")]
        [Authorize(Policy = "MustOwnImage")]
        public IActionResult GetImage(Guid id)
        {
            var imageFromRepo = _galleryRepository.GetImage(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            var imageToReturn = _mapper.Map<Model.Image>(imageFromRepo);

            return Ok(imageToReturn);
        }

        [HttpPost()]
        [Authorize]
        public async Task CreateImage([FromBody] ImageForCreation createImageCommand)
        {

            //return await _mediator.Send(createImageCommand);

            // Automapper maps only the Title in our configuration
            var imageEntity = _mapper.Map<Entities.Image>(createImageCommand);

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
            System.IO.File.WriteAllBytes(filePath, createImageCommand.Bytes);

            // fill out the filename
            imageEntity.FileName = fileName;
            imageEntity.Id = Guid.NewGuid();
            imageEntity.OwnerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            // ownerId should be set - can't save image in starter solution, will
            // be fixed during the course
            //imageEntity.OwnerId = ...;

            // add and save.
            _galleryRepository.AddImage(imageEntity);

            _galleryRepository.Save();

            var imageToReturn = _mapper.Map<Image>(imageEntity);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MustOwnImage")]
        public async Task DeleteImage(Guid id)
        {
            var deleteImageCommand = new DeleteImageCommand()
            {
                Id = id
            };

            
            _mediator.Send(deleteImageCommand);

            //var imageFromRepo = _galleryRepository.GetImage(id);

            //if (imageFromRepo == null)
            //{
            //    return NotFound();
            //}

            //_galleryRepository.DeleteImage(imageFromRepo);

            //_galleryRepository.Save();

            //return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MustOwnImage")]
        public IActionResult UpdateImage(Guid id,
            [FromBody] ImageForUpdate imageForUpdate)
        {
            var imageFromRepo = _galleryRepository.GetImage(id);
            if (imageFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(imageForUpdate, imageFromRepo);

            _galleryRepository.UpdateImage(imageFromRepo);

            _galleryRepository.Save();

            return NoContent();
        }
    }
}