using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using System;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Customers.Any())
            {
                return; // DB has been seeded
            }

            List<Customer> customer = new List<Customer>
            {
                new Customer { CompanyName = "Bad Company", RegistrationNumber = 1, EmailAddress = "bcompanyofficial@bcompany.com", PhoneNumber = "51223432", DefaultBillingAddress = "54 downingstreet 5400 London", DefaultShippingAddress = "54 downingstreet 5400 London" }
            };

            context.Customers.AddRange(customer);
            context.SaveChanges();
        }
    }
}
