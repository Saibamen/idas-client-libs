﻿using System.Threading.Tasks;

namespace Gandalan.Client.Contracts.ProduktionsServices
{
    public interface IAVModul 
    {
        Task ShowAVPlanung();
        Task ShowAVDruck();
    }
}
