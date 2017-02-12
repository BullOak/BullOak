namespace BullOak.Application
{
    using System;
    using System.Collections.Generic;
    using BullOak.Common;
    using Messages;
    using MethodBuilderContainer;

    public abstract class ChildEntity<TId, TParent> : Entity<TId>, IHaveAParent
        where TId : IId, IEquatable<TId>
        where TParent : Entity
    {
        public TParent Parent { get; private set; }

        private List<Action<TParent>> CachedParentActions { get; } = new List<Action<TParent>>();

        private static readonly ICacheMethods trySetChildMethodCache;

        static ChildEntity()
        {
            trySetChildMethodCache =
                new CachedMethodWithTypeSelectorBuilder(typeof(ChildAssigner).GetMethod(nameof(ChildAssigner.TrySetChildInParent)),
                    types => new[] { types[1], typeof(TId) });
        }

        protected ChildEntity()
        { }

        protected ChildEntity(TParent parent)
        {
            SetupFromParent(parent);
        }

        public void SetParent(TParent parent) => SetupFromParent(parent);
        //The below is for internal use only during reconstitution.
        void IHaveAParent.SetParent(Entity parent)
        {
            var typedParent = parent as TParent;

            if (typedParent == null)
            {
                //TODO (Savvas): replace with exception
                throw new Exception("Provided parent is not of expected type.");
            }

            SetupFromParent(typedParent);
        }

        private void SetupFromParent(TParent parent)
        {
            //If parent is already set continue on.
            if (parent == Parent) return;
            if (Parent != null)
            {
                //TODO (Savvas): replace with exception
                throw new Exception("Parent already set. Cannot move to another parent.");
            }
            if (parent == null)
            {
                //TODO (Savvas): replace with exception
                throw new Exception("Parent cannot be null and cannot be unset once set.");
            }

            Parent = parent;

            //If we have cached any actions that happened prior to assigning of the parent we now act them on the parent 
            // in order they occured.
            // This can happen for example when initializing the Entity (ie publishing an initialization event)
            // from within the ctor, but before calling the method SetParent on the child and not providing a parent in ctor.
            foreach (var storeEventActions in CachedParentActions)
            {
                storeEventActions(Parent);
            }
        }

        internal sealed override void ApplyInternal<TEvent>(TEvent eventEnvelope, bool addToStream)
        {
            base.ApplyInternal(eventEnvelope, addToStream);

            if (Parent != null)
            {
                trySetChildMethodCache.Invoke(null, Parent, this);
            }
            else
            {
                CachedParentActions.Add(parent => trySetChildMethodCache.Invoke(null, Parent, this));
            }
        }

        internal override void TryStoreSelfInAggregate<TEntityId>(Entity<TEntityId> entity)
        {
            if (Parent != null)
            {
                Parent.TryStoreSelfInAggregate(entity);
            }
            else
            {
                CachedParentActions.Add(parent => parent.TryStoreSelfInAggregate(entity));
            }
        }

        internal override void StoreEventInStream(Lazy<IParcelVisionEventEnvelope> lazyEventEnvelope)
        {
            if (Parent != null)
            {
                Parent.StoreEventInStream(lazyEventEnvelope);
            }
            else
            {
                CachedParentActions.Add(parent => parent.StoreEventInStream(lazyEventEnvelope));
            }
        }

        internal override Lazy<IParcelVisionEventEnvelope> GetEnvelopeFor<TEvent>(TEvent @event)
        {
            if (Parent != null)
            {
                var envelope = Parent.CreateParentEnvelope<TId, TEvent>(@event);
                envelope.SourceId = Id;
                envelope.SourceEntityType = GetType();

                return new Lazy<IParcelVisionEventEnvelope>(() => envelope);
            }

            return new Lazy<IParcelVisionEventEnvelope>(() =>
            {
                if (Parent == null) throw new Exception("Parent must be set on child entity before calling save.");

                var envelope = Parent.CreateParentEnvelope<TId, TEvent>(@event);
                envelope.SourceId = Id;
                envelope.SourceEntityType = GetType();

                return envelope;
            });
        }
    }
}