using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
	[Route("api/[controller]")]
	[ApiController]
	public class VehiclesController : ControllerBase {
		private readonly IAutobarnDatabase db;

		public VehiclesController(IAutobarnDatabase db) {
			this.db = db;
		}

		// GET: api/vehicles
		[HttpGet]
		[Produces("application/hal+json")]
		public IActionResult Get(char index = 'A') {
			if((int) index < 122 && (int) index > 97)
			{
				index = (char) ((int) index - 40);
			}
			if((int) index < 65 || (int) index > 90)
			{
				return NotFound();
			}
			var items = db.ListVehicles().Where(v => v.Registration.StartsWith(index)).ToList();
			var total = db.CountVehicles();
			var _links = Paginate("/api/vehicles", index);
			var result = new { _links, index, items.Count, total, items };
			return Ok(result);
}
		// GET api/vehicles/ABC123
		[HttpGet("{id}")]
		public IActionResult Get(string id) {
			var vehicle = db.FindVehicle(id);
			if (vehicle == default) return NotFound();
			return Ok(vehicle);
		}

		// POST api/vehicles
		[HttpPost]
		public IActionResult Post([FromBody] VehicleDto dto) {
			var vehicleModel = db.FindModel(dto.ModelCode);
			var vehicle = new Vehicle {
				Registration = dto.Registration,
				Color = dto.Color,
				Year = dto.Year,
				VehicleModel = vehicleModel
			};
			db.CreateVehicle(vehicle);
			return Ok(dto);
		}

		// PUT api/vehicles/ABC123
		[HttpPut("{id}")]
		public IActionResult Put(string id, [FromBody] VehicleDto dto) {
			var vehicleModel = db.FindModel(dto.ModelCode);
			var vehicle = new Vehicle {
				Registration = dto.Registration,
				Color = dto.Color,
				Year = dto.Year,
				ModelCode = vehicleModel.Code
			};
			db.UpdateVehicle(vehicle);
			return Ok(dto);
		}

		// DELETE api/vehicles/ABC123
		[HttpDelete("{id}")]
		public IActionResult Delete(string id) {
			var vehicle = db.FindVehicle(id);
			if (vehicle == default) return NotFound();
			db.DeleteVehicle(vehicle);
			return NoContent();
		}

		private char getNextChar(char letter)
		{
			char nextChar;
			if (letter == 'Z')
				nextChar = 'A';
			else
				nextChar = (char)(((int)letter) + 1);
			return nextChar;
		}

		private char getPreviousChar(char letter)
		{
			char previousChar;
			if (letter == 'A')
				previousChar = 'Z';
			else
				previousChar = (char)(((int)letter) -1);
			return previousChar;
		}

		private dynamic Paginate(string url, char firstLetter) {
			dynamic links = new ExpandoObject();
			links.self = new { href = url };
			links.final = new { href = $"{url}?index=Z" };
			links.first = new { href = $"{url}?index=A" };
			links.previous = new { href = $"{url}?index={getPreviousChar(firstLetter)}" };
			links.next = new { href = $"{url}?index={getNextChar(firstLetter)}"};
			return links;
		}
	}
}
