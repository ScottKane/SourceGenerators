namespace Server.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class ProductsController : ControllerBase
// {
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         var products = await Mediator.Send(new GetAllProductsQuery());
//         return Ok(products);
//     }
//     
//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetById(int id) => Ok(await Mediator.Send(new GetProductByIdQuery {Id = id}));
//     
//     [HttpPost]
//     public async Task<IActionResult> Post(AddEditProductCommand command) => Ok(await Mediator.Send(command));
//     
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> Delete(int id) => Ok(await Mediator.Send(new DeleteProductCommand {Id = id}));
// }