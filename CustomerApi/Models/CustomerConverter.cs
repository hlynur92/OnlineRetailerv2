using SharedModels;

namespace CustomerApi.Models
{
    public class CustomerConverter : IConverter<Customer, CustomerDto>
    {
        public Customer Convert(CustomerDto sharedCustomer)
        {
            return new Customer
            {
                Id = sharedCustomer.Id,
                CompanyName = sharedCustomer.CompanyName,
                RegistrationNumber = sharedCustomer.RegistrationNumber,
                EmailAddress = sharedCustomer.EmailAddress,
                PhoneNumber = sharedCustomer.PhoneNumber,
                DefaultBillingAddress = sharedCustomer.DefaultBillingAddress,
                DefaultShippingAddress = sharedCustomer.DefaultShippingAddress
            };
        }

        public CustomerDto Convert(Customer hiddenCustomer)
        {
            return new CustomerDto
            {
                Id = hiddenCustomer.Id,
                CompanyName = hiddenCustomer.CompanyName,
                RegistrationNumber = hiddenCustomer.RegistrationNumber,
                EmailAddress = hiddenCustomer.EmailAddress,
                PhoneNumber = hiddenCustomer.PhoneNumber,
                DefaultBillingAddress = hiddenCustomer.DefaultBillingAddress,
                DefaultShippingAddress = hiddenCustomer.DefaultShippingAddress
            };
        }
    }
}
