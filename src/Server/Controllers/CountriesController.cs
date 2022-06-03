using Chambio.Server.Entities;
using Chambio.Server.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chambio.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    readonly IMediator _mediator;

    public CountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<Country>>> List(
        [FromRoute] ListCountriesRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);

    [HttpGet("{Id:int}")]
    public async Task<ActionResult<Country>> Get(
        [FromRoute] GetCountryRequest request,
        CancellationToken cancellationToken) => await _mediator
            .Send(request, cancellationToken);
}
