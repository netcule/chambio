using Chambio.Server.Entities;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chambio.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartiesController : ControllerBase
{
    readonly IMediator _mediator;

    public PartiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{Name}")]
    public async Task<ActionResult<List<Party>>> List(
        [FromRoute] ListPartiesRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);

    [HttpGet("{Id:int}")]
    public async Task<ActionResult<Party>> Get(
        [FromRoute] GetPartyRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);
}
