using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services.Search;

public class SearchResult : IComparable<SearchResult>
{
    private readonly string _searchString;
    public Casefile Casefile { get; }
    public string SearchQuery { get; } = string.Empty;

    public SearchResult(Casefile casefile, string searchQuery)
    {
        Casefile = casefile;
        SearchQuery = searchQuery;
        _searchString = casefile.CourtFileNumber != null ?
            $"{casefile.CasefileNumber}{casefile.CourtFileNumber}" :
            casefile.CasefileNumber;
    }

    public int CompareTo(SearchResult? other)
    {
        if (other is null)
        {
            return -1;
        }

        return CalculateEditDistance(_searchString, other._searchString);
    }

    // An implementation of the Levenshtein edit distance algorithm
    private static int CalculateEditDistance(string s1, string s2)
    {
        var n = s1.Length;
        var m = s2.Length;
        var prevRow = new int[m + 1];
        var currRow = new int[m + 1];

        for (int j = 0; j <= m; j++)
        {
            prevRow[j] = j;
        }

        // Dynamic programming to calculate Levenshtein distance
        for (int i = 1; i <= n; i++)
        {
            // Initialise the current row witht he value of i
            currRow[0] = i;

            for (int j = 1; j <= m; j++)
            {
                // If characters are the same, no operation is needed
                if (s1[i - 1] == s2[j - 1])
                {
                    currRow[j] = prevRow[j - 1];
                }
                else
                {
                    // Choose the minimum of 3 operations: insert, replace, remove
                    currRow[j] = 1 + Math.Min(
                        currRow[j - 1], // insert
                        Math.Min(
                            prevRow[j], // remove
                            prevRow[j - 1])); // replace
                }
            }

            Array.Copy(currRow, prevRow, m + 1);
        }

        return currRow[m];
    }
}