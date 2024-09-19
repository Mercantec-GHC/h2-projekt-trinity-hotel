using Microsoft.AspNetCore.Mvc;
using API.Data;
using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly HotelContext _hotelContext;

        public BookingsController(HotelContext hotelContext)
        {
            _hotelContext = hotelContext;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var bookings = await _hotelContext.Bookings.Include(b => b.Room).ToArrayAsync();
            return Ok(bookings);
        }

        [HttpGet("id/{BookingId}")]
        public async Task<ActionResult<Booking>> GetBookingById(int BookingId)
        {

            var booking = await _hotelContext.Bookings
                .Where(b => b.BookingId == BookingId)
                .Include(b => b.Room)
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpPost("add")]
        public ActionResult AddBooking(Booking booking)
        {
            var room = _hotelContext.Rooms.Find(booking.RoomId);

            var bookings = _hotelContext.Bookings.ToArray();
            for (int i = 0; i<bookings.Length; i++)
            {
                if (bookings[i].RoomId == room.RoomId)
                {
                    if (booking.StartDate < bookings[i].StartDate && booking.EndDate > bookings[i].EndDate)
                    {
                        //Overlap
                        return BadRequest("Date Already booked");
                    }
                }
            }

            // Data validation
            if (room == null)
            {
                return NotFound("room not found");
            }

            if (booking.StartDate >= booking.EndDate)
            {
                return BadRequest("Invalid date range");
            }

            if (booking.StartDate < DateTime.Now)
            {
                return BadRequest("Invalid date range");
            }

            var newbooking = new Booking
            {
                Room = room,
                FullName = booking.FullName,
                Email = booking.Email,
                PhoneNr = booking.PhoneNr,
                StartDate = DateTime.SpecifyKind(booking.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(booking.EndDate, DateTimeKind.Utc)
            };
            _hotelContext.Bookings.Add(newbooking);

            //update room booked days.
            room.BookedDays.AddRange(Enumerable
                .Range(0, (int)(newbooking.EndDate - newbooking.StartDate).TotalDays)
                .Select(i => DateTime.SpecifyKind(newbooking.StartDate, DateTimeKind.Utc)
                .AddDays(i)));

            _hotelContext.SaveChanges();

            return Ok("Done");
        }

        [HttpPost]
        public async Task<IActionResult> SaveBookings(List<Booking> bookings)
        {
            if (ModelState.IsValid)
            {
                foreach (var booking in bookings)
                {
                    var existingBooking = await _hotelContext.Bookings.FindAsync(booking.BookingId);
                    if (existingBooking != null)
                    {
                        existingBooking.Room.Type = booking.Room.Type;
                        existingBooking.StartDate = booking.StartDate;
                        existingBooking.EndDate = booking.EndDate;
                        existingBooking.Email = booking.Email;
                        existingBooking.PhoneNr = booking.PhoneNr;

                        _hotelContext.Update(existingBooking);
                    }
                }
                await _hotelContext.SaveChangesAsync();
            }
            return Ok(bookings);
        }

        [HttpPut("update")]

        [ProducesResponseType(204)]
        public ActionResult UpdateBooking(Booking booking)
        {
            if (booking.BookingId <= 0)
            {
                return BadRequest("Invalid booking id");
            }

            var existingBooking = _hotelContext.Bookings.Find(booking.BookingId);
            if (existingBooking == null)
            {
                return NotFound("booking not found");
            }

            var user = _hotelContext.Users.Find(booking.UserId);
            if (user == null)
            {
                return NotFound("user not found");
            }

            var room = _hotelContext.Rooms.Find(booking.RoomId);
            if (room == null)
            {
                return NotFound("room not found");
            }

            if (booking.StartDate >= booking.EndDate)
            {
                return BadRequest("Invalid date range");
            }

            if (booking.StartDate < DateTime.Now)
            {
                return BadRequest("Invalid date range");
            }

            existingBooking.Room = room;
            existingBooking.FullName = booking.FullName;
            existingBooking.Email = booking.Email;
            existingBooking.PhoneNr = booking.PhoneNr;
            existingBooking.StartDate = DateTime.SpecifyKind(booking.StartDate, DateTimeKind.Utc);
            existingBooking.EndDate = DateTime.SpecifyKind(booking.EndDate, DateTimeKind.Utc);

            _hotelContext.Entry(existingBooking).State = EntityState.Modified;
            _hotelContext.SaveChanges();

            return Ok("Done");
        }

        [HttpDelete("id/{BookingId}")]
        public async Task<ActionResult<Booking>> DeleteBooking(int BookingId)
        {
            var booking = _hotelContext.Bookings.Include(b => b.Room).Where(b => b.BookingId == BookingId).FirstOrDefault();
            if (booking == null)
            {
                return NotFound();
            }


            _hotelContext.Bookings.Remove(booking);
            await _hotelContext.SaveChangesAsync();
            return NoContent();
        }
    }
}