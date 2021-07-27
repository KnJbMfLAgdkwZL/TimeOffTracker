using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using TimeOffTracker.Model;
using TimeOffTracker.Model.DTO;
using TimeOffTracker.Model.Enum;
using TimeOffTracker.Model.Repositories;
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
using Microsoft.EntityFrameworkCore.Query;
using PagedList;
using TimeOffTracker.Model.Enum;

namespace TimeOffTracker.CRUD
{
    public class Request
    {
        public async Task<int> CreateAsync(RequestDto requestDto, CancellationToken token)
        {
            requestDto.StateDetailId = StateDetails.New;
            //Находим бухгалтерию
            var userRepository = new UserRepository();
            var accounting = await userRepository.SelectOneAccounting(token);
            if (accounting == null)
            {
                throw new Exception("Accounting not found");
            }

            //Добавляем бухгалтерию в список подписчиков
            requestDto.UserSignatureDto.Add(new UserSignatureDto()
            {
                NInQueue = -1,
                UserId = accounting.Id
            });
            //Убираем менеджеров которые повторяются
            requestDto.UserSignatureDto = requestDto.UserSignatureDto
                .GroupBy(car => car.UserId)
                .Select(g => g.First())
                .ToList();
            //Сортируем по NInQueue
            requestDto.UserSignatureDto.Sort((x, y) => x.NInQueue.CompareTo(y.NInQueue));
            //Создаем заявку
            var requestRepository = new RequestRepository();
            var requestId = await requestRepository.InsertAsync(requestDto, token);
            var userSignatureRepository = new UserSignatureRepository();
            //Задаем NInQueue в правильном порядке и добавляем подписчиков в бд
            var nInQueue = 0;
            foreach (var us in requestDto.UserSignatureDto)
            {
                us.NInQueue = nInQueue;
                us.RequestId = requestId;
                us.Approved = false;
                us.Deleted = false;
                nInQueue++;
                await userSignatureRepository.InsertAsync(us, token);
            }

            return requestId;
        }

        public async Task<int> UpdateAsync(RequestDto requestDto, CancellationToken token)
        {
            var userSignatureRepository = new UserSignatureRepository();
            await userSignatureRepository.DeleteAllAsync(requestDto.Id, token);

            var userRepository = new UserRepository();

            var accounting = await userRepository.SelectOneAccounting(token);
            if (accounting == null)
            {
                throw new Exception("Accounting not found");
            }

            //Добавляем бухгалтерию в список подписчиков
            requestDto.UserSignatureDto.Add(new UserSignatureDto()
            {
                NInQueue = -1,
                UserId = accounting.Id
            });
            //Убираем менеджеров которые повторяются
            requestDto.UserSignatureDto = requestDto.UserSignatureDto
                .GroupBy(car => car.UserId)
                .Select(g => g.First())
                .ToList();
            //Сортируем по NInQueue
            requestDto.UserSignatureDto.Sort((x, y) => x.NInQueue.CompareTo(y.NInQueue));

            //Обновляем заявку
            var requestRepository = new RequestRepository();
            var requestId = await requestRepository.UpdateAsync(requestDto, token);

            //Удаляем старых подписчиков
            await userSignatureRepository.DeleteAllAsync(requestId, token);

            //Задаем NInQueue в правильном порядке и добавляем подписчиков в бд
            var nInQueue = 0;
            foreach (var us in requestDto.UserSignatureDto)
            {
                us.NInQueue = nInQueue;
                us.RequestId = requestId;
                us.Approved = false;
                us.Deleted = false;
                nInQueue++;
                await userSignatureRepository.InsertAsync(us, token);
            }

            return requestDto.Id;
        }

        public async Task DeleteOwner(int requestId, CancellationToken token)
        {
            var userSignatureRepository = new UserSignatureRepository();
            await userSignatureRepository.DeleteAllAsync(requestId, token);

            var requestRepository = new RequestRepository();
            await requestRepository.DeleteOwnerAsync(requestId, token);
        }


        public async Task<bool> CheckManagersAsync(List<UserSignatureDto> managers, CancellationToken token)
        {
            var userRepository = new UserRepository();
            foreach (var us in managers)
            {
                var user = await userRepository.SelectByIdAsync(us.UserId, token);
                if (user == null)
                {
                    return false;
                }

                if (user.RoleId != (int) UserRoles.Manager)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<string> ChekAsync(RequestDto requestDto, CancellationToken token)
        {
            //Проверка даты
            var time1 = requestDto.DateTimeFrom.Ticks - DateTime.Now.Ticks;
            var time2 = requestDto.DateTimeTo.Ticks - DateTime.Now.Ticks;
            var time3 = requestDto.DateTimeTo.Ticks - requestDto.DateTimeFrom.Ticks;
            if (time1 < 0 || time2 < 0)
            {
                return "The dates are in the past. Please change the dates";
            }

            if (time3 < 0)
            {
                return "Start date is greater than end date";
            }

            /*
             Нужно добавить проверку на пересечение дат
             
             Пользователь выбрал даты, которые пересекаются с уже существующей заявкой на отпуск (утвержденной или в процессе)
             Сообщение:”There is another request for these dates. Do you really want to create one more request?”
             (“Есть другая  заявка на эти даты. Вы хотите создать еще одну заявку?”)
             Yes (Да)  - заявка создается
             No (Нет) - заявка не создается
             */

            //Проверка причины отпуска на пустоту
            if (string.IsNullOrEmpty(requestDto.Reason))
            {
                return "Reason not set";
            }

            var enumRepository = new EnumRepository();

            //Проверка типа отпуска
            if (!enumRepository.Contains(requestDto.RequestTypeId))
            {
                return "Wrong RequestType";
            }

            //Типы отпуска который должны подписать менеджеры
            var managers = new List<RequestTypes>()
            {
                RequestTypes.PaidLeave,
                RequestTypes.AdministrativeUnpaidLeave,
                RequestTypes.StudyLeave
            };
            if (managers.Contains(requestDto.RequestTypeId))
            {
                //Менеджер не был указан
                if (requestDto.UserSignatureDto.Count <= 0)
                {
                    return "Manager not set";
                }

                //Не указан тип участия в проекте
                if (!enumRepository.Contains(requestDto.ProjectRoleTypeId))
                {
                    return "Wrong ProjectRoleType";
                }

                //Не указны обязанности на проекте
                if (string.IsNullOrEmpty(requestDto.ProjectRoleComment))
                {
                    return "ProjectRoleComment not set";
                }

                //Проверка менеджеров на наличие в базе
                var checkPass = await CheckManagersAsync(requestDto.UserSignatureDto, token);
                if (!checkPass)
                {
                    return "Wrong Manager set";
                }
            }

            //Типы отпуска который должна подписать только бухгалтерия
            var accountingOnly = new List<RequestTypes>()
            {
                RequestTypes.AdministrativeUnpaidLeave,
                RequestTypes.SocialLeave,
                RequestTypes.SickLeaveWithDocuments,
                RequestTypes.SickLeaveWithoutDocuments
            };
            if (accountingOnly.Contains(requestDto.RequestTypeId))
            {
            }

            return "Ok";
        }

        void FindAsync()
        {
        }

        void Read()
        {
        }

        void List()
        {
        }
    }
}