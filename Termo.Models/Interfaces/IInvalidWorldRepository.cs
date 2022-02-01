using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface IInvalidWorldRepository
    {
        List<Tuple<string, int>> GetMostInvalidWorlds();
        Task Add(InvalidWorldEntity invalidWorldEntity);
    }
}
