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
        private List<User> users = new List<User>();

        public void Save(User user)
        {
            var current = users.FirstOrDefault(i => i.Id == user.Id);
            if (current != null)
                users.Remove(current);

            users.Add(user);
        }

        public int GetByIdCount = 0;
        public int ListCount = 0;
        public int ListParamsCount = 0;

        public User GetById(Guid id)
        {
            GetByIdCount++;
            return users.FirstOrDefault(i => i.Id == id);
        }


        public IList<User> List(IEnumerable<Guid> ids)
        {
            ListCount++;
            return this.users.Where(u => ids.Contains(u.Id)).ToList();
        }

        public IList<User> ListParams(params Guid[] ids)
        {
            ListParamsCount++;
            return this.users.Where(u => ids.Contains(u.Id)).ToList();
        }
    }
}