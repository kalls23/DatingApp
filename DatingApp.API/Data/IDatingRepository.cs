using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         public void Add<T>(T entity) where T: class;

         public void Delete<T>(T entity) where T: class;

         public Task<bool> SaveAll();

         public Task<IEnumerable<User>> GetUsers();

         public Task<User> GetUser(int id);

         public Task<Photo> GetPhoto(int id);

         public Task<Photo> GetMainPhotoForUser(int userId);

    }
}