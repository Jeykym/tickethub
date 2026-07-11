using Riok.Mapperly.Abstractions;
using tickethub.Dtos;
using tickethub.Dtos.Concert;
using tickethub.Models;

namespace tickethub.Mappers;

[Mapper]
public partial class ConcertMapper
{
    [MapperIgnoreTarget("Id")]
    [MapperIgnoreTarget("TicketsSold")]
    public partial Concert ToEntity(CreateConcertRequest request);
    
    public partial ConcertResponse ToResponse(Concert concert);
}