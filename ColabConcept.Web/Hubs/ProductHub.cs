using ColabConcept.Web.Infrastructure;
using ColabConcept.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace ColabConcept.Web.Hubs
{
    [HubName("products")]
    public class ProductHub : Hub
    {
        public ProductHub(IProductStore productStore)
        {
            _productStore = productStore;
        }

        private static ConcurrentDictionary<string, int> _connectedClients = new ConcurrentDictionary<string,int>();

        private readonly IProductStore _productStore;

        public override System.Threading.Tasks.Task OnConnected()
        {
            string id = this.Context.User.Identity.Name;

            int connectionCount = _connectedClients.AddOrUpdate(id, 1, (k, v) => v + 1);

            Clients.Caller.connected(new
            {
                id = id,
                count = _connectedClients.Count,
                products = _productStore.GetAll()
            });

            if (connectionCount == 1)
                Clients.Others.joined(new
                {
                    id = id,
                    count = _connectedClients.Count
                });

            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string id = this.Context.User.Identity.Name;

            int connectionCount;

            if (_connectedClients.TryGetValue(id, out connectionCount))
            {
                if (connectionCount == 1)
                {
                    _connectedClients.TryRemove(id, out connectionCount);
                }
                else if (!_connectedClients.TryUpdate(id, connectionCount - 1, connectionCount))
                {
                    // TODO: log
                }
            }

            foreach (var productId in _productStore.CancelEdits(id))
            {
                PublishProductUnlocked(productId);
            }

            if (connectionCount == 1)
                Clients.All.left(new { id = id, count = _connectedClients.Count });

            return base.OnDisconnected(stopCalled);
        }
        
        public override System.Threading.Tasks.Task OnReconnected()
        {
            Clients.Caller.reconnected(new
            {
                id = this.Context.User.Identity.Name,
                count = _connectedClients.Count,
                products =_productStore.GetAll()
            });
            
            return base.OnReconnected();
        }

        public void CancelEdit(Guid productId)
        {
            if (_productStore.UnlockProduct(productId, this.Context.User.Identity.Name))
            {
                PublishProductUnlocked(productId);
            }

        }

        private void PublishProductUnlocked(Guid productId)
        {
            Clients.All.productUnlocked(productId);
        }

        public void BeginEdit(Guid productId)
        {
            if (_productStore.LockProduct(productId, this.Context.User.Identity.Name))
            {
                Clients
                .All
                .beginEdit(
                new
                {
                    editedBy = this.Context.User.Identity.Name,
                    product = productId.ToString()
                });

            }

        }

        public void CommitEdit(Product product)
        {
            product.LockedBy = this.Context.User.Identity.Name;

            if (_productStore.Update(product))
            {

                Clients
                  .All
                  .commitEdit(
                  new
                  {
                      editedBy = this.Context.User.Identity.Name,
                      product = product
                  });
            }
        }

        public void RemoveProduct(Guid productId)
        {
            bool removed = 
                _productStore
                   .Remove(new Product
                   {
                       Id = productId
                   });

            if (removed)
            {
                IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ProductHub>();

                context
                    .Clients
                    .All
                    .removeProduct(
                        new 
                        { 
                            Id = productId,
                            By = this.Context.User.Identity.Name
                        });
            }
        }

        public void SaveProduct(Product product)
        {
            product.Id = Guid.NewGuid();

            _productStore
                .Add(product);

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ProductHub>();

            context.Clients.All.addProduct(
                new 
                {
                    addedBy = this.Context.User.Identity.Name,
                    product = product
                });
        }


        private Product GetFromRepository(Guid productId)
        {
            return _productStore.Get(productId);
        }

    }
}