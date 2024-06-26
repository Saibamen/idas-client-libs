using System;
using System.Threading.Tasks;
using Gandalan.IDAS.WebApi.DTO;

namespace Gandalan.Client.Contracts.DataServices;

public interface IArtikelService
{
    Task<KatalogArtikelDTO[]> GetAllAsync();
    Task<WarenGruppeDTO[]> GetAllWarenGruppenAsync();
    Task<string> SaveArtikelAsync(KatalogArtikelDTO artikel);
    Task<KatalogArtikelDTO> LoadArtikelAsync(Guid guid);
    Task<KatalogArtikelDTO> LoadArtikelAsync(string artikelNummer);
}