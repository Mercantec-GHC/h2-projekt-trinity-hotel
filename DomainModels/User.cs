using DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
	public class User
	{
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("fullname")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("administrator")]
        public bool Administrator { get; set; } = false;

        [MaxLength(20)]
        [Column("phonenr")]
        public string? PhoneNr { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}









