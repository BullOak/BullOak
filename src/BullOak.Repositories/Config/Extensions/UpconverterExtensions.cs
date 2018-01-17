namespace BullOak.Repositories.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BullOak.Repositories.Upconverting;

    public static class UpconverterExtensions
    {
        public static IBuildConfiguration WithNoUpconverters(this IConfigureUpconverter config)
            => config.WithUpconverter(new NullUpconverter());

        public static IConfigureUpconverters WithUpconvertersFrom(this IConfigureUpconverter config,
            Assembly assembly)
            => (new UpconverterConfig(config)).WithUpconvertersFrom(assembly);

        public static IConfigureUpconverters WithUpconvertersFrom(this IConfigureUpconverter config,
            IEnumerable<Type> types)
            => (new UpconverterConfig(config)).WithUpconvertersFrom(types);

        public static IConfigureUpconverters WithUpconverter<TUpconverter>(this IConfigureUpconverter config)
            => (new UpconverterConfig(config)).WithUpconverter<TUpconverter>();
    }

    internal class UpconverterConfig : IConfigureUpconverters
    {
        private readonly HashSet<Type> discoveredTypes = new HashSet<Type>();

        private readonly IConfigureUpconverter config;

        public UpconverterConfig(IConfigureUpconverter config)
            => this.config = config ?? throw new ArgumentNullException(nameof(config));

        public IBuildConfiguration AndNoMoreUpconverters() =>
            config.WithUpconverter((EventUpconverter)UpconverterCompiler.GetFrom(discoveredTypes));

        public IConfigureUpconverters WithUpconvertersFrom(Assembly assembly)
        {
            if(assembly == null) throw new ArgumentNullException(nameof(assembly));

            discoveredTypes.UnionWith(assembly.GetTypes());
            return this;
        }

        public IConfigureUpconverters WithUpconvertersFrom(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            discoveredTypes.UnionWith(types);
            return this;
        }

        public IConfigureUpconverters WithUpconverter<TUpconverter>()
        {
            discoveredTypes.UnionWith(new[] {typeof(TUpconverter)});
            return this;
        }
    }
}