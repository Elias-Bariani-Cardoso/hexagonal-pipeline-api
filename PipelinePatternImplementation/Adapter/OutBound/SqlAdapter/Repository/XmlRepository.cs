using Adapter.OutBound.SqlAdapter.Context;
using Domain.Models.Xml;
using Domain.Ports.OutBound;
using Microsoft.EntityFrameworkCore;

namespace Adapter.OutBound.SqlAdapter.Repository
{
    public class XmlRepository : IXmlRepository
    {
        private readonly SqlContext _context;

        public XmlRepository(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _context = serviceProvider
                .GetRequiredService<SqlContext>();
        }

        public async Task<List<Xml>> ReadTop10(
            CancellationToken cancellationToken = default)
        {
            return await _context.Xmls
                .AsNoTracking()
                .Take(10)
                .ToListAsync(cancellationToken);
        }
    }
}