using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers.CloudinarySettings;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController] // This validates the model state error, returning 400 bad request
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _daringRepo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        /* We use IOptions because we want to retrieve data for claudinary
           which are added as a service in the startup */

        public PhotosController(
            IDatingRepository daringRepo,
            IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _daringRepo = daringRepo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            // We pass in the cloudinary class the required fields
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
                );

            _cloudinary = new Cloudinary(acc);
        }

        // In http get is the id of the photo
        // The name is used for returning created at route, hence the dto for photo only
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _daringRepo.GetPhoto(id);

            var photoToReturn = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photoToReturn);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            // is matching the token for that particular user?
            // Compare id of the path, with the user id from the token
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            // Retrieve user
            var userFromRepo = await _daringRepo.GetUser(userId);

            var file = photoForCreationDto.File;

            // We store result that comes from cloudinary
            var uploadResult = new ImageUploadResult();

            // Here, we need to do a check, if there is a file, otherwise this will return a null reference exception
            if (file.Length > 0)
            {
                // then we read the file in memory. We use using so that we can dispose once we have completed the method
                // openreadstream reads our file into the memory
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    // uploading to cloudinary and get response
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // This needs to be wraped inside a try catch block
            // We might get an error if the secrets/keys are null
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicPhotoId = uploadResult.PublicId;

            // Map from dto to photo class
            // Map from is inside the parenthesis
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // if this returns false, it means that the user doesn`t have a main photo
            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await _daringRepo.SaveAll())
            {
                // This is not the detailed photo to be returned
                var returnPhoto = _mapper.Map<PhotoForReturnDto>(photo);

                // For http post we shouldn`t return ok, instead we should return a createdatroute
                // We need to provide a resource that we just created
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, returnPhoto);
            }

            return BadRequest("Could not upload the photo");
        }

        // Passing an http post with photo id if we want to make a photo main
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _daringRepo.GetUser(userId);

            // if the Id passing in, is not matching with any of the user photo collection
            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            // The selected photo to be main
            var photoFromRepo = await _daringRepo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }

            // The current main set to false
            var currentMainPhoto = await _daringRepo.GetMainPhoto(userId);

            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _daringRepo.SaveAll())
            {
                return NoContent();
            }

            // If above failed to save
            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _daringRepo.GetUser(userId);

            // if the Id passing in, is not matching with any of the user photo collection
            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            // The selected photo to be main
            var photoFromRepo = await _daringRepo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("You cannot delete your main photo");
            }

            if (photoFromRepo.PublicPhotoId != null)
            {
                // We need to delete photo from cloudinary and our reference in the DB
                var deleteParams = new DeletionParams(photoFromRepo.PublicPhotoId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _daringRepo.Delete(photoFromRepo);
                }
            }
            else
            {
                // Delete photo if photo is not from cloudinary
                _daringRepo.Delete(photoFromRepo);
            }

            if (await _daringRepo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("failed to delete the photo");
        }
    }
}