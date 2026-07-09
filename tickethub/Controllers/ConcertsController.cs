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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAllConcerts()
    {
        var concerts = context.Concerts
            .AsEnumerable()
            .Select(mapper.ToResponse)
            .ToArray();
        
        return Ok(concerts);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetConcertById(int id)
    {
        var concert = context.Concerts
            .SingleOrDefault(c => c.Id == id);

        if (concert is null)
        {
            return NotFound();
        }
        
        return Ok(mapper.ToResponse(concert));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteConcert(int id)
    {
        var concert = context.Concerts
            .SingleOrDefault(c => c.Id == id);

        if (concert is null)
        {
            return NotFound();
        }

        context.Concerts.Remove(concert);
        context.SaveChanges();
        
        return NoContent();
    }
}