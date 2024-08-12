using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services.Search;
public class SearchService
{
    public SearchTrie SearchTrie { get; private set; }

    public SearchService(CourtList courtList)
    {
        SearchTrie = BuildTrie(courtList);
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
            trie.Insert(cf.CaseFileNumber, new NodeValue(cf));
            if (cf.CourtFileNumber is not null)
            {
                trie.Insert(cf.CourtFileNumber, new NodeValue(cf));
            }
        }
        return trie;
    }
}