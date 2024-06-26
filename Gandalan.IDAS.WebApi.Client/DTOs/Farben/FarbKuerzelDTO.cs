using System;

namespace Gandalan.IDAS.WebApi.DTO;

public class FarbKuerzelDTO
{
    public Guid FarbKuerzelGuid { get; set; }
    public string Kuerzel { get; set; }
    public string Beschreibung { get; set; }
    public string FarbBezeichnung { get; set; }
    public bool IstTrendfarbe { get; set; }
    public Guid FarbItemGuid { get; set; }
    public Guid OberflaecheGuid { get; set; }
    public DateTime? GueltigAb { get; set; }
    public DateTime? GueltigBis { get; set; }
    public long Version { get; set; }
    public DateTime ChangedDate { get; set; }
}