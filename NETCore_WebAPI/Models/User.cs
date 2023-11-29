using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NETCore_WebAPI.Models
{
    public class User
    {
        [Key]
        public Guid Guid { get; set; }
        [Required(ErrorMessage ="Укажите логин"), RegularExpression(@"[A-Za-z0-9]*", ErrorMessage ="Не корректный логин. Использовать только латинские буквы и цифры")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Укажите пароль"), RegularExpression(@"[A-Za-z0-9]*", ErrorMessage = "Не корректный пароль. Использовать только латинские буквы и цифры")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Укажите имя"), RegularExpression(@"[A-Za-zА-Яа-я]*", ErrorMessage = "Не корректное имя. Использовать только латинские и русские буквы")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Укажите пол"),Range(0,2, ErrorMessage ="0 - женщина, 1- мужчина, 2 - неизвестно")]
        public int Gender { get; set; }
        [Required(ErrorMessage = "Укажите дату рождения")]
        public DateTime? Birthday { get; set; }
        [Required(ErrorMessage = "Укажите является ли пользователь админом")]
        public bool Admin { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? RevokedOn { get; set; }
        public string RevokedBy { get; set; }
    }
}
