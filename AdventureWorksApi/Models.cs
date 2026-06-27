namespace AdventureWorksApi
{
    public class Product
    {
        public int ProductID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ProductNumber { get; set; } = string.Empty;

        public decimal StandardCost { get; set; }

        public decimal ListPrice { get; set; }

        public string? Color { get; set; }

        public decimal? Weight { get; set; }
    }
    public class Customer
    {
        public int CustomerID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? EmailAddress { get; set; }

        public string? Phone { get; set; }
    }
}
