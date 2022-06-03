using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class ListPartiesHandler
    : IRequestHandler<ListPartiesRequest, List<Party>>
{
    readonly ChambioContext _context;

    public ListPartiesHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<List<Party>> Handle(ListPartiesRequest request,
        CancellationToken cancellationToken) => await _context.Parties
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(request.Name.ToLower()))
            .ToListAsync(cancellationToken);
}
