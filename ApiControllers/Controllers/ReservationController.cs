using System;
using System.Collections.Generic;
using System.Linq;
using ApiControllers.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace ApiControllers.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger _logger;

        private IRepository repository;
        public ReservationController(IRepository repo, ILogger<ReservationController> logger)
        {
            repository = repo;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Reservation> Get() => repository.Reservations;

        [HttpGet("{id}")]
        public Reservation Get(int id) => repository[id];

        [HttpPost]
        public Reservation Post([FromBody] Reservation res)
        {
            _logger.LogInformation("Post reservation from body");
            return repository.AddReservation(new Reservation
            {
                Name = res.Name,
                StartLocation = res.StartLocation,
                EndLocation = res.EndLocation
            });
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
    }
}
