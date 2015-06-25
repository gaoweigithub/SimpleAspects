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
        void Save(User user);
    }

    public class UserRepository : IUserRepository
    {
        private List<User> users = new List<User>();

        public void Save(User user)
        {
            var current = users.FirstOrDefault(i => i.Id == user.Id);
            if (current != null)
                users.Remove(current);

            users.Add(user);
        }

        public int GetByIdCount = 0;

        public User GetById(Guid id)
        {
            GetByIdCount++;
            return users.FirstOrDefault(i => i.Id == id);
        }
    }
}