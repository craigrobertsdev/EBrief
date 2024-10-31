using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services.Search;

public class SearchService
{
    public SearchTrie SearchTrie { get; private set; }
    private readonly CaseFile[] _caseFiles;

    public SearchService(CourtList courtList)
    {
        SearchTrie = BuildTrie(courtList);
        var courtListCaseFiles = courtList.GetCaseFiles();
        var caseFiles = new CaseFile[courtListCaseFiles.Count];
        courtList.GetCaseFiles().CopyTo(caseFiles, 0);
        foreach (var cf in caseFiles)
        {
            cf.CaseFileNumber = cf.CaseFileNumber.ToUpper();
            cf.CourtFileNumber = cf.CourtFileNumber?.ToUpper();
        }

        _caseFiles = [..caseFiles];
    }

    public List<SearchResult> Find(string key)
    {
        var results = new List<SearchResult>();
        foreach (var caseFile in _caseFiles)
        {
            if (results.Count == 10)
            {
                break;
            }

            key = key.ToUpper();
            if (caseFile.CaseFileNumber.Contains(key) ||
                (caseFile.CourtFileNumber is not null && caseFile.CourtFileNumber!.Contains(key)))
            {
                results.Add(new(SearchTrie.Find(caseFile.CaseFileNumber), key));
            }
        }

        results.Sort();

        return results;
    }

    private static SearchTrie BuildTrie(CourtList courtList)
    {
        SearchTrie trie = new();
        var caseFiles = courtList.GetCaseFiles();
        var caseFileNumbers = caseFiles.Select(cf => cf.CaseFileNumber);
        var courtFileNumbers = caseFiles.Select(cf => cf.CourtFileNumber);
        // go through each item, create trie node and add reference at leaf of the key.
        foreach (var cf in caseFiles)
        {
            trie.Insert(cf.CaseFileNumber.ToUpper(), cf);
            if (cf.CourtFileNumber is not null)
            {
                trie.Insert(cf.CourtFileNumber.ToUpper(), cf);
            }
        }

        return trie;
    }
}