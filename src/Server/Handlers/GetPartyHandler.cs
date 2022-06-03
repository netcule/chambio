using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class GetPartyHandler : IRequestHandler<GetPartyRequest, Party>
{
    readonly ChambioContext _context;

    public GetPartyHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<Party> Handle(GetPartyRequest request,
        CancellationToken cancellationToken) => await _context.Parties
            .AsNoTracking()
            .Include(p => p.Ideologies!)
            .Include(p => p.Members!)
            .Include(p => p.Country!)
            .FirstAsync(p => p.Id == request.Id, cancellationToken);
}
