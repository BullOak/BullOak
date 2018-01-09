namespace BullOak.Repositories.EntityFramework.Test.Integration.DbModel
{
    using System;

    public interface IHoldHighOrders
    {
        string ClientId { get; set; }
        int HigherOrder { get; set; }
    }
}