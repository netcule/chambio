using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class GetIdeologyHandler : IRequestHandler<GetIdeologyRequest, Ideology>
{
    readonly ChambioContext _context;

    public GetIdeologyHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<Ideology> Handle(GetIdeologyRequest request,
        CancellationToken cancellationToken) => await _context.Ideologies
            .AsNoTracking()
            .Include(i => i.Parties!)
            .FirstAsync(i => i.Id == request.Id, cancellationToken);
}
