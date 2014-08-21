using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColabConcept.Web.Infrastructure
{
    public interface IUserStore
    {
        Task<bool> ExistsAsync(string userName);

        Task<Guid> AddAsync(string userName);
    }
}
