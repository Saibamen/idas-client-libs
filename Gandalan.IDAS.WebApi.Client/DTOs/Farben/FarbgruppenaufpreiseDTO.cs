using System;
using System.Collections.Generic;

namespace Gandalan.IDAS.WebApi.Data.DTOs.Farben;

public class FarbgruppenaufpreiseDTO
{
    public Guid FarbgruppenaufpreiseGuid { get; set; }

    public bool NeherModellAktiv { get; set; }
    public bool EigenesModellAktiv { get; set; }
    public bool IstAdminDTO { get; set; }
    public List<FarbgruppeSettingsDTO> FarbgruppenSettings { get; set; } = [];

    public long Version { get; set; }
    public DateTime ChangedDate { get; set; }
}

public class FarbgruppeSettingsDTO
{
    public Guid FarbgruppenGuid { get; set; }
    public bool GruppeAktiv { get; set; }
    public bool PreisAufAnfrage { get; set; }
    public decimal AufpreisElement { get; set; }
    public decimal AufpreisFarbe { get; set; }
    public decimal ProzentAufpreisElement { get; set; }
    public decimal AufpreisMaximal { get; set; }
    public bool AufpreisMaximalAktiv { get; set; }
}