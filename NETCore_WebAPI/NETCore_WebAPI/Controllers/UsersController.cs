using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NETCore_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NETCore_WebAPI.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        UsersContext db;
        public UsersController(UsersContext context)
        {
            db = context;
            if (!db.Users.Any())
            {
                db.Users.Add(new Models.User { Guid= new Guid(), Login="Admin", Password="password", Name="Alina", Gender=0, Birthday=new DateTime(1992,1,1), Admin=true, 
                    CreatedOn = DateTime.Now, CreatedBy="Admin", ModifiedOn = DateTime.Now, ModifiedBy="Admin"});
                db.SaveChanges();
            }
        }


        //запрос всех активных пользователй (admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            try
            {
                if (IsAdmin())
                {
                    return await db.Users.OrderBy(x => x.CreatedOn).Where(x => x.RevokedOn == null).ToListAsync();
                }
                else ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex){  ModelState.AddModelError("Guid", "Unexpected exception! "+ex.Message); }
            return BadRequest(ModelState); 
        }
        //запрос всех  пользователй (admin)        
        [HttpGet("get-with-revoked/")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                if (IsAdmin())
                {
                    return await db.Users.OrderBy(x => x.CreatedOn).ToListAsync();
                }
                else ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        //запрос пользователя по логину (только current и админ)
        [HttpGet("{login}")]
        public async Task<ActionResult<User>> Get(string login)
        {
            try
            {
                if (login == null)
                    ModelState.AddModelError("Login", "Не указан логин");
                if (IsAdmin() || IsCurrentUser(login))
                {
                    User user = await db.Users.FirstOrDefaultAsync(x => x.Login == login);
                    if (user == null)
                        ModelState.AddModelError("Login", "Польщователь не найден");
                    else return new ObjectResult(user);
                }
                else ModelState.AddModelError("Login", "Доступ запрещен");                
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        //запрос пользователей старше x
        [HttpGet("get-by-age/{age:int}")]
        public async Task<ActionResult<IEnumerable<User>>> GetByAge(int age)
        {
            try
            {
                if (IsAdmin())
                {
                    DateTime date = DateTime.Now.AddYears(-age);
                    return await db.Users.Where(x => x.Birthday < date).ToListAsync();
                }
                else ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }


        //добавление пользователя (админ)
        [HttpPost]
        public async Task<ActionResult<User>> Post(User user)
        {
            try
            {
                if (user == null || user.Login == null)
                    ModelState.AddModelError("Login", "Не указан логин");
                else if (!IsAdmin())
                    ModelState.AddModelError("Admin", "Создание новых пользователей доступно только администраторам");
                else if (db.Users.Any(x => x.Login == user.Login))
                    ModelState.AddModelError("Login", "Пользователь c таким логином уже существует");
                else
                {
                    user.CreatedBy = reqUser();
                    user.CreatedOn = DateTime.Now;
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    return Ok(user);
                }
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }

            return BadRequest(ModelState);
        }

        //отключение/восстановление пользователя по логину
        [HttpPut("{login}")]
        public async Task<ActionResult<User>> Put(string login)
        {
            try
            {
                if (login == null)
                    ModelState.AddModelError("Login", "Не указан логин");
                if (IsAdmin())
                {
                    User user = await db.Users.FirstOrDefaultAsync(x => x.Login == login);
                    if (user == null)
                        ModelState.AddModelError("Login", "Польщователь не найден");
                    else
                    {
                        user.ModifiedBy = reqUser();
                        user.ModifiedOn = DateTime.Now;
                        if (user.RevokedOn == null)
                        {
                            user.RevokedBy = reqUser();
                            user.RevokedOn = DateTime.Now;
                        }
                        else
                        {
                            user.RevokedBy = null;
                            user.RevokedOn = null;
                        }
                        db.Update(user); ;
                        await db.SaveChangesAsync();
                        return Ok(user);
                    }

                }
                else ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }



        //изменение пользоватея (админ, current)
        [HttpPut]
        public async Task<ActionResult<User>> Put(User user)
        {
            try
            {
                if (user == null || user.Guid == null)
                    ModelState.AddModelError("Guid", "Не указан guid");
                if (!db.Users.Any(x => x.Guid == user.Guid))
                    ModelState.AddModelError("Guid", "Пользователь не найден");
                if (IsAdmin() || (IsCurrentUser(user.Login) && db.Users.Any(x => x.Login == user.Login && x.RevokedOn == user.RevokedOn)))
                {
                    var dbuser = db.Users.First(x => x.Guid == user.Guid);
                    dbuser.Name = user.Name;
                    dbuser.Gender = user.Gender;
                    dbuser.Birthday = user.Birthday;
                    dbuser.Password = user.Password;
                    dbuser.Login = user.Login;
                    dbuser.ModifiedBy = reqUser();
                    dbuser.ModifiedOn = DateTime.Now;
                    if (user.RevokedOn != dbuser.RevokedOn && IsAdmin())
                    {
                        if (user.RevokedOn == null)
                        { dbuser.RevokedOn = null; dbuser.RevokedBy = null; }
                        else
                        { dbuser.RevokedOn = DateTime.Now; dbuser.RevokedBy = reqUser(); }
                    }

                    db.Update(dbuser);
                    await db.SaveChangesAsync();
                    return Ok(dbuser);
                }
                else
                    ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);
        }

        //удалить пользователя по логину
        [HttpDelete("{login}")]
        public async Task<ActionResult<User>> Delete(string login)
        {
            try
            {
                User user = db.Users.FirstOrDefault(x => x.Login == login);
                if (user == null)
                    ModelState.AddModelError("Login", "Пользователь не найден");
                if (IsAdmin())
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return Ok(user);
                }
                else ModelState.AddModelError("Login", "Доступ запрещен");
            }
            catch (Exception ex) { ModelState.AddModelError("Guid", "Unexpected exception! " + ex.Message); }
            return BadRequest(ModelState);

        }


        private bool IsCurrentUser(string userlogin)
        {
            string currentlogin = reqUser();
            if (userlogin == currentlogin)
            {
                string password = HttpContext.Request.Headers["Authorization"].ToString().Replace(currentlogin + " ", "");
                if (db.Users.FirstOrDefaultAsync(x => x.Login == currentlogin && x.Password == password&& x.RevokedOn==null) == null)
                    return true;
                return false;
            }
            return false;
        }
        private bool IsAdmin()
        {
            string currentlogin = reqUser();
            string password = HttpContext.Request.Headers["Authorization"].ToString().Replace(currentlogin + " ", "");
            return db.Users.Any(x => x.Login == currentlogin && x.Password == password && x.Admin == true);
           
        }
        private string reqUser()
        {
            return HttpContext.Request.Headers["Authorization"].ToString().Substring(0, HttpContext.Request.Headers["Authorization"].ToString().IndexOf(" "));
        }
    }
}
