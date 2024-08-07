namespace TeamSorting.Utils;

public static class SeedGenerator
{
    private static readonly Random Random = new();

    public static string CreateSeed(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}