using KnrmVaarRaport;
using Xunit;

namespace TestKnrmVaarRaport;
public class SplitCsvTest
{
    [Fact]
    public void HeadersTest()
    {
        string headers = "Bijlagen,Datum,Soort,Boot,Aanvang O&O (uur),Aanvang O&O (minuten),Afvaart (uren),Afvaart (minuten),Afgemeerd (uren),Afgemeerd (minuten),Duur,Korte omschrijving,Weersomstandigheden,Windkracht (Beaufort),Windrichting,Schipper,Opstapper 1,Opstapper 2,Opstapper 3,Opstapper 4,Opstapper 5,Brandstof (daadwerkelijk afgetankte hoeveelheid),Type Oefening,Actie ten behoeve van,Oproep (uren),Oproep (minuten),Opmerkingen,Andere hulpverleners,Aantal Opvarenden,Naam,Straat,Postcode,Woonplaats,Telefoon";
        var result = SplitCsv.Split(headers);
        Assert.Equal(34, result.Length);
    }

    [Fact]
    public void IssueWithEmptyBlock()
    {
        string line = "\"text\",,\"MoreText\"";
        var result = SplitCsv.Split(line);
        Assert.Equal(3, result.Length);
        Assert.Equal("text", result[0]);
        Assert.Equal(string.Empty, result[1]);
        Assert.Equal("MoreText", result[2]);
    }
}