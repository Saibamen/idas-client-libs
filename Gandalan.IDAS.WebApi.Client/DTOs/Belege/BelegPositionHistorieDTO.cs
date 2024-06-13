﻿using System;

namespace Gandalan.IDAS.WebApi.DTO;

public class BelegPositionHistorieDTO
{
    public Guid BelegPositionHistorieGuid { get; set; }
    public virtual string Text { get; set; }
    public virtual DateTime Zeitstempel { get; set; }
    public virtual string Benutzer { get; set; }
}