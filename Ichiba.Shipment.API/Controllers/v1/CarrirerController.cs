using Ichiba.Shipment.Application.Carriers.Commands;
using Ichiba.Shipment.Application.Carriers.Queries;
using Ichiba.Shipment.Application.Shipments.Commands;
using Ichiba.Shipment.Application.Shipments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ichiba.Shipment.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrirerController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CarrirerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateShipment([FromBody]CreateCarrierCommand command)
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


        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] GetListCarrierQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetail([FromQuery] GetDetailCarrierQuery query)
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
}
