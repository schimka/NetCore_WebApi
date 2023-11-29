using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NETCore_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NETCore_WebAPI.Models
{
    public class UserRepository:IUserRepository
    {
        private UsersContext db;
        public UserRepository(UsersContext context)
        {
            db = context;
            if (!db.Users.Any())
            {
                db.Users.Add(new Models.User
                {
                    Guid = new Guid(),
                    Login = "Admin",
                    Password = "password",
                    Name = "Alina",
                    Gender = 0,
                    Birthday = new DateTime(1992, 1, 1),
                    Admin = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = "Admin",
                });
                db.SaveChanges();
            }
        }
        #region GET
        public async Task<ActionResult<IEnumerable<User>>> GetAll(string reqUser, string reqPassword)
        {

            if (IsAdmin(reqUser, reqPassword))
            {
                return await db.Users.OrderBy(x => x.CreatedOn).ToListAsync();
            }
            else throw new Exception("Доступ запрещен");
        }
        public async Task<ActionResult<IEnumerable<User>>> GetAllActive(string reqUser, string reqPassword)
        {
            if (IsAdmin(reqUser, reqPassword))
            {
                return await db.Users.OrderBy(x => x.CreatedOn).Where(x => x.RevokedOn == null).ToListAsync();
            }
            else throw new Exception("Доступ запрещен");
        }
        public async Task<ActionResult<User>> GetUser(string login, string reqUser, string reqPassword)
        {
            if (login == null)
                throw new Exception("Не указан логин");
            if (IsAdmin(reqUser, reqPassword) || IsCurrentUser(login, reqUser, reqPassword))
            {
                User user = await db.Users.FirstOrDefaultAsync(x => x.Login == login);
                if (user == null)
                    throw new Exception("Польщователь не найден");
                else return new ObjectResult(user);
            }
            else throw new Exception("Доступ запрещен");
        }
        public async Task<ActionResult<IEnumerable<User>>> GetByAge(int age, string reqUser, string reqPassword)
        {
            if (IsAdmin(reqUser, reqPassword))
            {
                DateTime date = DateTime.Now.AddYears(-age);
                return await db.Users.Where(x => x.Birthday < date).ToListAsync();
            }
            else throw new Exception("Доступ запрещен");
        }
        #endregion

        #region UPDATE        
        public async Task<ActionResult<User>> Revoke(string login, string reqUser, string reqPassword)
        {
            if (login == null)
                throw new Exception("Не указан логин");
            if (IsAdmin(reqUser, reqPassword))
            {
                User user = await db.Users.FirstOrDefaultAsync(x => x.Login == login);
                if (user == null)
                    throw new Exception("Польщователь не найден");
                else
                {
                    user.ModifiedBy = reqUser;
                    user.ModifiedOn = DateTime.Now;
                    if (user.RevokedOn == null)
                    {
                        user.RevokedBy = reqUser;
                        user.RevokedOn = DateTime.Now;
                    }
                    else
                    {
                        user.RevokedBy = null;
                        user.RevokedOn = null;
                    }
                    db.Update(user); ;
                    await db.SaveChangesAsync();
                    return (user);
                }
            }
            else throw new Exception("Доступ запрещен");
        }
        public async Task<ActionResult<User>> UpdateUser(User user, string reqUser, string reqPassword)
        {
            if (user == null || user.Guid == null)
                throw new Exception("Не указан guid");
            if (!db.Users.Any(x => x.Guid == user.Guid))
                throw new Exception("Пользователь не найден");
            if (IsAdmin(reqUser, reqPassword) || (IsCurrentUser(user.Login, reqUser, reqPassword) && db.Users.Any(x => x.Login == user.Login && x.RevokedOn == user.RevokedOn)))
            {
                var dbuser = db.Users.First(x => x.Guid == user.Guid);
                dbuser.Name = user.Name;
                dbuser.Gender = user.Gender;
                dbuser.Birthday = user.Birthday;
                dbuser.Password = user.Password;
                dbuser.Login = user.Login;
                dbuser.ModifiedBy = reqUser;
                dbuser.ModifiedOn = DateTime.Now;                

                db.Update(dbuser);
                await db.SaveChangesAsync();
                return (dbuser);
            }
            else
                throw new Exception("Доступ запрещен");
        }
        #endregion


        public async Task<ActionResult<User>> Add(User user, string reqUser, string reqPassword)
        {
            if (user == null || user.Login == null)
                throw new Exception("Не указан логин");
            else if (!IsAdmin(reqUser, reqPassword))
                throw new Exception("Создание новых пользователей доступно только администраторам");
            else if (db.Users.Any(x => x.Login == user.Login))
                throw new Exception("Пользователь c таким логином уже существует");
            else
            {
                user.CreatedBy = reqUser;
                user.CreatedOn = DateTime.Now;
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return (user);
            }
        }

        public async Task<ActionResult<User>> Delete(string login, string reqUser, string reqPassword)
        {
            User user = db.Users.FirstOrDefault(x => x.Login == login);
            if (user == null)
                throw new Exception("Пользователь не найден");
            if (IsAdmin(reqUser, reqPassword))
            {
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return (user);
            }
            else throw new Exception("Доступ запрещен");
        }

        #region PRIVATE
        private bool IsCurrentUser(string userlogin, string currentlogin, string password)
        {
            if (userlogin == currentlogin)
            {
                if (db.Users.FirstOrDefaultAsync(x => x.Login == currentlogin && x.Password == password && x.RevokedOn == null) == null)
                    return true;
                return false;
            }
            return false;
        }
        private bool IsAdmin(string currentlogin, string password)
        {
            return db.Users.Any(x => x.Login == currentlogin && x.Password == password && x.Admin == true);
        }
        #endregion

    }
}

