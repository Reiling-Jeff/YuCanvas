using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace YuCanvas.Media;

public static class HtmlToInlines
{
    public static List<Inline> Convert(string html)
    {
        List<Inline> inlines = new List<Inline>();

        if (string.IsNullOrWhiteSpace(html))
            return inlines;

        html = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"</p>", "\n\n", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"</li>", "\n", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<li>", "•  ", RegexOptions.IgnoreCase);

        string pattern = @"(<(?:strong|b)>.*?</(?:strong|b)>)";
        string[] parts = Regex.Split(html, pattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            bool isBold = Regex.IsMatch(part, @"^<(?:strong|b)>",
                RegexOptions.IgnoreCase);

            string text = Regex.Replace(part, @"<[^>]+>", "");
            text = WebUtility.HtmlDecode(text);

            // Mehr als eine Leerzeile am Stück auf eine reduzieren
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            // Nicht-brechende Leerzeichen zu normalen machen
            text = text.Replace('\u00A0', ' ');

            if (string.IsNullOrWhiteSpace(text))
                continue;

            Run run = new Run(text);
            if (isBold)
                run.FontWeight = FontWeight.Bold;

            inlines.Add(run);
        }

        return inlines;
    }
}
