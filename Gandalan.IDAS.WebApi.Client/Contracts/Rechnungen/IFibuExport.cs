using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Gandalan.IDAS.WebApi.Client.DTOs.Rechnung;

namespace Gandalan.IDAS.WebApi.Client.Contracts.Rechnungen;

public interface IFibuExport
{
    Task ExportFibu(IList<IFibuRecord> data);
    IFibuRecord CreateRecord();
}

public enum FibuRecordBelegArt
{
    Rechnung = 0,
    Sammelrechnung = 1,
    Gutschrift = 2,
}

public interface IFibuRecord
{
    Guid OrginialBelegGuid { get; set; }
    long Nummer { get; set; }
    FibuRecordBelegArt OrginialBelegArt { get; set; }
    IList<PropertyInfo> GetExportProperties();
    Task FromSammelrechnung(SammelrechnungListItemDTO sammelrechnung);
    Task FromRechnung(BelegeInfoDTO rechnung);
}