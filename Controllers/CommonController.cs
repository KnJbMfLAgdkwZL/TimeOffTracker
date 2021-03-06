using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TimeOffTracker.Model.DTO;
using TimeOffTracker.Model.Repositories;
using System.Collections.Generic;
using TimeOffTracker.Model.Enum;
using System.Linq;
using System.Security.Claims;
using TimeOffTracker.Model;

namespace TimeOffTracker.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class CommonController : ControllerBase
    {
        [ProducesResponseType(200, Type = typeof(List<EnumDto>))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<List<EnumDto>>> GetProjectRoleTypes(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var enumRepository = new EnumRepository();
            return Ok(enumRepository.GetAll<ProjectRoleTypes>());
        }

        [ProducesResponseType(200, Type = typeof(List<EnumDto>))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<List<EnumDto>>> GetRequestTypes(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var enumRepository = new EnumRepository();
            return Ok(enumRepository.GetAll<RequestTypes>());
        }

        [ProducesResponseType(200, Type = typeof(List<EnumDto>))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<List<EnumDto>>> GetStateDetails(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var enumRepository = new EnumRepository();
            return Ok(enumRepository.GetAll<StateDetails>());
        }

        /// <summary>
        /// GET: /Common/GetUserRoles
        /// Header
        /// {
        ///     Authorization: Bearer {TOKEN}
        /// }
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// [ EnumDto1, EnumDto2, EnumDto3]
        /// </returns>
        [ProducesResponseType(200, Type = typeof(List<EnumDto>))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<List<EnumDto>>> GetUserRoles(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var enumRepository = new EnumRepository();
            return Ok(enumRepository.GetAll<UserRoles>());
        }

        [ProducesResponseType(200, Type = typeof(List<EnumDto>))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<string>> GetCurrentUser(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var userIdStr = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userId = int.Parse(userIdStr);

            var userRepository = new UserRepository();
            var user = await userRepository.SelectByIdAsync(userId, token);
            if (user == null)
            {
                return NoContent();
            }

            user.Password = "";
            var userDto = Converter.EntityToDto(user);
            return Ok(userDto);
        }
    }
}