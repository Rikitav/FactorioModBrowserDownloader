using FactorioNexus.Infrastructure.Services;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.UserInterface.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactorioNexus.Utilities
{
    public static class ServiceCollectionExtensions
    {
        public static IViewModelLocatorBuilder AddViewModel<TViewModel, TFrameworkElement>(this IViewModelLocatorBuilder builder) where TViewModel : class, IViewModel<TFrameworkElement> where TFrameworkElement : FrameworkElement
        {
            builder.ModelsMap.Add(typeof(TFrameworkElement), typeof(TViewModel));
            builder.Services.AddSingleton<IViewModel<TFrameworkElement>, TViewModel>();
            return builder;
        }

        public static IServiceCollection AddViewModelLocator(this IServiceCollection services, Action<IViewModelLocatorBuilder> configure)
        {
            ViewmodelLocatorBuilder builder = new ViewmodelLocatorBuilder(services);
            configure.Invoke(builder);

            ViewModelLocator locator = new ViewModelLocator(builder.ModelsMap);
            services.AddSingleton<IViewModelLocator>(locator);
            return services;
        }

        public static TService ResolveRequired<TService>(this IServiceProvider serviceProvider, params object[] args)
        {
            return ActivatorUtilities.CreateInstance<TService>(serviceProvider, args);
        }
    }
}
