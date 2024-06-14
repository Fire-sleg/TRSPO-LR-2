using LR_2.Data;
using LR_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LR_2.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:Guid}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUserAsync(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            if (users == null || users.Count == 0)
            {
                return NotFound();
            }
            return Ok(users);
        }

        [HttpPut("{id:Guid}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateUser(Guid id, [FromBody] User user)
        {
            if (user == null || id != user.Id)
            {
                return BadRequest();
            }

            var userUp = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            userUp.Email = user.Email;
            userUp.Password = user.Password;

            _context.Users.Update(userUp);

            return NoContent();
        }
        [HttpPatch("{id:Guid}", Name = "UpdatePartialUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdatePartialUserAsync(Guid id, JsonPatchDocument<User> patch)
        {
            if (patch == null || id == null)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return BadRequest();
            }

            patch.ApplyTo(user, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id:Guid}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (_context.Users.FirstOrDefault(u => u.Email.ToLower() == user.Email.ToLower()) != null)
            {
                ModelState.AddModelError("", "User already exists!");
                return BadRequest(ModelState);
            }
            if (user == null)
            {
                return BadRequest(user);
            }
            user.Id = new Guid();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetProduct", new { id = user.Id }, user);
        }
    }
}
