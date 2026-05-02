using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentNovel.Helpers;

public static class PageRangeParser
{
    public static List<int> Parse(string range, int maxPage)
    {
        var pages = new HashSet<int>();
        var parts = range.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var rangeParts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length == 2 &&
                    int.TryParse(rangeParts[0], out int start) &&
                    int.TryParse(rangeParts[1], out int end))
                {
                    for (int i = Math.Max(1, start); i <= Math.Min(maxPage, end); i++)
                    {
                        pages.Add(i);
                    }
                }
            }
            else if (int.TryParse(trimmed, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= maxPage)
                {
                    pages.Add(pageNum);
                }
            }
        }

        return pages.OrderBy(p => p).ToList();
    }
}
