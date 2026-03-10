namespace WebAPI.Tests;

using WebAPI;
using DataAccessLayer;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

public class UnitTest1
{
    [Fact]
    //[Theory]
    //[InlineData(1, 2, 3, 4)]
    public async void Customer_CalculateInterest_ReturnsCorrectValue()
    // public void Customer_CalculateInterest_ReturnsCorrectValue(int duration, int principal, int rate, int expectedInterest)
    {
        // Arrange
        // controller, services
        ICustomerService customerService = A.Fake<ICustomerService>();

        var customers = new List<Customer>
        {
            new Customer
            {
                Id = 3,
                Name = "Sarah Smith",
                Email = "sarah.smith@example.com"
            }
        };

        A.CallTo(() => customerService.GetAllCustomers()).Returns(customers);

        var customerController = new CustomerController(customerService);

        var expectedResult = new
        {
            StatusCode = 200,
            Value = customers
        };

        // Act
        var actualResult = customerController.Get();

        var okResult = actualResult as OkObjectResult;
        var actualCustomers = okResult?.Value as List<Customer>;

        // Assert
        // Check of Status Code is 200
        // Check of Interest is correct
        Assert.Equal(expectedResult.StatusCode, okResult?.StatusCode);

        // Assert collections are equal

        Assert.Equal(expectedResult.Value, actualCustomers);
    }
}