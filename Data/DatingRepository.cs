﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        // We don`t use async because we don`t querying the context
        // This is saved in memory until we actually save changes
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipient)
        {
            // Getting the like for an X user who liked another user
            return await _context.Likes
                .FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipient);
        }

        public async Task<Photo> GetMainPhoto(int userId)
        {
            // For X user get main photo
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p=>p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p=>p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            // When retrieve the user, we retrieve his photos as well.
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            // returning default means null if the user doesn`t matches his id
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            // This filters out the current user
            users = users.Where(u => u.Id != userParams.UserId);
            // This filters out genders
            users = users.Where(u => u.Gender == userParams.Gender);

            // Min & Max age in the query parameters
            if (userParams.MinAge != 18 || userParams.Maxage != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.Maxage - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                // if this is passed as a query parameter
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.CreatedProfile);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            // We return a page list of users, but before we do, we create it first
            return await PagedList<User>.CreateAsync(users,userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            // if this return more than 0 we return true
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
