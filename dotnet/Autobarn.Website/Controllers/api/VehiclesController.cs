using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
	[Route ("api/[controller]")]
	[ApiController]
	public class VehiclesController : ControllerBase {
		private readonly IAutobarnDatabase db;

		public VehiclesController (IAutobarnDatabase db) {
			this.db = db;
		}

		// GET: api/vehicles
		[HttpGet]
		[Produces ("application/hal+json")]
		public IActionResult Get (char index = 'A') {
			if ((int) index < 122 && (int) index > 97) {
				index = (char) ((int) index - 40);
			}
			if ((int) index < 65 || (int) index > 90) {
				return NotFound ();
			}
			var items = db.ListVehicles ().Where (v => v.Registration.StartsWith (index)).ToList ();
			var total = db.CountVehicles ();
			var _links = Paginate ("/api/vehicles", index);
			var result = new { _links, index, items.Count, total, items };
			return Ok (result);
		}

		// POST api/vehicles
		[HttpPost]
		public IActionResult Post ([FromBody] VehicleDto dto) {
			var vehicleModel = db.FindModel (dto.ModelCode);
			var vehicle = new Vehicle {
				Registration = dto.Registration,
					Color = dto.Color,
					Year = dto.Year,
					VehicleModel = vehicleModel
			};
			db.CreateVehicle (vehicle);
			return Ok (dto);
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

		private dynamic Paginate (string url, char firstLetter) {
			dynamic links = new ExpandoObject ();
			links.self = new { href = url };
			links.final = new { href = $"{url}?index=Z" };
			links.first = new { href = $"{url}?index=A" };
			links.previous = new { href = $"{url}?index={getPreviousChar(firstLetter)}" };
			links.next = new { href = $"{url}?index={getNextChar(firstLetter)}" };
			return links;
		}
	}
}

public static class HypermediaExtensions {
	public static dynamic ToDynamic (this object value) {
		IDictionary<string, object> expando = new ExpandoObject ();
		var properties = TypeDescriptor.GetProperties (value.GetType ());
		foreach (PropertyDescriptor prop in properties) {
			if (Ignore (prop)) continue;
			expando.Add (prop.Name, prop.GetValue (value));
		}
		return (ExpandoObject) expando;
	}

	private static bool Ignore (PropertyDescriptor prop) {
		return prop.Attributes.OfType<JsonIgnoreAttribute> ().Any ();
	}
}