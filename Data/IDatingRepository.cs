using DatingApp.API.Helpers;
using DatingApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
        // Generic type of method,  we constrain the method for just classes
        // Adding user/ adding photo
        void Add<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;

        // When we save changes we send a boolean
        Task<bool> SaveAll();

        Task<PagedList<User>> GetUsers(UserParams userParams);

        Task<User> GetUser(int id, bool isCurrentUser);

        Task<Photo> GetPhoto(int id);

        Task<Photo> GetMainPhoto(int userId);

        Task<Like> GetLike(int userId, int recipient);

        Task<Message> GetMessage(int messageId);

        // We return pagedlist because we are going to add pagination to the messages
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);

        // This is the conversation between two users
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}