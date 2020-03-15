using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers.CloudinarySettings;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    /* 
     * This class is to show how to implement policy based authorization.
     * The Repository is not implemented for this class hence we have the DataContext in. 
     */
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private  Cloudinary _cloudinary;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _context = context;
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
                );

            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "RequireAdminrole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            // Order by name and then we are going to project to a new object (we use select)
            // Construct a new object
            // Find out which roles the user belongs to and return the role name. We use not linq query but query expresion instead
            var userList = await _context.Users
                .OrderBy(x => x.UserName)
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Roles = (from userRole in u.UserRoles
                             join role in _context.Roles on userRole.RoleId
                             equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();
            return Ok(userList);
        }
        /*
         Edit the roles for a user
         */
        [Authorize(Policy = "RequireAdminrole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            // Get roles for this particullar user
            var userRoles = await _userManager.GetRolesAsync(user);

            // This roles are selected on the frontend
            var selectedRoles = roleEditDto.RoleNames;

            // taking in consideration that the user has been de-selected from all the roles.
            // If roles are empty create an empty object
            selectedRoles = selectedRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }
            
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove the roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "RequireModeratorRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var allPhotos = await _context.Photos
                .Include(u => u.User)
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.User.UserName,
                    Url = u.Url,
                    IsApproved = u.IsApproved
                }).ToListAsync();

            return Ok(allPhotos);
        }

        [Authorize(Policy = "RequireModeratorRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photoToApprove = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            photoToApprove.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok("Photo has been approved!");
        }

        [Authorize(Policy = "RequireModeratorRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photoToReject = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            // Unnecessary check, there is no way that the user will add the photo as main if it is not approved
            if (photoToReject.IsMain)
            {
                return BadRequest("Profile picture cannot be rejected");
            }

            // If it has a publicId, it means it is a cloudinary photo
            if (photoToReject.PublicPhotoId != null)
            {
                // Delete the photo from cloudinary
                var deleteParams = new DeletionParams(photoToReject.PublicPhotoId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _context.Photos.Remove(photoToReject);
                }
            }

            // If it is null, don`t need to delete it from cloudinary
            if (photoToReject.PublicPhotoId == null)
            {
                _context.Photos.Remove(photoToReject);
            }

            await _context.SaveChangesAsync();

            return Ok("Photo has been rejected!");
        }
    }
}
