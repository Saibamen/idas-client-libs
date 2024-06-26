﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Gandalan.IDAS.WebApi.Data;

namespace Gandalan.IDAS.Client.Contracts.Contracts.AV;

public interface IBearbeitungsKuerzel
{
    Task<IList<BearbeitungsKuerzelDTO>> GetListe();
}