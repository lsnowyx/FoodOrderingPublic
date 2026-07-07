using Application.Features.Location.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int? limit = null)
    {
        var result = await _mediator.Send(new SearchLocationsQuery
        {
            Query = query,
            Limit = limit
        });

        return Ok(result);
    }
}
