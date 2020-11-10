using Microsoft.AspNetCore.Mvc;

//Controller we can inherit from so that we don't have to specify attributes for each of our new controllers

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BaseApiController : ControllerBase
    {
        
    }
}