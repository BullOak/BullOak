namespace BullOak.Repositories.EntityFramework.Test.Integration.DbModel
{
    using System.ComponentModel.DataAnnotations;

    public class HoldHighOrders : IHoldHighOrders
    {
        [Key]
        public string ClientId { get; set; }

        //SQLite does not support timestamping => no concurrency exceptions
        //[Timestamp]
        public byte[] RowVersion { get; set; }

        public int HigherOrder { get; set; }
    }
}
