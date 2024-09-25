using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FeedbacksController : ControllerBase
	{
		private readonly HotelContext _hotelContext;

		public FeedbacksController(HotelContext hotelContext)
		{
			_hotelContext = hotelContext;
		}

		// Get all Feedbacks
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Feedback>>> GetAllFeedbacks()
		{
			var feedback = await _hotelContext.Feedbacks.ToListAsync();
			if (feedback == null)
			{
				return BadRequest();
			}
			return Ok(feedback);
		}

		// Post a Feedback
		[HttpPost]
		public async Task<ActionResult<Feedback>> CreateFeedback(Feedback feedback)
		{
            if (feedback.UserId == 0)
            {
                return Unauthorized("User must be logged in to submit feedback.");
            }

            try
			{
				_hotelContext.Feedbacks.Add(feedback);
				await _hotelContext.SaveChangesAsync();
				return Ok(StatusCode(200));
			}
			catch (Exception ex)
			{
				return NotFound();
			}
		}
	}
}