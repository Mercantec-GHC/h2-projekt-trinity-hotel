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

            var bookings = await _hotelContext.Bookings.ToArrayAsync();
     
            return Ok(bookings);
        }

        [HttpGet("id/{BookingId}")]
        public async Task<ActionResult<Booking>> GetBookingById(int BookingId)
        {

            var booking = await _hotelContext.Bookings
                .Where(b => b.BookingId == BookingId)
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBooking([FromBody] Booking booking)
        {
            var room = _hotelContext.Rooms.Find(booking.RoomId);

            var bookings = _hotelContext.Bookings.ToArray();
            var existingBookings = _hotelContext.Bookings
            .Where(b => b.RoomId == booking.RoomId &&
                ((booking.StartDate >= b.StartDate && booking.StartDate < b.EndDate) ||
                 (booking.EndDate > b.StartDate && booking.EndDate <= b.EndDate) ||
                 (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate)))
                .ToList();

            if (existingBookings.Any())
            {
                return BadRequest("Room is already booked for the selected dates.");
            }


            // Data validation
            if (room == null)
            {
                return NotFound("room not found");
            }

            var user = await _hotelContext.Users.FindAsync(booking.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (booking.StartDate >= booking.EndDate)
            {
                return BadRequest("Invalid date range");
            }

            if (booking.StartDate < DateTime.Now.Date)
            {
                return BadRequest("Invalid date range");
            }

            var newbooking = new Booking
            {
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                StartDate = DateTime.SpecifyKind(booking.StartDate, DateTimeKind.Utc).Date,
                EndDate = DateTime.SpecifyKind(booking.EndDate, DateTimeKind.Utc).Date
            };
            _hotelContext.Bookings.Add(newbooking);

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
                        existingBooking.StartDate = booking.StartDate;
                        existingBooking.EndDate = booking.EndDate;

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
            if (booking.BookingId < 0)
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

            if (booking.StartDate < DateTime.Now.Date)
            {
                return BadRequest("Invalid date range");
            }

            existingBooking.StartDate = DateTime.SpecifyKind(booking.StartDate, DateTimeKind.Utc);
            existingBooking.EndDate = DateTime.SpecifyKind(booking.EndDate, DateTimeKind.Utc);

            _hotelContext.Entry(existingBooking).State = EntityState.Modified;
            _hotelContext.SaveChanges();

            return Ok("Done");
        }

        [HttpDelete("id/{BookingId}")]
        public async Task<ActionResult<Booking>> DeleteBooking(int BookingId)
        {
            var booking = _hotelContext.Bookings.Where(b => b.BookingId == BookingId).FirstOrDefault();
            if (booking == null)
            {
                return NotFound();
            }


            _hotelContext.Bookings.Remove(booking);
            await _hotelContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("get_bookings_user/{UserId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> get_bookings_user(int UserId)
        {
            var booking = await _hotelContext.Bookings.Where(b => b.UserId == UserId).ToListAsync();
            if (booking == null)
            {
                return NotFound();
            }


            return Ok(booking);
        }

        [HttpGet("max-booking-id")]
        public async Task<ActionResult<int>> GetMaxBookingId()
        {
            // Get the maximum BookingId from the database
            var maxBookingId = await _hotelContext.Bookings.MaxAsync(b => b.BookingId);

            return Ok(maxBookingId);
        }


    }
}