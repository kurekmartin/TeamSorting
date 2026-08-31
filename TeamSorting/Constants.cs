namespace TeamSorting;

public class Constants
{
 public const string DisciplineColumnTagPrefix = "Discipline-";
 public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TeamSorting");
 public static string LogDirectory => Path.Combine(DataDirectory, "Logs");
}
