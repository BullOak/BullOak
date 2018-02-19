namespace BullOak.Repositories.Config
{
    using BullOak.Repositories.Middleware;

    public static class InterceptorExtensions
    {
        public static TConfig WithInterceptor<TConfig>(this TConfig config, IInterceptEvents interceptor)
            where TConfig : IConfigureBullOak
        {
            config.AddInterceptor(interceptor);
            return config;
        }
    }
}
