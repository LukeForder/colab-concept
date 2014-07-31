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
        private static int _clients;

        public override System.Threading.Tasks.Task OnConnected()
        {
            Interlocked.Add(ref _clients, 1);

            Clients.Caller.connected(new
            {
                id = this.Context.ConnectionId,
                count = _clients,
                products = new ProductStore().GetAll()
            });

            Clients.Others.joined(new { id = this.Context.ConnectionId, count = _clients });

            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            Interlocked.Add(ref _clients, -1);

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
                products = new ProductStore().GetAll()
            });

            Clients.Others.joined(new { id = this.Context.ConnectionId, count = _clients });

            return base.OnReconnected();
        }

        public void BeginEdit(Guid productId)
        {
            if (new ProductStore().LockProduct(productId, Context.ConnectionId))
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
            new
                ProductStore()
                .Update(product);

            Clients
              .All
              .commitEdit(
              new
              {
                  editedBy = Context.ConnectionId,
                  product = product
              });
        }

        public void RemoveProduct(Guid productId)
        {
            bool removed = 
                new ProductStore()
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

            new
                ProductStore()
                .Add(product);

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ProductHub>();

            context.Clients.All.addProduct(product);
        }


        private Product GetFromRepository(Guid productId)
        {
            return new ProductStore().Get(productId);
        }

    }
}