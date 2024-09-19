using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DomainModels
{
    public class Room
    {
        public int RoomId { get; set; }
        public int Type { get; set; } 
        public int Price { get; set; }
        // Navigation property for related bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
