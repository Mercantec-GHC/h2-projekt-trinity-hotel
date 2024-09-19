using Microsoft.AspNetCore.Mvc;
using DomainModels;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly HotelContext _hotelContext;

        public RoomsController(HotelContext hotelContext)
        {
            _hotelContext = hotelContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            try
            {
                //fetches a list of rooms with specific fields only
                var rooms = await _hotelContext.Rooms
                    .Select(r => new { r.RoomId, r.Type, r.Price})
                    .ToListAsync();
                //Returns an Ok(200) response with the list of rooms
                return Ok(rooms);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id) 
        {
            try
            {
                //Attempts to find the room by its ID
                var room = await _hotelContext.Rooms.FindAsync(id);

                if (room == null)
                {
                    return NotFound();
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<Room>>> GetTypes()
        {
            try
            {
                // Get all exist rooms from the database
                var allRooms = await _hotelContext.Rooms.ToListAsync();

                // Create a new list of rooms
                List<Room> rooms = new List<Room>();

                foreach (var DBRoom in allRooms)
                {
                    var searchedRoom = rooms.Find(r => r.Type == DBRoom.Type);
                    if (searchedRoom != null)
                    {
                        continue;
                    }
                    rooms.Add(DBRoom);
                }

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostRoom(Room room)
        {
            try
            {
                var newroom = new Room()
                {
                    Type = room.Type,
                    Price = room.Price
                };
                //Adds the new room to the context and saves changes asynchronously
                _hotelContext.Rooms.Add(newroom);
                await _hotelContext.SaveChangesAsync();

                return Ok(StatusCode(200));
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            //Finds the existing room by its ID
            var existingRoom = await _hotelContext.Rooms.FindAsync(id);

            // Returns a 404 Not Found if the room doesn't exist
            if (existingRoom == null)
            {
                return NotFound();
            }


            // Mark the room entity as modified in the context
            _hotelContext.Entry(existingRoom).State = EntityState.Modified;

            try
            {
                // Save the changes to the database asynchronously
                await _hotelContext.SaveChangesAsync();
            }
            catch
            {
                //Checks if the room still exists before rethrowing the exception
                if (!_hotelContext.Rooms.Any(r => r.RoomId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(200);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                //find the room by its ID.
                var room = await _hotelContext.Rooms.FindAsync(id);

                if (room == null)
                {
                    return NotFound();
                }
                //// Removes the room from the context and saves changes asynchronously
                _hotelContext.Rooms.Remove(room);
                await _hotelContext.SaveChangesAsync();

                // Returns a 204 No Content after successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}