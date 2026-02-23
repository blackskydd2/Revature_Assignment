using MyApp;

namespace MyApp.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Add_TwoNumbers_GivesCorrectResult(int x, int y, int expectedResult)
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var actualResult = calculator.Add(x, y);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, -1)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, -2)]
    public void Subtract_TwoNumbers_GivesCorrectResult(int x, int y, int expectedResult)
    {
        var calculator = new Calculator();
        var actualResult = calculator.Subtract(x, y);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(6, 3, 2)]
    [InlineData(10, 2, 5)]
    [InlineData(-8, 2, -4)]
    public void Divide_TwoNumbers_GivesCorrectResult(double x, double y, double expectedResult)
    {
        var calculator = new Calculator();
        var actualResult = calculator.Divide(x, y);
        Assert.Equal(expectedResult, actualResult);
    }

    // Optional: explicitly test divide by zero behavior
    [Theory]
    [InlineData(10, 0, double.PositiveInfinity)]
    [InlineData(-10, 0, double.NegativeInfinity)]
    public void Divide_ByZero_ReturnsInfinity(double x, double y, double expectedResult)
    {
        var calculator = new Calculator();
        var actualResult = calculator.Divide(x, y);
        Assert.Equal(expectedResult, actualResult);
    }

}