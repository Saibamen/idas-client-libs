﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gandalan.IDAS.Client.Contracts
{
    public interface IModelMapper<T, U> 
    {
        void Convert(T source, U target);
        void Convert(U source, T target);
    }
}
