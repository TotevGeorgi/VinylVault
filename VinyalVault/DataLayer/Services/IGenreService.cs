using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Services
{
    public interface IGenreService
    {
        Task<List<string>> GetGenresAsync();
    }
}
