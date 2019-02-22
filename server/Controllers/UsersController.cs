using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlayServices.DataModel;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        UserService _userService = new DataModel.UserService();
    }
}
