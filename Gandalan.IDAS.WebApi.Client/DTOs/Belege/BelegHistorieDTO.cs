﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace Gandalan.IDAS.WebApi.DTO
{
    public class BelegHistorieDTO
    {
        public Guid BelegHistorieGuid { get; set; }
        public virtual string Text { get; set; }
        public virtual DateTime Zeitstempel { get; set; }
        public virtual string Benutzer { get; set; }
    }
}
