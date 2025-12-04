using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Repositorio
{
    public class LaudoEvidenciaRepository : Repository<LaudoEvidencia>, ILaudoEvidenciaRepository
    {
        public LaudoEvidenciaRepository(SingleOneDbContext context) : base(context)
        {
        }
    }
}
