using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services.Search;
public class SearchTrie
{
    public TrieNode Root { get; private set; } = new();

    public class TrieNode
    {
        public char Key;
        public Dictionary<char, TrieNode> Children = [];
        public CaseFile? Value;
        public bool HasValue => Value != null;

        public TrieNode() { }
        public TrieNode(char key) => Key = Char.ToUpper(key);
    }

    public void Insert(string key, CaseFile value)
    {
        TrieNode current = Root;
        for (int i = 0; i < key.Length; i++)
        {
            if (current.Children.TryGetValue(key[i], out var node))
            {
                current = node;
            }
            else
            {
                var child = new TrieNode(key[i]);
                current.Children.Add(key[i], child);
                current = child;
            }
        }

        current.Value = value;
    }

    /// <summary>
    /// Used in SearchService when the existence of the casefile is already known
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public CaseFile Find(string key)
    {
        return Search(key)!.Value!;
    }

    public List<CaseFile> GetSearchResults(string key)
    {
        List<CaseFile> results = [];

        TrieNode? current = Search(key);
        if (current is null) // key not found, return empty set
        {
            return results;
        }
        else if (current.HasValue) // key exists so return only that key
        {
            results.Add(current.Value!);
        }
        else // build list of child nodes that branch off the endpoint of key and return them
        {
            int maxResults = 10;
            results = FindWords(current, key, results, maxResults);
        }

        return results;
    }

    private TrieNode? Search(string key)
    {
        TrieNode current = Root;
        for (int i = 0; i < key.Length; i++)
        {
            if (current.Children.TryGetValue(key[i], out var node))
            {
                current = node;
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    /* while results.count < maxResults
     * search each of the child nodes to find word endings
     * for each child node, search its children for word endings
     * if current.IsWord then add key to results and continue to recurse.
     * do in order pre-order traversal. when adding word to result, check if result count == maxResults
     * if so, return.
     * Does this need a bool flag atMax, or will it have the maxCheck at the right point of the function?
     */
    private static List<CaseFile> FindWords(TrieNode node, string key, List<CaseFile> results, int maxResults)
    {
        if (node.HasValue)
        {
            results.Add(node.Value!);
            if (results.Count == maxResults)
            {
                return results;
            }
        }

        foreach (var child in node.Children.Values)
        {
            FindWords(child, key + child.Value, results, maxResults);
        }

        return results;
    }

    public void Delete()
    {
        // this needs to be implemented when the ability to delete case files from the court list is implemented
        throw new NotImplementedException();
    }
}
