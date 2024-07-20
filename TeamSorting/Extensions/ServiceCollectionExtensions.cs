using Microsoft.Extensions.DependencyInjection;
using TeamSorting.Sorting;
using TeamSorting.ViewModels;

namespace TeamSorting.Extensions;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<Data>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddSingleton<InputViewModel>();
        collection.AddSingleton<TeamsViewModel>();
        // collection.AddSingleton<ISorter,EvolutionSorter>();
    }
}