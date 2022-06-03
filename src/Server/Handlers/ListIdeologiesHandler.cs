using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class ListIdeologiesHandler
    : IRequestHandler<ListIdeologiesRequest, List<Ideology>>
{
    readonly ChambioContext _context;

    public ListIdeologiesHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<List<Ideology>> Handle(ListIdeologiesRequest request,
        CancellationToken cancellationToken) => await _context.Ideologies
            .AsNoTracking()
            .Where(i => i.Name.ToLower().Contains(request.Name.ToLower()))
            .ToListAsync(cancellationToken);
}
