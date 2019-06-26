﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gandalan.IDAS.WebApi.Data.DTOs.PositionsDaten
{
    public class PositionsDatenDTO
    {
        public string VariantenName { get; set; }
        public int PositionsNummer { get; set; }
        public int AlternativPositionZuNummer { get; set; }
        public string Besonderheiten { get; set; }
        public string Einbauort { get; set; }
        public decimal Menge { get; set; }
        public string MengenEinheit { get; set; }
        public string BelegKommission { get; set; }
        public string PositionsKommission { get; set; }
        public string Text { get; set; }
        public string AngebotsText { get; set; }
        public DateTime ErfassungsDatum { get; set; }
    }
}