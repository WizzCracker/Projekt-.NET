using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models;
using Projekt_NET.Models.System;

namespace Projekt_NET.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DroneCloudsControllerApi : ControllerBase
    {
        private readonly DroneDbContext _context;

        public DroneCloudsControllerApi(DroneDbContext context)
        {
            _context = context;
        }

        // GET: api/DroneCloudsControllerApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DroneCloud>>> GetDroneClouds()
        {
            return await _context.DroneClouds.ToListAsync();
        }

        // GET: api/DroneCloudsControllerApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DroneCloud>> GetDroneCloud(int id)
        {
            var droneCloud = await _context.DroneClouds.FindAsync(id);

            if (droneCloud == null)
            {
                return NotFound();
            }

            return droneCloud;
        }

        // PUT: api/DroneCloudsControllerApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDroneCloud(int id, DroneCloud droneCloud)
        {
            if (id != droneCloud.DroneCloudId)
            {
                return BadRequest();
            }

            _context.Entry(droneCloud).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DroneCloudExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DroneCloudsControllerApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DroneCloud>> PostDroneCloud(DroneCloud droneCloud)
        {
            _context.DroneClouds.Add(droneCloud);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDroneCloud", new { id = droneCloud.DroneCloudId }, droneCloud);
        }

        // DELETE: api/DroneCloudsControllerApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDroneCloud(int id)
        {
            var droneCloud = await _context.DroneClouds.FindAsync(id);
            if (droneCloud == null)
            {
                return NotFound();
            }

            _context.DroneClouds.Remove(droneCloud);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DroneCloudExists(int id)
        {
            return _context.DroneClouds.Any(e => e.DroneCloudId == id);
        }
    }
}
