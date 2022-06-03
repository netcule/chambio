using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class ListCountriesHandler
    : IRequestHandler<ListCountriesRequest, List<Country>>
{
    readonly ChambioContext _context;

    public ListCountriesHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<List<Country>> Handle(ListCountriesRequest request,
        CancellationToken cancellationToken) => await _context.Countries
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
