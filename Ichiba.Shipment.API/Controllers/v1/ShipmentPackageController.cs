using Ichiba.Shipment.Application.Packages;
using Ichiba.Shipment.Application.Packages.Commands;
using Ichiba.Shipment.Application.ShipmentPackages.Commands;
using Ichiba.Shipment.Application.ShipmentPackages.Queries;
using Ichiba.Shipment.Application.Shipments.Commands;
using Ichiba.Shipment.Application.Shipments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ichiba.Shipment.API.Controllers.v1;

[Route("api/[controller]")]
[ApiController]
public class ShipmentPackageController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentPackageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList([FromQuery] GetListShipmentQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetDetailShipmentPackageQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateShipmentPackageCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateShipmentPackageCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeletePackageCommand { Id = id });
        return result.Status ? Ok(result) : BadRequest(result);
    }
}
