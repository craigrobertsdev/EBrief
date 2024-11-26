using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services.Search;

public class SearchService
{
    public SearchTrie SearchTrie { get; private set; }
    private readonly Casefile[] _casefiles;

    public SearchService(CourtList courtList)
    {
        SearchTrie = BuildTrie(courtList);
        var courtListCasefiles = courtList.GetCasefiles();
        var casefiles = new Casefile[courtListCasefiles.Count];
        courtList.GetCasefiles().CopyTo(casefiles, 0);
        foreach (var cf in casefiles)
        {
            cf.CasefileNumber = cf.CasefileNumber.ToUpper();
            cf.CourtFileNumber = cf.CourtFileNumber?.ToUpper();
        }

        _casefiles = [..casefiles];
    }

    public List<SearchResult> Find(string key)
    {
        var results = new List<SearchResult>();
        foreach (var casefile in _casefiles)
        {
            if (results.Count == 10)
            {
                break;
            }

            key = key.ToUpper();
            if (casefile.CasefileNumber.Contains(key) ||
                (casefile.CourtFileNumber is not null && casefile.CourtFileNumber!.Contains(key)))
            {
                results.Add(new(SearchTrie.Find(casefile.CasefileNumber), key));
            }
        }

        results.Sort();

        return results;
    }

    private static SearchTrie BuildTrie(CourtList courtList) 
    {
        SearchTrie trie = new();
        var casefiles = courtList.GetCasefiles();
        // go through each item, create trie node and add reference at leaf of the key.
        foreach (var cf in casefiles)
        {
            trie.Insert(cf.CasefileNumber.ToUpper(), cf);
            if (cf.CourtFileNumber is not null)
            {
                trie.Insert(cf.CourtFileNumber.ToUpper(), cf);
            }
        }
        return trie;
    }
}