using Ichiba.Shipment.Application.Shipments.Commands;
using Ichiba.Shipment.Application.Shipments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ichiba.Shipment.API.Controllers.v1;

[Route("v1/api/[controller]")]
[ApiController]
public class ShipmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentCommand command)
    {
        if (command == null)
            return BadRequest("Invalid request data");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("create-multiple")]
    public async Task<IActionResult> CreateMultiShipment([FromBody] CreateMultiShipmentsCommand command)
    {
        if (command == null)
            return BadRequest("Invalid request data");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShipment([FromBody] UpdateShipmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("update-status/{id}")]
    public async Task<IActionResult> UpdateStatusShipment([FromBody] UpdateStatusShipmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList([FromQuery] GetListShipmentQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> GetDetailShipment(DeleteShipmentCommand query)
    {
        if (query == null)
            return BadRequest("Invalid request data");

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
