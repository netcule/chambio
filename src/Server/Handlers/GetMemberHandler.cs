using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class GetMemberHandler : IRequestHandler<GetMemberRequest, Member>
{
    readonly ChambioContext _context;

    public GetMemberHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<Member> Handle(GetMemberRequest request,
        CancellationToken cancellationToken) => await _context.Members
            .AsNoTracking()
            .Include(m => m.Party)
            .Include(m => m.Chamber)
            .Include(m => m.Country)
            .FirstAsync(m => m.Id == request.Id, cancellationToken);
}
