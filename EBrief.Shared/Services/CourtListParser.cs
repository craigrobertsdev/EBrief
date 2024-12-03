using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Services;
public class CourtListParser
{
    bool _endOfCourtRoom = false;
    bool _endOfLandscapeList = false;
    int pos = 0;

    /*********DELETE THIS IN PROD*********/
    record ParsedDefendant(string First, string Last, Guid Id);
    List<ParsedDefendant> defendantNames = [];
    Guid currentId = Guid.NewGuid();
    /*************************************/

    public List<CourtListModel> ParseLandscapeList(string filePath)
    {
        try
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart!.Document.Body;

            var headerText = document.MainDocumentPart!.HeaderParts.First().Header.InnerText;
            var hearingEntries = body!.ChildElements[0].ChildElements[5].ChildElements[1].ChildElements[1];
            var date = DateTime.Parse(string.Join(" ", headerText.Split(" ")[^3..]));
            var court = body.ChildElements[0].ChildElements[3].InnerText.Split(',')[0];

            List<CourtListModel> lists = [];
            pos = 2;
            while (!_endOfLandscapeList)
            {
                _endOfCourtRoom = false;
                CourtListModel courtList = ParseCourtList(hearingEntries, date, court);
                lists.Add(courtList);
                _endOfLandscapeList = IsEndOfLandscapeList(hearingEntries);
            }

            return lists;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private bool IsEndOfLandscapeList(OpenXmlElement hearingEntries)
    {
        return pos >= hearingEntries.ChildElements.Count;
    }

    private CourtListModel ParseCourtList(OpenXmlElement hearingEntries, DateTime date, string court)
    {
        var courtRoom = Int32.Parse(hearingEntries.ChildElements[pos].ChildElements[1].InnerText.Split(' ')[1]);
        pos += 2;

        var courtList = new CourtListModel
        {
            CourtDate = date,
            CourtCode = CourtCodeMappings[court],
            CourtRoom = courtRoom
        };

        while (!_endOfCourtRoom && pos < hearingEntries.ChildElements.Count)
        {
            ParseNext(courtList, hearingEntries.ChildElements);
        }

        return courtList;
    }

    private void ParseNext(CourtListModel courtList, OpenXmlElementList childElements)
    {
        var row = childElements[pos];

        if (row.InnerText == string.Empty)
        {
            pos++;
            return;
        }
        
        if (row.InnerText.StartsWith("COURT"))
        {
            _endOfCourtRoom = IsNewCourtRoom(row, courtList.CourtRoom);
            if (!_endOfCourtRoom)
            {
                pos += 2; // skips the column header line immediately following the court session header
            }
            return;
        }

        var elements = row.ChildElements;
        var casefile = new CasefileModel();

        //elements[3] = "3. MCCRM-XX-XXXXXX(H1234567B)First Appearances"
        if (elements[3].InnerText == string.Empty) // this happens for co-accused
        {
            var lastCasefile = courtList.Casefiles.Last();
            casefile.CourtFileNumber = lastCasefile.CourtFileNumber;
            casefile.CasefileNumber = lastCasefile.CasefileNumber;
            casefile.ListNumber = lastCasefile.ListNumber;
            casefile.ListingType = lastCasefile.ListingType;
        }
        else
        {
            var ids = SplitCourtIdentifiers(elements[3].InnerText);
            casefile.CasefileNumber = ids.PoliceFileNumber;
            casefile.CourtFileNumber = ids.CourtFileNumber;
            casefile.ListNumber = ids.ListNo;
            casefile.ListingType = ids.ListingType;
        }

        //elements[4] = "SURNAME, FirstName"
        var defendantName = elements[4].InnerText.Split(", ");
        casefile.Defendant = new DefendantModel
        {
            FirstName = defendantName[1].Trim(),
            LastName = defendantName[0].Trim()
        };

        /*********DELETE THIS IN PROD*********/
        var defendant = defendantNames.LastOrDefault();
        if (defendant is not null && defendant.First == casefile.Defendant.FirstName && defendant.Last == casefile.Defendant.LastName)
        {
            casefile.Defendant.Id = defendant.Id;
        }
        else
        {
            casefile.Defendant.Id = currentId;
            defendantNames.Add(new(casefile.Defendant.FirstName, casefile.Defendant.LastName, currentId));
            currentId = Guid.NewGuid();
        }
        /*************************************/


        //elements[5] = "Prescribed Road, Truck/Bus Exceed Speed Limit >= 10Km/Hr (Camera Offence)"
        casefile.OffenceDetails = elements[5].InnerText.Split("/ ").Select(o => o.Trim()).ToArray();

        // elements[6] && elements[7] 
        if (elements[6].InnerText != string.Empty)
        {
            var counsel = new DefenceCounsel
            {
                Name = elements[6].InnerText,
                Number = elements[7].InnerText
            };
            casefile.Counsel = counsel;
        }

        //elements[8] = "Hearing"
        casefile.HearingType = elements[8].InnerText;

        //elements[9] = "Gaol/Bail"
        casefile.DefendantAppearanceMethod = elements[9].InnerText;

        courtList.Casefiles.Add(casefile);
        pos++;
    }

    private CourtIdentifiers SplitCourtIdentifiers(string text)
    {
        var courtFileNumber = string.Empty;
        var policeFileNumber = string.Empty;

        // "3. MCCRM-24-016760(H3617790B)First Appearances"
        // "3. MCCRM-24-016760First Appearances"
        int idTextPos = text.IndexOf('.');
        int listNo = int.Parse(text[0..idTextPos]);
        idTextPos++; // skip the '.' after the list number
        while (text[idTextPos] == ' ')
        {
            idTextPos++;
        }

        var polFilePos = text.IndexOf('(');
        if (polFilePos > 0) // MCCRM-24-016760(H3617790B)First Appearances"
        {
            courtFileNumber = text[idTextPos..polFilePos];
            idTextPos = polFilePos + 1;

            var rightParenPos = text.IndexOf(')');
            policeFileNumber = text[idTextPos..rightParenPos];
            idTextPos = rightParenPos + 1;
        }
        else // MCCRM-24-016760First Appearances"
        {
            for (int i = idTextPos + 10; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    continue;
                }
                else
                {
                    courtFileNumber = text[idTextPos..(i - 1)];
                    idTextPos = i;
                    break;
                }
            }
        }

        var listingType = text[idTextPos..];

        return new CourtIdentifiers(listNo, courtFileNumber, policeFileNumber, listingType);
    }

    private bool IsNewCourtRoom(OpenXmlElement row, int courtRoom)
    {
        var text = row.ChildElements[1].InnerText.Split(' ')[1];
        var room = int.Parse(text);
        return room != courtRoom;
    }

    private static readonly Dictionary<string, CourtCode> CourtCodeMappings = new()
    {
        { "Adelaide Magistrates Court", CourtCode.AMC },
        { "Elizabeth Magistrates Court", CourtCode.EMC },
        { "Port Adelaide Magistrates Court", CourtCode.PAMC },
        { "Christies Beach Magistrates Court", CourtCode.CBMC },
    };

    record CourtIdentifiers(int ListNo, string CourtFileNumber, string PoliceFileNumber, string ListingType);
}
