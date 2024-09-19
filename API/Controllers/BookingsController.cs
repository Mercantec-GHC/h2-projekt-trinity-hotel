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
            if (booking == null)
            {
                return BadRequest("Booking data is required.");
            }

            // Validate RoomId and UserId
            var room = await _hotelContext.Rooms.FindAsync(booking.RoomId);
            if (room == null)
            {
                return NotFound("Room not found.");
            }

            var user = await _hotelContext.Users.FindAsync(booking.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Add the booking directly
            _hotelContext.Bookings.Add(booking);

            // Update Room's BookedDays if needed (assuming logic exists)
            // UpdateUser (assuming logic exists)

            await _hotelContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookingById), new { BookingId = booking.BookingId }, booking);
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
                        // Update existing booking properties
                        existingBooking.StartDate = booking.StartDate;
                        existingBooking.EndDate = booking.EndDate;
                        existingBooking.Email = booking.Email;
                        existingBooking.PhoneNr = booking.PhoneNr;

                        // Update Room and User if necessary (assuming logic exists)
                        // ...
                    }
                }

                await _hotelContext.SaveChangesAsync();
            }

            return Ok(bookings);
        }

        [HttpPut("update")]
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

            // Update booking properties
            existingBooking.StartDate = booking.StartDate;
            existingBooking.EndDate = booking.EndDate;
            existingBooking.Email = booking.Email;
            existingBooking.PhoneNr = booking.PhoneNr;

            // Update Room and User if necessary (assuming logic exists)
            // ...

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
    }
}