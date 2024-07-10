using TeamSorting.ViewModel;
using Serilog;
using TeamSorting.Model;

namespace TeamSorting;

class Program
{
    static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }
}