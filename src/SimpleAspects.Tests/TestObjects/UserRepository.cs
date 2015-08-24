using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tests
{
    public interface IUserRepository
    {
        [MemoryCacheAspectAttribute]
        User GetById(Guid id);

        [MemoryCacheAspectAttribute]
        IList<User> List(IEnumerable<Guid> ids);

        [MemoryCacheAspectAttribute]
        IList<User> ListParams(params Guid[] ids);

        void Save(User user);
    }

    public class UserRepository : IUserRepository
    {
        public UserRepository()
        {
        }
        public UserRepository(IList<User> users)
        {
            this.Users = users;
        }

        public readonly IList<User> Users = new List<User>();

        public void Save(User user)
        {
            var current = Users.FirstOrDefault(i => i.Id == user.Id);
            if (current != null)
                Users.Remove(current);

            Users.Add(user);
        }

        public int GetByIdCount = 0;
        public int ListCount = 0;
        public int ListParamsCount = 0;

        public User GetById(Guid id)
        {
            GetByIdCount++;
            return Users.FirstOrDefault(i => i.Id == id);
        }


        public IList<User> List(IEnumerable<Guid> ids)
        {
            ListCount++;
            return this.Users.Where(u => ids.Contains(u.Id)).ToList();
        }

        public IList<User> ListParams(params Guid[] ids)
        {
            ListParamsCount++;
            return this.Users.Where(u => ids.Contains(u.Id)).ToList();
        }
    }
}