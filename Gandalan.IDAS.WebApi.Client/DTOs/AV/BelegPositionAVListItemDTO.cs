﻿using System;

namespace Gandalan.IDAS.WebApi.DTO;

public class BelegPositionAVListItemDTO
{
    public Guid BelegPositionAVGuid { get; set; }

    public Guid? BelegPositionGuid { get; set; }

    public string PCode { get; set; }

    public DateTime ChangedDate { get; set; }
}