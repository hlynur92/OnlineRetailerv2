using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;
        IBus bus;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkCompleted",
                    HandleOrderCompleted, x => x.WithTopic("completed"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkCancelled",
                    HandleOrderCanceled, x => x.WithTopic("cancelled"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkShipped",
                    HandleOrderShipped, x => x.WithTopic("shipped"));

                //bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkPaid",
                    //HandleOrderPaid, x => x.WithTopic("paid"));

                // Add code to subscribe to other OrderStatusChanged events:
                // * cancelled
                // * shipped
                // * paid
                // Implement an event handler for each of these events.
                // Be careful that each subscribe has a unique subscription id
                // (this is the first parameter to the Subscribe method). If they
                // get the same subscription id, they will listen on the same
                // queue.

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }
        /*
        private void HandleOrderPaid(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;

            }
        }
        */
        private void HandleOrderShipped(OrderStatusChangedMessage message)
        {
            //when the ordered items are shipped, the reservations are removed,
            //and the number of items in stock are decremented for each ordered product.

            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    product.ItemsInStock -= orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderCanceled(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        /*
        public void Start()
        {
            using (bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<OrderCreatedMessage>("productApiHkCreated", 
                    HandleOrderCreated);

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }*/

        /*
        private void HandleOrderCreated(OrderCreatedMessage message)
        {
            Console.WriteLine("received and handling OrderCreatedMessage");
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                if (ProductItemsAvailable(message.OrderLines, productRepos))
                {
                    // Reserve items and publish an OrderAcceptedMessage
                    foreach (var orderLine in message.OrderLines)
                    {
                        var product = productRepos.Get(orderLine.ProductId);
                        product.ItemsReserved += orderLine.Quantity;
                        productRepos.Edit(product);
                    }

                    var replyMessage = new OrderAcceptedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                    Console.WriteLine("Published OrderAcceptedMessage");
                }
                else
                {
                    // Publish an OrderRejectedMessage
                    var replyMessage = new OrderRejectedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                    Console.WriteLine("Published OrderRejectedMessage");
                }
            }
            Console.WriteLine("Doen handling OrderCreatedMessage");
        }

        // Moved to Order Controller
        private bool ProductItemsAvailable(IList<OrderLine> orderLines, IRepository<Product> productRepos)
        {
            foreach (var orderLine in orderLines)
            {
                var product = productRepos.Get(orderLine.ProductId);
                if (orderLine.Quantity > product.ItemsInStock - product.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }*/
    }
}
