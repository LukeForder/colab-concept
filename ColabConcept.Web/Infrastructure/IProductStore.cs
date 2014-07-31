using ColabConcept.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColabConcept.Web.Infrastructure
{
    public interface IProductStore
    {
        void Add(Product product);

        bool Update(Product product);

        bool LockProduct(Guid productId, string lockedBy);

        bool Remove(Product product);

        Product Get(Guid id);

        IEnumerable<Product> GetAll();
    }
}
