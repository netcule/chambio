using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class GetCountryHandler : IRequestHandler<GetCountryRequest, Country>
{
    readonly ChambioContext _context;

    public GetCountryHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<Country> Handle(GetCountryRequest request,
        CancellationToken cancellationToken) => await _context.Countries
            .AsNoTracking()
            .Include(c => c.Chambers!)
            .Include(c => c.Parties!)
            .ThenInclude(c => c.Ideologies!)
            .Include(c => c.Members!)
            .FirstAsync(c => c.Id == request.Id, cancellationToken);
}
