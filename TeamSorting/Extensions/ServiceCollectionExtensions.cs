using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TeamSorting.Models;
using TeamSorting.Sorting;
using TeamSorting.Utils;
using TeamSorting.ViewModels;

namespace TeamSorting.Extensions;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<Data>();
        collection.AddSingleton<Members>();
        collection.AddSingleton<Disciplines>();
        collection.AddSingleton<Teams>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddSingleton<InputViewModel>();
        collection.AddSingleton<TeamsViewModel>();
        collection.AddSingleton<ISorter,EvolutionSorter>();
        collection.AddSingleton<CsvUtil>();
        collection.AddLogging(builder => builder.AddSerilog(dispose: true));
    }

    //Fix for circular dependency
    public static void AddLazyResolution(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(Lazy<>),
            typeof(LazilyResolved<>));
    }

    private class LazilyResolved<T>(IServiceProvider serviceProvider) : Lazy<T>(serviceProvider.GetRequiredService<T>) where T : notnull;
}