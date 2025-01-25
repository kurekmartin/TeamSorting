using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace TeamSorting.Utils;

public static class TextParser
{
    /// <summary>
    /// Supports only bold using Markdown syntax **
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static InlineCollection Parse(string text)
    {
        string[] boldParts = text.Split("**");
        var inlineCollection = new InlineCollection();
        var isBold = false;
        foreach (string part in boldParts)
        {
            if (!isBold)
            {
                inlineCollection.Add(part);
            }
            else
            {
                inlineCollection.Add(new Run(part) { FontWeight = FontWeight.Bold });
            }

            isBold = !isBold;
        }

        return inlineCollection;
    }
}