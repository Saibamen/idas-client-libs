﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gandalan.IDAS.WebApi.DTO
{
    public class AnpassungVorlageDTO
    {
        public Guid AnpassungVorlageGuid { get; set; }
        public string Art { get; set; }
        public string Name { get; set; }
        public string Beschreibung { get; set; }
        public string Content { get; set; }
        public long Version { get; set; }
        public DateTime ChangedDate { get; set; }
    }
}
