using System;
using System.Collections.Generic;
using System.Linq;
using ApiControllers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ApiControllers.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger _logger;

        private IRepository repository;

        private IWebHostEnvironment webHostEnvironment;

        public ReservationController(IRepository repo, IWebHostEnvironment environment, ILogger<ReservationController> logger)
        {
            repository = repo;
            _logger = logger;
            webHostEnvironment = environment;
        }

        [HttpGet]
        [Produces("application/xml")]
        public IEnumerable<Reservation> Get()
        {
            //repository.Reservations;
            Authenticate();
            return repository.Reservations;
        }

        [HttpGet("{id}")]
        public Reservation Get(int id) => repository[id];

        [HttpGet("ShowReservation.{format}"), FormatFilter]
        public IEnumerable<Reservation> ShowReservation() => repository.Reservations;

        [HttpPost("UploadFile")]
        public async Task<string> UploadFile([FromForm] IFormFile file)
        {
            string path = Path.Combine(webHostEnvironment.WebRootPath, "Images/" + file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "http://localhost:8888/Images/" + file.FileName;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Reservation res)
        {
            _logger.LogInformation("Post reservation from body");
            if (!Authenticate())
            {
                return Unauthorized();
            }
            return Ok(repository.AddReservation(new Reservation
            {
                Name = res.Name,
                StartLocation = res.StartLocation,
                EndLocation = res.EndLocation
            }));
        }

        [HttpPut]
        public Reservation Put([FromBody] Reservation res) => repository.UpdateReservation(res);

        [HttpPatch("{id}")]
        public StatusCodeResult Patch(int id, [FromBody]JsonPatchDocument<Reservation> patch)
        {
            _logger.LogInformation("Patching...");
            _logger.LogInformation(patch.ToString());
            Reservation res = Get(id);
            if (res != null)
            {
                patch.ApplyTo(res);
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public void Delete(int id) => repository.DeleteReservation(id);

        [HttpPost("PostXml")]
        [Consumes("application/xml")]
        public Reservation PostXml([FromBody] System.Xml.Linq.XElement res)
        {
            return repository.AddReservation(new Reservation
            {
                Name = res.Element("Name").Value,
                StartLocation = res.Element("StartLocation").Value,
                EndLocation = res.Element("EndLocation").Value
            });
        }
        bool Authenticate()
        {
            var allowedKeys = new[] { "Secret@123", "Secret#12", "SecretABC" };
            StringValues key = Request.Headers["Key"];
            int count = (from t in allowedKeys where t == key select t).Count();
            return count == 0 ? false : true;
        }
    }
}
