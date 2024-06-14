using LR_2.Data;
using LR_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LR_2.Controllers
{
    [Route("api/Recipes")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public RecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:Guid}", Name = "GetRecipe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Recipe>> GetRecipeAsync(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var recipe = await _context.Recipes.FirstOrDefaultAsync(e => e.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipesAsync()
        {
            var recipes = await _context.Recipes.ToListAsync();
            if (recipes == null || recipes.Count == 0)
            {
                return NotFound();
            }
            return Ok(recipes);
        }

        [HttpPut("{id:Guid}", Name = "UpdateRecipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateRecipe(Guid id, [FromBody] Recipe recipe)
        {
            if (recipe == null || id != recipe.Id)
            {
                return BadRequest();
            }

            var recipeUp = await _context.Recipes.FirstOrDefaultAsync(u => u.Id == id);

            recipeUp.Name = recipe.Name;
            recipeUp.Description = recipe.Description;
            recipeUp.ImageURL = recipe.ImageURL;

            _context.Recipes.Update(recipeUp);

            return NoContent();
        }
        [HttpPatch("{id:Guid}", Name = "UpdatePartialRecipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdatePartialRecipeAsync(Guid id, JsonPatchDocument<Recipe> patch)
        {
            if (patch == null || id == null)
            {
                return BadRequest();
            }

            var recipe = await _context.Recipes.FirstOrDefaultAsync(u => u.Id == id);

            if (recipe == null)
            {
                return BadRequest();
            }

            patch.ApplyTo(recipe, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id:Guid}", Name = "DeleteRecipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRecipe(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var recipe = await _context.Recipes.FirstOrDefaultAsync(u => u.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Recipe>> CreateRecipe([FromBody] Recipe recipe)
        {
            if (_context.Recipes.FirstOrDefault(u => u.Name.ToLower() == recipe.Name.ToLower()) != null)
            {
                ModelState.AddModelError("", "Recipe already exists!");
                return BadRequest(ModelState);
            }
            if (recipe == null)
            {
                return BadRequest(recipe);
            }
            recipe.Id = new Guid();

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetRecipe", new { id = recipe.Id }, recipe);
        }
    }
}
