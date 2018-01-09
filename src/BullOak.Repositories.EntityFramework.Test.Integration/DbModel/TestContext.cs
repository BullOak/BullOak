namespace BullOak.Repositories.EntityFramework.Test.Integration.DbModel
{
    using System.Data.Entity;

    public class TestContext : DbContext
    {
        public DbSet<HoldHighOrders> Orders { get; set; }

        public TestContext()
        { }
    }
}