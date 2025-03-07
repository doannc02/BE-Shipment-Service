using Ichiba.Shipment.Application.Warehouses.Commands;
using Ichiba.Shipment.Application.Warehouses.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Ichiba.Shipment.Application.Warehouses.Queries.GetListWarehouseQuery;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ichiba.Shipment.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehouseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<WarehouseController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateWarehouseCommand value)
        {
            var result = await _mediator.Send(value);
            if (result.Status == false) return BadRequest("Errror");
            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] QueryGetListWarehouse value)
        {
            var result = await _mediator.Send(value);
            if (result.Status == false) return BadRequest("Errror");
            return Ok(result);
        }
    }
}
