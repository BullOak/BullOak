namespace BullOak.Repositories.Test.Unit.Session
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Xunit;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Session;
    using System.Threading;
    using BullOak.Repositories.Repository;
    using FluentAssertions;

    public class InterceptorSpecs
    {
        public interface IState
        {
            int Number { get; set; }
        }

        public class EnqueueMethodCallInterceptor : IInterceptEvents
        {
            private readonly ConcurrentQueue<string> queue;

            public EnqueueMethodCallInterceptor(ConcurrentQueue<string> queue)
                => this.queue = queue ?? throw new ArgumentNullException();

            public void AfterPublish(object @event, Type typeOfEvent, object state, Type typeOfState)
                => queue.Enqueue(nameof(AfterPublish));

            public void AfterSave(object @event, Type typeOfEvent, object state, Type typeOfState)
                => queue.Enqueue(nameof(AfterSave));

            public void BeforePublish(object @event, Type typeOfEvent, object state, Type typeOfState)
                => queue.Enqueue(nameof(BeforePublish));

            public void BeforeSave(object @event, Type typeOfEvent, object state, Type typeOfState)
                => queue.Enqueue(nameof(BeforeSave));
        }

        public IStartSessions<int, IState> GetSUT(ConcurrentQueue<string> queue)
            => new StubRepo(new ConfigurationStub<IState>()
                .WithDefaultSetup()
                .WithEventPublisher(new MySyncEventPublisher(o => queue.Enqueue(nameof(IPublishEvents.Publish))))
                .WithInterceptor(new EnqueueMethodCallInterceptor(queue)), queue);

        public struct Indexes
        {
            public int beforePublish;
            public int publish;
            public int afterPublish;
            public int beforeSave;
            public int save;
            public int afterSave;
        }

        public class StubRepo : IStartSessions<int, IState>
        {
            private Action<string> OnSave { get; }
            private IHoldAllConfiguration Configuration { get; }

            public StubRepo(IHoldAllConfiguration config, ConcurrentQueue<string> calls)
            {
                OnSave = calls.Enqueue;
                Configuration = config;
            }

            public Task<IManageSessionOf<IState>> BeginSessionFor(int id, bool throwIfNotExists)
                => Task.FromResult<IManageSessionOf<IState>>(new StubSession(Configuration, OnSave));

            public Task<bool> Contains(int selector) => throw new NotImplementedException();
            public Task Delete(int selector) => throw new NotImplementedException();
        }

        public class StubSession : BaseRepoSession<IState>
        {
            private Action<string> OnSave { get; }

            public StubSession(IHoldAllConfiguration config, Action<string> onSave)
                :base(config, new MemoryStream())
                => OnSave = onSave;

            protected override Task<int> SaveChanges(ItemWithType[] newEvents,
                IState currentState,
                CancellationToken? cancellationToken)
            {
                OnSave(nameof(SaveChanges));
                return Task.FromResult(0);
            }
        }

        public async Task<Indexes> DoWithGuarantee(DeliveryTargetGuarantee guarantee)
        {
            ConcurrentQueue<string> calls = new ConcurrentQueue<string>();
            var sut = GetSUT(calls);

            using (var session = await sut.BeginSessionFor(0, false))
            {
                session.AddEvent(new object());
                await session.SaveChanges(guarantee);
            }

            var methodCalls = calls.ToList();

            return new Indexes
            {
                beforePublish = methodCalls.IndexOf(nameof(IInterceptEvents.BeforePublish)),
                publish = methodCalls.IndexOf(nameof(IPublishEvents.Publish)),
                afterPublish = methodCalls.IndexOf(nameof(IInterceptEvents.AfterPublish)),
                beforeSave = methodCalls.IndexOf(nameof(IInterceptEvents.BeforeSave)),
                save = methodCalls.IndexOf(nameof(IManageSessionOf<IState>.SaveChanges)),
                afterSave = methodCalls.IndexOf(nameof(IInterceptEvents.AfterSave))
            };
        }

        [Fact]
        public async Task SaveSession_OneEvent_BeforePublishInterceptorMethodIsCalledBeforeEventPublish()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtLeastOnce);

            //Assert
            indexes.beforePublish.Should().BeLessThan(indexes.publish);
        }

        [Fact]
        public async Task SaveSession_OneEvent_AfterPublishInterceptorMethodIsCalledAfterEventPublish()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtLeastOnce);

            //Assert
            indexes.afterPublish.Should().BeGreaterThan(indexes.publish);
        }

        [Fact]
        public async Task SaveSession_OneEventWithAtLeastOnce_BeforeSaveInterceptorMethodIsCalledAfterPublish()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtLeastOnce);

            //Assert
            indexes.beforeSave.Should().BeGreaterThan(indexes.afterPublish);
        }

        [Fact]
        public async Task SaveSession_OneEvent_BeforeSaveInterceptorMethodIsCalledBeforeSave()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtLeastOnce);

            //Assert
            indexes.beforeSave.Should().BeLessThan(indexes.save);
        }

        [Fact]
        public async Task SaveSession_OneEvent_AfterInterceptorMethodIsCalledAfterSave()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtLeastOnce);

            //Assert
            indexes.afterSave.Should().BeGreaterThan(indexes.save);
        }

        [Fact]
        public async Task SaveSession_OneEventWithAtMostOnce_BeforePublishInterceptorMethodIsCalledAfterSave()
        {
            //Arrange

            //Act
            var indexes = await DoWithGuarantee(DeliveryTargetGuarantee.AtMostOnce);

            //Assert
            indexes.beforePublish.Should().BeGreaterThan(indexes.afterSave);
        }
    }
}
