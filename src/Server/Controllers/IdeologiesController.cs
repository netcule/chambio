using Chambio.Server.Entities;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chambio.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdeologiesController : ControllerBase
{
    readonly IMediator _mediator;

    public IdeologiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{Name}")]
    public async Task<ActionResult<List<Ideology>>> List(
        [FromRoute] ListIdeologiesRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);

    [HttpGet("{Id:int}")]
    public async Task<ActionResult<Ideology>> Get(
        [FromRoute] GetIdeologyRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);
}
