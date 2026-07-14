using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using tickethub.Dtos;
using tickethub.Dtos.Concert;
using tickethub.Dtos.Order;
using tickethub.Mappers;

namespace tickethub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertsController(
    AppDbContext context,
    ConcertMapper concertMapper,
    OrderMapper orderMapper) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateConcert([FromBody] CreateConcertRequest request)
    {
        var concert = concertMapper.ToEntity(request);

        context.Concerts.Add(concert);
        context.SaveChanges();

        var response = concertMapper.ToResponse(concert);
        
        return CreatedAtAction(nameof(CreateConcert), new { id = concert.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAllConcerts()
    {
        var concerts = context.Concerts
            .AsEnumerable()
            .Select(concertMapper.ToResponse)
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
        
        return Ok(concertMapper.ToResponse(concert));
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
        
        var hasExistingOrders = context.Orders
            .Any(o => o.ConcertId == id);

        if (hasExistingOrders)
        {
            return Conflict("Concert can't be deleted because it has orders processed already");
        }

        context.Concerts.Remove(concert);
        context.SaveChanges();
        
        return NoContent();
    }

    [HttpPost("{id:int}/orders")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CreateOrder(
        int id,
        [FromBody] CreateOrderRequest request)
    {
        var concert = context.Concerts
            .SingleOrDefault(c => c.Id == id);

        if (concert is null)
        {
            return NotFound();
        }

        if (concert.TicketsSold + request.Qty > concert.MaxCapacity)
        {
            return BadRequest("Order's quantity is above concert's maximum capacity");
        }
        concert.TicketsSold += request.Qty;
        
        var order = orderMapper.ToEntity(request);
        order.UnitPrice = concert.TicketPrice;
        order.ConcertId = concert.Id;

        try
        {
            context.Orders.Add(order);
            context.SaveChanges();
        } catch (DbUpdateConcurrencyException)
        {
            return Conflict("Failed to process order due to a concurrency conflict.");
        }
        
        var response = orderMapper.ToResponse(order);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}