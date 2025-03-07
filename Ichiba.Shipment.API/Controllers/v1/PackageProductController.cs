using Ichiba.Shipment.Application.PackageProducts.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ichiba.Shipment.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PackageProductController(IMediator mediator)
        {
            _mediator = mediator;
        }
        // GET: api/<PackageProductController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PackageProductController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PackageProductController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreatePackageProductCommand value)
        {
            var result = await _mediator.Send(value);
            if(result.Status == false)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // PUT api/<PackageProductController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<PackageProductController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
