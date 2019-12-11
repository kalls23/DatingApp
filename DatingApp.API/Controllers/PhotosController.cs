using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Data;
using DatingApp.API.DTOS;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using DatingApp.API.helpers;
using DatingApp.API.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Linq;



namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        private readonly IOptions <CloudinarySettings> _cloudinaryConfig;

        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
         {
            _cloudinaryConfig = cloudinaryConfig;
            _repo = repo;
            _mapper = mapper;

            Account account = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
         }

         [HttpGet("{id}", Name = "GetPhoto")]

         public async Task<IActionResult> GetPhoto(int id)
         {
             var photoFromRepo = await _repo.GetPhoto(id);

             var photo = _mapper.Map<PhotoForReturnDTO>(photoFromRepo);

             return Ok(photo);
         }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userid, 
        [FromForm]PhotoForCreationDTO photoForCreationDTO)
        {

            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
                
                var userFromRepo = await _repo.GetUser(userid);

                var file = photoForCreationDTO.File;

                var uploadResult = new ImageUploadResult();

                if(file.Length > 0){

                    using(var stream = file.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(file.Name, stream),
                            Transformation = new Transformation().Width(500).Height(500).
                            Crop("fill").Gravity("face")
                        };

                        uploadResult = _cloudinary.Upload(uploadParams);
                    }
                }
                photoForCreationDTO.Url = uploadResult.Uri.ToString();
                photoForCreationDTO.PublicId = uploadResult.PublicId;

                var photo = _mapper.Map<Photo>(photoForCreationDTO);

                if(!userFromRepo.Photos.Any(u => u.IsMain))
                    photo.IsMain = true;

                userFromRepo.Photos.Add(photo);

                if(await _repo.SaveAll())
                {
                    var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                    return CreatedAtRoute("GetPhoto", new { userId = userid, id = photo.Id},photoToReturn);
                };

                return BadRequest("Could not add photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id){

            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("This is already the main photo.");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo.");

            if(photoFromRepo.PublicId != null ){

            var deleteParams = new DeletionParams(photoFromRepo.PublicId);

            var result = _cloudinary.Destroy(deleteParams);

            if(result.Result == "ok"){
                _repo.Delete(photoFromRepo);
            }

            }

            if(photoFromRepo.PublicId == null) {
                    _repo.Delete(photoFromRepo);
            }

            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}