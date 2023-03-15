using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        IOrderRepository repository;
        IServiceGateway<ProductDto> productServiceGateway;
        IServiceGateway<CustomerDto> customerServiceGateway;
        IMessagePublisher messagePublisher;
        

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> pgateway,
            IServiceGateway<CustomerDto> cgateway,
            IMessagePublisher publisher)
        {
            repository = repos as IOrderRepository;
            productServiceGateway = pgateway;
            customerServiceGateway = cgateway;
            messagePublisher = publisher;
        }

        // GET orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            CustomerDto customer;
            try {
                if(order.customerId != null) customer = customerServiceGateway.Get(order.customerId ?? 0);
                else return StatusCode(500, "customerId is required");
            } catch (Exception) {
                // If the customer does not exist.
                return StatusCode(500, "Customer does not exist");
            }


            if (ProductItemsAvailable(order))
            {
                try
                {
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        order.customerId, order.OrderLines, "completed");

                    // Create order.
                    order.Status = Order.OrderStatus.completed;
                    var newOrder = repository.Add(order);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                // If there are not enough product items available.
                return StatusCode(500, "Not enough items in stock.");
            }
        }

        private bool ProductItemsAvailable(Order order)
        {
            foreach (var orderLine in order.OrderLines)
            {
                // Call product service to get the product ordered.
                var orderedProduct = productServiceGateway.Get(orderLine.ProductId);
                if (orderLine.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }


        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            var order = repository.Get(id);
            messagePublisher.PublishOrderStatusChangedMessage(
                order.customerId, order.OrderLines, "cancelled");


            // Edit order.
            order.Status = Order.OrderStatus.cancelled;
            repository.Edit(order);
            return StatusCode(200, "Order Cancelled");
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            var order = repository.Get(id);
            messagePublisher.PublishOrderStatusChangedMessage(
                order.customerId, order.OrderLines, "shipped");


            // Edit order.
            order.Status = Order.OrderStatus.cancelled;
            repository.Edit(order);
            return StatusCode(200, "Order Shipped");
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            /*
            var order = repository.Get(id);
            messagePublisher.CreditStandingChangedMessage(
                order.customerId, order.OrderLines, "paid");


            // Edit order.
            order.Status = Order.OrderStatus.cancelled;
            repository.Edit(order);
            return StatusCode(200, "Order Paid");
            */

            throw new NotImplementedException();
        }
    }
}
