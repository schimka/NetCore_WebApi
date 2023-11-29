using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NETCore_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NETCore_WebAPI.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {

        public UsersController(IUserRepository users)
        {
            Users = users;
        }
        public IUserRepository Users { get; set; }


        /// <summary>
        /// запрос всех активных пользователй (admin)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get([FromHeader(Name = "Authorization")] string auth)
        {
            try
            {
                return await Users.GetAllActive(reqUser(), reqPassword());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Login", ex.Message);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// запрос всех  пользователй (admin) 
        /// </summary>
        [HttpGet("get-with-revoked/")]
        public async Task<ActionResult<IEnumerable<User>>>  GetAll()
        {
            try
            {
                return await Users.GetAll(reqUser(), reqPassword());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Login", ex.Message);                
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// запрос пользователя по логину (только current и админ)
        /// </summary>
        [HttpGet("{login}")]
        public async Task<ActionResult<User>> Get([FromHeader(Name = "Authorization")] string auth, string login)
        {
            try
            {
                return await Users.GetUser(login, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Login", ex.Message); }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// запрос пользователей старше x
        /// </summary>
        [HttpGet("get-by-age/{age:int}")]
        public async Task<ActionResult<IEnumerable<User>>> GetByAge(int age)
        {
            try
            {
                return await Users.GetByAge(age, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// добавление пользователя (админ)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> Post(User user)
        {
            try
            {
                return await Users.Add(user, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// отключение/восстановление пользователя по логину
        /// </summary>
        [HttpPut("{login}")]
        public async Task<ActionResult<User>> Put(string login)
        {
            try
            {
                return await Users.Revoke(login, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// изменение пользоватея (админ, currentuser)
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<User>> Put(User user)
        {
            try
            {
                return await Users.UpdateUser(user, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// удалить пользователя по логину
        /// </summary>
        [HttpDelete("{login}")]
        public async Task<ActionResult<User>> Delete(string login)
        {
            try
            {                
                return await Users.Delete(login, reqUser(), reqPassword());
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);

        }


       
        private string reqUser()
        {
            return HttpContext.Request.Headers["Authorization"].ToString().Substring(0, HttpContext.Request.Headers["Authorization"].ToString().IndexOf(" "));
        }
        private string reqPassword()
        {
            return HttpContext.Request.Headers["Authorization"].ToString().Substring(HttpContext.Request.Headers["Authorization"].ToString().IndexOf(" ")+1);
        }
    }
}
