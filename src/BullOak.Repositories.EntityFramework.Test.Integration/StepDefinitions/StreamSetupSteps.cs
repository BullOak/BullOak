namespace BullOak.Repositories.EntityFramework.Test.Integration.StepDefinitions
{
    using System;
    using System.Linq;
    using BullOak.Repositories.EntityFramework.Test.Integration.Contexts;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using TechTalk.SpecFlow;

    [Binding]
    internal class StreamSetupSteps
    {
        private EventGenerator eventGenerator;
        private ClientIdContainer clientIdInfo;
        private TestContextContainer contextContainer;

        public StreamSetupSteps(EventGenerator eventGenerator, ClientIdContainer clientIdInfo,
            TestContextContainer contextContainer)
        {
            this.eventGenerator = eventGenerator;
            this.clientIdInfo = clientIdInfo;
            this.contextContainer = contextContainer;
        }

        [Given(@"an existing entity with HigherOrder (.*)")]
        public void GivenAnExistingEntityWithHigherOrder(int highOrderValue)
        {
            contextContainer.TestContext.Orders.Add(new HoldHighOrders()
            {
                ClientId = clientIdInfo.Id,
                HigherOrder = highOrderValue
            });

            contextContainer.TestContext.SaveChanges();
        }

        [Given(@"no existing entity")]
        public void GivenNoExistingEntity()
        {
            var orderOrDefault = contextContainer.TestContext.Orders
                .FirstOrDefault(x => x.ClientId == clientIdInfo.Id);

            if (orderOrDefault != null)
            {
                contextContainer.TestContext.Orders.Remove(orderOrDefault);
                contextContainer.TestContext.SaveChanges();
            }
        }
    }
}
