using ColabConcept.Web.Infrastructure;
using ColabConcept.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections;
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

        private static int _clients;
        private readonly IProductStore _productStore;

        public override System.Threading.Tasks.Task OnConnected()
        {
            Interlocked.Add(ref _clients, 1);

            Clients.Caller.connected(new
            {
                id = this.Context.ConnectionId,
                count = _clients,
                products = _productStore.GetAll()
            });

            Clients.Others.joined(new { id = this.Context.ConnectionId, count = _clients });

            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            Interlocked.Add(ref _clients, -1);

            foreach (var productId in _productStore.CancelEdits(this.Context.ConnectionId))
            {
                PublishProductUnlocked(productId);
            }

            Clients.All.left(new { id = this.Context.ConnectionId, count = _clients });

            return base.OnDisconnected();
        }
        
        public override System.Threading.Tasks.Task OnReconnected()
        {
            Interlocked.Add(ref _clients, 1);

            Clients.Caller.reconnected(new
            {
                id = this.Context.ConnectionId,
                count = _clients,
                products =_productStore.GetAll()
            });
            
            return base.OnReconnected();
        }

        public void CancelEdit(Guid productId)
        {
            if (_productStore.UnlockProduct(productId, this.Context.ConnectionId))
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
            if (_productStore.LockProduct(productId, Context.ConnectionId))
            {
                Clients
                .All
                .beginEdit(
                new
                {
                    editedBy = this.Context.ConnectionId,
                    product = productId.ToString()
                });

            }

        }

        public void CommitEdit(Product product)
        {
            product.LockedBy = this.Context.ConnectionId;

            if (_productStore.Update(product))
            {

                Clients
                  .All
                  .commitEdit(
                  new
                  {
                      editedBy = Context.ConnectionId,
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
                            By = Context.ConnectionId 
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
                    addedBy = Context.ConnectionId,
                    product = product
                });
        }


        private Product GetFromRepository(Guid productId)
        {
            return _productStore.Get(productId);
        }

    }
}