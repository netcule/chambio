using Chambio.Server.Entities;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chambio.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{Name}")]
    public async Task<ActionResult<List<Member>>> List(
        [FromRoute] ListMembersRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);

    [HttpGet("{Id:int}")]
    public async Task<ActionResult<Member>> Get(
        [FromRoute] GetMemberRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);
}
