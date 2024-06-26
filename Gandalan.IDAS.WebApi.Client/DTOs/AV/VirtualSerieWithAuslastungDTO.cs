using System;
using System.Collections.Generic;

namespace Gandalan.IDAS.WebApi.DTO;

public class VirtualSerieWithAuslastungDTO
{
    /// <summary>
    /// Eindeutige ID der Serie
    /// </summary>
    public Guid SerieGuid { get; set; }

    /// <summary>
    /// Name der Serie (Langform)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Serienkürzel für Ausdrucke, Kurzanzeige usw.
    /// </summary>
    public string Kuerzel { get; set; }

    /// <summary>
    /// Beginn der Produktion (erster Produktionstag)
    /// </summary>
    public DateTime Start { get; set; } = new(2000, 1, 1);

    /// <summary>
    /// Ende der Produktion (letzter Produktionstag)
    /// </summary>
    public DateTime Ende { get; set; } = new(2099, 12, 31);

    public IList<SerieAuslastungDTO> Auslastungen { get; set; } = [];
}