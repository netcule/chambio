using Chambio.Server.Entities;
using Chambio.Server.Persistence;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Handlers;

public class ListMembersHandler
    : IRequestHandler<ListMembersRequest, List<Member>>
{
    readonly ChambioContext _context;

    public ListMembersHandler(ChambioContext context)
    {
        _context = context;
    }

    public async Task<List<Member>> Handle(ListMembersRequest request,
        CancellationToken cancellationToken) => await _context.Members
            .AsNoTracking()
            .Where(m => m.Name.ToLower().Contains(request.Name.ToLower()))
            .ToListAsync(cancellationToken);
}
