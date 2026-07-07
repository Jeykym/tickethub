using Microsoft.AspNetCore.Mvc;
using tickethub.Dtos;
using tickethub.Mappers;

namespace tickethub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertsController(
    AppDbContext context,
    ConcertMapper mapper) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateConcert([FromBody] CreateConcertRequest request)
    {
        var concert = mapper.ToEntity(request);

        context.Concerts.Add(concert);
        context.SaveChanges();

        var response = mapper.ToResponse(concert);
        
        return CreatedAtAction(nameof(CreateConcert), new { id = concert.Id }, response);
    }
}