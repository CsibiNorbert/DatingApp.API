using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    interface IDatingRepository
    {
        // Generic type of method,  we constrain the method for just classes
        // Adding user/ adding photo
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        // When we save changes we send a boolean
        Task<bool> SaveAll();
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
    }
}
