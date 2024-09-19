using Microsoft.AspNetCore.Mvc;
using API.Data;
using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

        [HttpGet("name/{GuestName}")]
        public async Task<ActionResult<Booking>> GetBookingByGuestName(string GuestName)
        {
            var booking = await _hotelContext.Bookings
                .Where(b => b.GuestName == GuestName)
                .Include(b => b.Room)
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpGet("email/{GuestEmail}")]
        public async Task<ActionResult<Booking>> GetBookingByGuestEmail(string GuestEmail)
        {
            var booking = await _hotelContext.Bookings
                .Where(b => b.GuestEmail == GuestEmail)
                .Include(b => b.Room)
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpGet("emails/{GuestEmail}")]
        public async Task<ActionResult<List<Booking>>> GetBookingsByGuestEmail(string GuestEmail)
        {
            var bookings = await _hotelContext.Bookings
                .Where(b => b.GuestEmail == GuestEmail)
                .Include(b => b.Room)
                .ToListAsync();
            if (bookings == null)
            {
                return NotFound();
            }

            return Ok(bookings);
        }


        [HttpGet("phone/{GuestPhoneNr}")]
        public async Task<ActionResult<Booking>> GetBookingByGuestPhoneNr(string GuestPhoneNr)
        {
            var booking = await _hotelContext.Bookings
                .Where(b => b.GuestPhoneNr == GuestPhoneNr)
                .Include(b => b.Room)
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }



        [HttpPost("add")]

        public ActionResult AddBooking(CreateBookingDTO bookingDTO)
        {
            var room = _hotelContext.Rooms.Find(bookingDTO.RoomId);
            // Data validation
            if (room == null)
            {
                return NotFound("room not found");
            }

            if (bookingDTO.StartDate >= bookingDTO.EndDate)
            {
                return BadRequest("Invalid date range");
            }

            if (bookingDTO.StartDate < DateTime.Now)
            {
                return BadRequest("Invalid date range");
            }

            var booking = new Booking
            {
                Room = room,
                GuestName = bookingDTO.GuestName,
                GuestEmail = bookingDTO.GuestEmail,
                GuestPhoneNr = bookingDTO.GuestPhoneNr,
                StartDate = DateTime.SpecifyKind(bookingDTO.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(bookingDTO.EndDate, DateTimeKind.Utc)
            };
            _hotelContext.Bookings.Add(booking);

            //update room booked days.
            room.BookedDays.AddRange(Enumerable
                .Range(0, (int)(bookingDTO.EndDate - bookingDTO.StartDate).TotalDays)
                .Select(i => DateTime.SpecifyKind(bookingDTO.StartDate, DateTimeKind.Utc)
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
                        existingBooking.GuestEmail = booking.GuestEmail;
                        existingBooking.GuestPhoneNr = booking.GuestPhoneNr;

                        _hotelContext.Update(existingBooking);
                    }
                }
                await _hotelContext.SaveChangesAsync();
            }
            return Ok(bookings);
        }

        [HttpPut("update")]

        [ProducesResponseType(204)]
        public ActionResult UpdateBooking(UpdateBookingDTO bookingDTO)
        {

            if (bookingDTO.BookingId <= 0)
            {
                return BadRequest("Invalid booking id");
            }

            var booking = _hotelContext.Bookings.Find(bookingDTO.BookingId);
            if (booking == null)
            {
                return NotFound("booking not found");
            }

            var user = _hotelContext.Users.Find(bookingDTO.UserId);
            if (user == null)
            {
                return NotFound("user not found");
            }

            var room = _hotelContext.Rooms.Find(bookingDTO.RoomId);


            if (room == null)
            {
                return NotFound("room not found");
            }

            if (bookingDTO.StartDate >= bookingDTO.EndDate)
            {
                return BadRequest("Invalid date range");
            }

            if (bookingDTO.StartDate < DateTime.Now)
            {
                return BadRequest("Invalid date range");
            }

            booking.Room = room;
            booking.GuestName = bookingDTO.GuestName;
            booking.GuestEmail = bookingDTO.GuestEmail;
            booking.GuestPhoneNr = bookingDTO.GuestPhoneNr;
            booking.StartDate = DateTime.SpecifyKind(bookingDTO.StartDate, DateTimeKind.Utc);
            booking.EndDate = DateTime.SpecifyKind(bookingDTO.EndDate, DateTimeKind.Utc);

            _hotelContext.Entry(booking).State = EntityState.Modified;
            _hotelContext.SaveChanges();

            return Ok("Done");
        }

        [HttpDelete("id/{BookingId}")]
        public async Task<ActionResult<Booking>> DeleteBooking(int BookingId)
        {
            var booking =  _hotelContext.Bookings.Include(b => b.Room).Where(b => b.BookingId == BookingId).FirstOrDefault();
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
