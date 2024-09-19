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
        public List<DateTime>? BookedDays { get; set; } = new List<DateTime>();

        // Navigation property for Bookings
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
