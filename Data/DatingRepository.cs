using System;
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

        public async Task<Message> GetMessage(int messageId) {
            return await _context.Messages.FirstOrDefaultAsync(m=>m.Id == messageId);
        }
        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams) {
            // Include the sender to see who messaged us + include photos to see their photos
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos).AsQueryable(); // asQueryable because we use the where clause

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }

            // show the most recent messages based on the date
            messages = messages.OrderByDescending(d => d.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }
        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId) {
            // Get full conversation between two users. Returning IEnumerable
            var messages =await  _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId 
                    || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)
                .OrderByDescending(m => m.MessageSent).ToListAsync();

            return messages;
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

            if (userParams.Likers)
            {
                // get list of ids that the user has liked
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                // matches any of the user ids in the users table, then we return this
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                // matches any of the user ids in the users table, then we return this
                users = users.Where(u => userLikees.Contains(u.Id));
            }

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

        private async Task<IEnumerable<int>> GetUserLikes (int id, bool likers)
        {
            var user = await _context
                .Users.Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u=>u.Id == id);

            if (likers)
            {
                // This will return the likers of the currently logged in user
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }

        }
    }
}
