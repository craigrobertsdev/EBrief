using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Services;
public class CourtListParser
{
    bool _endOfCourtRoom = false;
    bool _endOfLandscapeList = false;
    int pos = 0;

    public CourtListModel ParseIndividual(string filePath, int courtRoom)
    {
        try
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart?.Document.Body;

            var headerText = document.MainDocumentPart.HeaderParts.First().Header.InnerText;
            var hearingEntries = body.ChildElements[0].ChildElements[5].ChildElements[1].ChildElements[1];
            var date = DateTime.Parse(string.Join(" ", headerText.Split(" ")[^3..]));
            var court = body.ChildElements[0].ChildElements[3].InnerText.Split(',')[0];

            CourtListModel courtList = ParseCourtListIndividual(courtRoom, hearingEntries, date, court);

            return courtList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

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
                CourtListModel courtList = ParseCourtListLandscape(hearingEntries, date, court);
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

    private CourtListModel ParseCourtListIndividual(int courtRoom, OpenXmlElement hearingEntries, DateTime date, string court)
    {
        var courtList = new CourtListModel
        {
            CourtDate = date,
            CourtCode = CourtCodeMappings[court],
            CourtRoom = courtRoom
        };

        pos = 4;
        while (!_endOfCourtRoom)
        {
            ParseNext(courtList, hearingEntries.ChildElements);
        }

        return courtList;
    }

    private CourtListModel ParseCourtListLandscape(OpenXmlElement hearingEntries, DateTime date, string court)
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
        else if (row.InnerText.StartsWith("COURT"))
        {
            _endOfCourtRoom = IsNewCourtRoom(row, courtList.CourtRoom);
            if (!_endOfCourtRoom)
            {
                pos += 2; // skips the column header line immediately following the court session header
            }
            return;
        }

        var elements = row.ChildElements;
        var caseFile = new CaseFileModel();

        //elements[3] = "3. MCCRM-XX-XXXXXX(H1234567B)First Appearances"
        if (elements[3].InnerText == string.Empty) // this happens for co-accused
        {
            var lastCaseFile = courtList.CaseFiles.Last();
            caseFile.CourtFileNumber = lastCaseFile.CourtFileNumber;
            caseFile.CaseFileNumber = lastCaseFile.CaseFileNumber;
            caseFile.ListNumber = lastCaseFile.ListNumber;
            caseFile.ListingType = lastCaseFile.ListingType;
        }
        else
        {
            var ids = SplitCourtIdentifiers(elements[3].InnerText);
            caseFile.CaseFileNumber = ids.PoliceFileNumber;
            caseFile.CourtFileNumber = ids.CourtFileNumber;
            caseFile.ListNumber = ids.ListNo;
            caseFile.ListingType = ids.ListingType;
        }

        //elements[4] = "SURNAME, FirstName"
        var defendantName = elements[4].InnerText.Split(", ");
        caseFile.Defendant = new DefendantModel
        {
            FirstName = defendantName[1].Trim(),
            LastName = defendantName[0].Trim()
        };

        // elements[6] && elements[7]
        if (elements[6].InnerText != string.Empty)
        {
            var counsel = new DefenceCounsel
            {
                Name = elements[6].InnerText,
                Number = elements[7].InnerText
            };
            caseFile.Counsel = counsel;
        }

        //elements[5] = "Prescribed Road, Truck/Bus Exceed Speed Limit >= 10Km/Hr (Camera Offence)"
        caseFile.OffenceDetails = elements[5].InnerText.Split("/ ").Select(o => o.Trim()).ToArray();

        //elements[8] = "Hearing"
        caseFile.HearingType = elements[8].InnerText;

        //elements[9] = "Gaol/Bail"
        caseFile.DefendantAppearanceMethod = elements[9].InnerText;

        courtList.CaseFiles.Add(caseFile);
        pos++;
    }

    private string[] SplitOffenceDetails(string offenceText)
    {
        var offences = offenceText.Split("/ ");
        return offences;
    }

    private CourtIdentifiers SplitCourtIdentifiers(string text)
    {
        var courtFileNumber = string.Empty;
        var policeFileNumber = string.Empty;
        var listingType = string.Empty;

        // "3. MCCRM-24-016760(H3617790B)First Appearances"
        // "3. MCCRM-24-016760First Appearances"
        int idTextPos = text.IndexOf('.');
        int listNo = Int32.Parse(text[0..idTextPos]);
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

        listingType = text[idTextPos..];

        return new CourtIdentifiers(listNo, courtFileNumber, policeFileNumber, listingType);
    }

    private bool IsNewCourtRoom(OpenXmlElement row, int courtRoom)
    {
        var text = row.ChildElements[1].InnerText.Split(' ')[1];
        var room = Int32.Parse(text);
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

/* 
* Court session header: these are found as one of the row types in the list
* hearingEntries.ChildElements[2].ChildElements[1] = "COURT 2"
* hearingEntries.ChildElements[2].ChildElements[2] = "Before MAGISTRATE MCLEOD"
* 
* Observations about hearing entries:
* A hearing with a solicitor but without a gaol/bail type:
*      10:00 AM3.   MCCRM-24-016760(H3617790B)First AppearancesHARDING, Christopher Prescribed Road, Truck/Bus Exceed Speed Limit >= 10Km/Hr (Camera Offence)Jessica Kurtzer(0488 345 065)Hearing 
* hearingElements = hearingEntries.ChildElements[2].ChildElements[10].ChildElements
* hearingElements[1] = "10:00AM"
* hearingElements[2] = ""
* hearingElements[3] = "3. MCCRM-24-016760(H3617790B)First Appearances"
* hearingElements[4] = "HARDING, Christopher"
* hearingElements[5] = "Prescribed Road, Truck/Bus Exceed Speed Limit >= 10Km/Hr (Camera Offence)"
* hearingElements[6] = "Jessica Kurtzer"
* hearingElements[7] = "(0488 345 065)"
* hearingElements[8] = "Hearing"
* hearingElements[9] = ""
* hearingElements[10] = ""
* 
* 
* A hearing with a solicitor and a gaol/bail type:
*      11:30 AM26.   MCCRM-24-027522(CO2400027635)First AppearancesTAVINO, Costanzo Dino Basic Offence: Dishonestly Take Property Without Consent Evangelos Dimou(8111 5555)HearingBail
* hearingElements = hearingEntries.ChildElements[2].ChildElements[58].ChildElements
* hearingElements[1] = "11:30AM"
* hearingElements[2] = ""
* hearingElements[3] = "26. MCCRM-24-027522(CO2400027635)First Appearances"
* hearingElements[4] = "TAVINO, Costanzo Dino "
* hearingElements[5] = "Basic Offence: Dishonestly Take Property Without Consent "
* hearingElements[6] = "Evangelos Dimou"
* hearingElements[7] = "(8111 5555)"
* hearingElements[8] = "Hearing"
* hearingElements[9] = "Bail"
* hearingElements[10] = ""
* 
* Hearing entries can exist without case file numbers if they are breaches of bond or similar. The format is:
* hearingElements[3] = "50. MCCRM-24-028483General List"
* will need to find the last number and split it there.
* 
* Multiple offences are created like:
* hearingElements[5] = "Unlawfully On Premises(3)/ Basic Offence: Dishonestly Take property Without consent/ Damage Building Or Motor Vehicle (Not Graffiti Or Unkown)"
* these will be able to be split by the "/ ". Will be useful informatino to have in the tooltip or perhaps in a listing information tab?
* 
* Multiple defendants:
* The usual hearing details exist for the first accused
* The next accused's entries look like:
* hearingElements[1] = ""
* hearingElements[2] = ""
* hearingElements[3] = ""
* hearingElements[4] = "COOKE, Mitchell "
* hearingElements[5] = " Commit Theft Using Force (Aggravated Offence) (2)/ Drive Or Use Motor Vehicle Without Consent (2)"
* hearingElements[6] = "Stacey Carter"
* hearingElements[7] = "(1300 707 054)"
* hearingElements[8] = "Committal Hearing - Answer Charge"
* hearingElements[9] = "Bail"
* hearingElements[10] = ""
*/

//public string[] HearingTypes = [
//    "First Appearances",
//    "General List",
//    "Committal",
//    "Part Heard",
//    "Trial",
//    "Callover",
//    "Pre Trial Conference",
//    "PTC Following Files",
//    "Family Violence",
//    "Police Interim Intervention Orders",
//    "Aboriginal Community Court",
//    "DCS",
//    "Tax"
//];

/* Header contains text like: Magistrates Court - Criminal JurisdictionThursday 01 August 2023
 * 
 * CourtHearings can be divided into:
 *  List number: 1
 *  CourtFileNumber: MCCRM-24-123456
 *  CaseFileNumber: CO2300012345
 *  Defendant: SURNAME, First, (Middle Names?)
 *  Offences: Basic Offence: Dishonestly Take Property Without Consent (probably don't care about this unless we have no case file number?)
 *  Counsel: FirstName, LastName (may be null)
 *  Hearing Type: Hearing/Committal Hearing - Answer Charge/Bail Hearing/Charge Determination Hearing/For Mention Only/Trial/Callover etc
 *  Gaol/Bail: null/Bail/Gaol
 *  
 *  
 *  Will need regex for:
 *  1. /Adelaide Magistrates Court, 260-280 Victoria Square,Adelaide 5000COURT/ - to get word COURT from end of first part of court list
 *  2. /ChargeGaolCOURT 14Before MAGISTRATE / - to extract COURT 14
 *  3. 
 */
