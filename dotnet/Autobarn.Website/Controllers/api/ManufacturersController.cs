using System.Dynamic;
using System.Linq;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
	[Route ("api/[controller]")]
	[ApiController]
	public class ManufacturersController : ControllerBase {
		private readonly IAutobarnDatabase db;

		public ManufacturersController (IAutobarnDatabase db) {
			this.db = db;
		}

		// GET: api/manufacturers
		[HttpGet]
		[Produces ("application/hal+json")]
		public IActionResult Get (int index = 0, int count = 10) {
			var items = db.ListManufacturers ().Skip (index).Take (count).ToList ()
				.Select (v => v.ToResource ());
			var total = db.CountManufacturers ();
			var _links = HypermediaExtensions.Paginate ("/api/manufacturers", index, count, total);
			var result = new {
				_links,
				total,
				index,
				count = items.Count (),
				items
			};
			return Ok (result);
		}

		// GET api/manufacturers/aixam
		[HttpGet ("{code}")]
		[Produces ("application/hal+json")]
		public IActionResult Get (string code) {
			var manufacturer = db.FindManufacturer (code);
			if (manufacturer == default) return NotFound ();
			var result = manufacturer.ToDynamic ();
			var models = db.ListModels().Where(model => model.ManufacturerCode == code);
			result._links = new {
				self = new {
					href = $"/api/manufacturers/{manufacturer.Code}"
				},
				models = models,
			};
			return Ok (result);
		}
	}
}