using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TimeOffTracker.Model;
using Microsoft.AspNetCore.Authorization;
using TimeOffTracker.Model.DTO;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TimeOffTracker.Model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TimeOffTracker.Model.DTO;
using TimeOffTracker.Model.Repositories;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Matching;
using PagedList;
using TimeOffTracker.Model.Enum;

namespace TimeOffTracker.Controllers
{
    /// <summary>
    /// Бухгалтерия (Accounting): Роль пользователя Бухгалтерия - это обобщенная роль для нескольких людей.  
    ///     ◦ Первым подписывает заявку на любой отпуск
    ///     ◦ Получает нотификацию о полностью подписанной заявке 
    ///     ◦ Может в любой момент просмотреть статистику заявок
    /// Менеджер (Manager):
    ///     ◦ Подписывает заявку на отпуск
    ///     ◦ Может в любой момент
    ///     ◦ просмотреть статистику заявок
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Manager, Accounting")]
    public class ManagerController : ControllerBase
    {
        /// <summary>
        /// Одобрить заявку на отпуск
        /// GET: /Manager/AcceptRequest?id=10
        /// Header
        /// {
        ///     Authorization: Bearer {TOKEN}
        /// }
        /// </summary>
        /// <param name="id">ид запроса</param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<string>> AcceptRequest([FromQuery(Name = "id")] int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var userIdStr = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userId = int.Parse(userIdStr);

            var userSignatureRepository = new UserSignatureRepository();
            var userSignature = await userSignatureRepository.SelectOneAsync(userId, id, token);

            var requestRepository = new RequestRepository();
            var request = await requestRepository.SelectAsync(id, token);

            if (userSignature == null || request == null)
            {
                return NoContent();
            }

            if (userSignature.NInQueue != 0)
            {
                return BadRequest("Not your turn");
            }

            if (userSignature.Approved)
            {
                return BadRequest("Already approved");
            }

            if (request.StateDetailId != (int) StateDetails.New &&
                request.StateDetailId != (int) StateDetails.InProgress)
            {
                var enumRepository = new EnumRepository();
                var state = enumRepository.GetById<StateDetails>(request.StateDetailId);
                return BadRequest($"Request.StateDetail: {state.Type}");
            }

            await userSignatureRepository.ConfirmSignatureAsync(userSignature, token);
            var userSignatures =
                await userSignatureRepository.SelectAllNotApprovedByIdAsync(userSignature.RequestId, token);

            if (userSignatures.Count <= 0)
            {
                var req = await requestRepository.SelectNotIncludeAsync(request.Id, token);
                req.StateDetailId = (int) StateDetails.Approved;
                await requestRepository.UpdateAsync(req, token);

                //Send email to Accounting
            }

            if (userSignatures.Count > 0)
            {
                //Send email next manager
            }

            return Ok("Ok");
        }

        /// <summary>
        /// Отклонить заявку на отпуск
        /// GET: /Manager/RejectRequest?id=19&reason=to long
        /// Header
        /// {
        ///     Authorization: Bearer {TOKEN}
        /// }
        /// </summary>
        /// <param name="id">ид запроса</param>
        /// <param name="reason">причина отказа</param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<string>> RejectRequest(
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "reason")] string reason,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var userIdStr = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userId = int.Parse(userIdStr);

            var userSignatureRepository = new UserSignatureRepository();
            var userSignature = await userSignatureRepository.SelectOneAsync(userId, id, token);

            var requestRepository = new RequestRepository();
            var request = await requestRepository.SelectAsync(id, token);

            if (userSignature == null || request == null)
            {
                return NoContent();
            }

            if (userSignature.NInQueue != 0)
            {
                return BadRequest("Not your turn");
            }

            if (userSignature.Approved)
            {
                return BadRequest("Already approved");
            }

            if (request.StateDetailId != (int) StateDetails.New &&
                request.StateDetailId != (int) StateDetails.InProgress)
            {
                var enumRepository = new EnumRepository();
                var state = enumRepository.GetById<StateDetails>(request.StateDetailId);
                return BadRequest($"Request.StateDetail: {state.Type}");
            }

            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest("Reason not set");
            }

            request.StateDetailId = (int) StateDetails.Rejected;
            await requestRepository.UpdateAsync(request, token);

            userSignature.Reason = reason;
            await userSignatureRepository.UpdateAsync(userSignature, token);

            //Send email to Accountant about Rejecting

            return Ok("Ok");
        }

        /// <summary>
        /// Получить список запросов, в которых был указан текущий менеджер
        /// POST: /Manager/GetRequests?page=3&pageSize=10
        /// Header
        /// {
        ///     Authorization: Bearer {TOKEN}
        /// }
        /// </summary>
        /// <param name="filter">
        /// Получить все заявки
        /// Body
        /// {
        ///     "RequestTypeId": 0,
        ///     "StateDetailId": 0,
        ///     "Reason": ""
        /// }
        /// </param>
        /// <param name="page">Текущая страница</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpPost]
        public async Task<ActionResult<string>> GetRequests([FromBody] RequestDto filter,
            [FromQuery(Name = "page")] int page,
            [FromQuery(Name = "pageSize")] int pageSize, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var userIdStr = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userId = int.Parse(userIdStr);
            filter.UserId = userId;

            var userSignatureRepository = new UserSignatureRepository();
            var requests = await userSignatureRepository.SelectAllRequestsAsync(userId, filter, token);

            var totalPages = (int) Math.Ceiling((double) requests.Count / pageSize);
            var requestsDto = requests.ToPagedList(page, pageSize).Select(Converter.EntityToDto);
            var result = new
            {
                page = page,
                pageSize = pageSize,
                totalPages = totalPages,
                requests = requestsDto,
            };

            return Ok(result);
        }

        /// <summary>
        /// Получить детальную информацию о заявке, в которой был указан текущий менеджер
        /// GET: /Manager/GetRequestDetails?id=10
        /// Header
        /// {
        ///     Authorization: Bearer {TOKEN}
        /// }
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<ActionResult<string>> GetRequestDetails([FromQuery(Name = "id")] int id,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var userIdStr = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userId = int.Parse(userIdStr);

            var userSignatureRepository = new UserSignatureRepository();
            var request = await userSignatureRepository.SelectRequestAsync(id, userId, token);
            if (request == null)
            {
                return NoContent();
            }

            return Ok(new
            {
                Request = Converter.EntityToDto(request),
                UserSignatures = request.UserSignatures is {Count: > 0}
                    ? request.UserSignatures.Select(Converter.EntityToDto).ToList()
                    : new List<UserSignatureDto>()
            });
        }
    }
}