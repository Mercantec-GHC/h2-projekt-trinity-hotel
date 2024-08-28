﻿using Microsoft.AspNetCore.Mvc;
using DomainModels;
using API.Data;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UsersController : ControllerBase
	{

		private readonly HotelContext _hotelContext;

		public UsersController(HotelContext hotelContext)
		{
			_hotelContext = hotelContext;
		}

		[HttpGet]
		public IEnumerable<User> Get()
		{
			return _hotelContext.Users.ToArray();
		}

		[HttpPost]
		public void Post([FromBody] User user)
		{
			_hotelContext.Users.Add(user);
			_hotelContext.SaveChanges();
		}
	}
}