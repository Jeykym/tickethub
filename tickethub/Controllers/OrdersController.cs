using Microsoft.AspNetCore.Mvc;
using tickethub.Mappers;

namespace tickethub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    AppDbContext context,
    OrderMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAllOrders()
    {
        var orders = context.Orders
            .AsEnumerable()
            .Select(mapper.ToResponse)
            .ToArray();
        
        return Ok(orders);
    }
}