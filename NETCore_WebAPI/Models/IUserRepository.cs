using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore_WebAPI.Models;

namespace NETCore_WebAPI.Models
{
    public interface IUserRepository
    {
        #region GET
        Task<ActionResult<IEnumerable<User>>> GetAllActive(string reqUser, string reqPassword);
        Task<ActionResult<IEnumerable<User>>> GetAll(string reqUser, string reqPassword);
        Task<ActionResult<IEnumerable<User>>> GetByAge(int age, string reqUser, string reqPassword);
        Task<ActionResult<User>> GetUser(string login, string reqUser, string reqPassword);
        #endregion

        #region Update
        Task<ActionResult<User>> Revoke(string login, string reqUser, string reqPassword);
        Task<ActionResult<User>> UpdateUser(User user, string reqUser, string reqPassword);
        #endregion

        Task<ActionResult<User>> Add(User user, string reqUser, string reqPassword);

        Task<ActionResult<User>> Delete(string login, string reqUser, string reqPassword);
    }
}
