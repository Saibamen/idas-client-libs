using System.Threading.Tasks;
using Gandalan.IDAS.WebApi.DTO;

namespace Gandalan.Client.Contracts.DataServices;

public interface IUpdateInfoService
{
    Task<UpdateInfoDTO> Get();
}