namespace TeamSorting.Extensions;

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list, Random? rng = null)
    {
        rng ??= new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    public static void Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }
}