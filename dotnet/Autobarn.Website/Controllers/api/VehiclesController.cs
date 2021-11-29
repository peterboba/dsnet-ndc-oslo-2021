using System;
using System.Dynamic;
using System.Linq;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Messages;
using Autobarn.Website.Models;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
	[Route ("api/[controller]")]
	[ApiController]
	public class VehiclesController : ControllerBase {
		private readonly IAutobarnDatabase db;
		private readonly IBus bus;

		public VehiclesController (IAutobarnDatabase db, IBus bus) {
			this.db = db;
			this.bus = bus;
		}

		// GET: api/vehicles
		[HttpGet]
		[Produces ("application/hal+json")]
		public IActionResult Get (int index = 0, int count = 10) {
			var items = db.ListVehicles ().Skip (index).Take (count).ToList ()
				.Select (v => v.ToResource ());
			var total = db.CountVehicles ();
			var _links = HypermediaExtensions.Paginate ("/api/vehicles", index, count, total);
			var result = new {
				_links,
				total,
				index,
				count = items.Count (),
				items
			};
			return Ok (result);
		}

		// POST api/vehicles
		[HttpPost]
		public IActionResult Post([FromBody] VehicleDto dto) {
			var vehicleModel = db.FindModel(dto.ModelCode);
			if(vehicleModel==default)
			{
				return NotFound("Model not found!");
			}
			var vehicle = new Vehicle {
				Registration = dto.Registration,
				Color = dto.Color,
				Year = dto.Year,
				VehicleModel = vehicleModel
			};
			db.CreateVehicle(vehicle);
			PublishNewVehicleMessage(vehicle);
			return Created($"/api/vehicles/{vehicle.Registration}", dto);
		}

		// PUT api/vehicles/ABC123
		[HttpPut ("{id}")]
		public IActionResult Put (string id, [FromBody] VehicleDto dto) {
			var vehicleModel = db.FindModel (dto.ModelCode);
			var vehicle = new Vehicle {
				Registration = dto.Registration,
					Color = dto.Color,
					Year = dto.Year,
					ModelCode = vehicleModel.Code
			};
			db.UpdateVehicle (vehicle);
			return Ok (dto);
		}

		// // GET: api/vehicles
		// [HttpGet]
		// [Produces ("application/hal+json")]
		// public IActionResult Get (int index = 0, int count = 10) {
		// 	var items = db.ListVehicles ().Skip (index).Take (count).ToList ();
		// 	var total = db.CountVehicles ();
		// 	dynamic _links = new ExpandoObject ();
		// 	_links.self = new {
		// 		href = $"/api/vehicles?index={index}&count={count}"
		// 	};
		// 	if (index - count >= 0) {
		// 		_links.prev = new { href = $"/api/vehicles?index={index - count}&count={count}" };
		// 		_links.first = new { href = $"/api/vehicles?index=0&count={count}" };
		// 	}
		// 	if (index + count < total) {
		// 		_links.next = new { href = $"/api/vehicles?index={index + count}&count={count}" };
		// 		_links.final = new { href = $"/api/vehicles?index={total - (total % count)}&count={count}" };
		// 	}
		// 	var result = new {
		// 		_links,
		// 		total,
		// 		index,
		// 		count = items.Count (),
		// 		items
		// 	};
		// 	return Ok (result);
		// }

		// GET api/vehicles/ABC123
		[HttpGet ("{id}")]
		[Produces ("application/hal+json")]
		public IActionResult Get (string id) {
			var vehicle = db.FindVehicle (id);
			if (vehicle == default) return NotFound ();
			var result = vehicle.ToDynamic ();
			result._links = new {
				self = new {
					href = $"/api/vehicles/{vehicle.Registration}"
				}
			};
			return Ok (result);
		}

		// DELETE api/vehicles/ABC123
		[HttpDelete ("{id}")]
		public IActionResult Delete (string id) {
			var vehicle = db.FindVehicle (id);
			if (vehicle == default) return NotFound ();
			db.DeleteVehicle (vehicle);
			return NoContent ();
		}

		private char getNextChar (char letter) {
			if (letter == 'Z')
				return 'A';
			else
				return (char) (((int) letter) + 1);
		}

		private char getPreviousChar (char letter) {
			if (letter == 'A')
				return 'Z';
			else
				return (char) (((int) letter) - 1);
		}

		private void PublishNewVehicleMessage (Vehicle vehicle) {
			var message = new NewVehicleMessage () {
				Registration = vehicle.Registration,
					Manufacturer = vehicle.VehicleModel?.Manufacturer?.Name,
					ModelName = vehicle.VehicleModel?.Name,
					ModelCode = vehicle.VehicleModel?.Code,
					Color = vehicle.Color,
					Year = vehicle.Year,
					ListedAtUtc = DateTime.UtcNow
			};
			bus.PubSub.Publish (message);
		}
	}
}