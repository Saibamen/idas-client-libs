﻿using System;
using System.Threading.Tasks;

namespace Gandalan.Client.Contracts.UIServices
{
    public interface IVorgangDisplay
    {
        Task DisplayVorgang(Guid vorgangGuid);
    }
}
