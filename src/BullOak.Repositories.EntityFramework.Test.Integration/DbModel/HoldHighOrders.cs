namespace BullOak.Repositories.EntityFramework.Test.Integration.DbModel
{
    using System.ComponentModel.DataAnnotations;

    public class HoldHighOrders : IHoldHighOrders
    {
        [Key]
        public string ClientId { get; set; }
        public int HigherOrder { get; set; }
    }
}
