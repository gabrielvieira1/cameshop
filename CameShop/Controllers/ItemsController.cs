using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cameshop.Entities;
using Cameshop.Errors;
using Cameshop.Extensions;
using Cameshop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Cameshop.Dtos.ItemDtos;

namespace Cameshop.Controllers
{
  [Authorize]
  [EnableCors("AllowAll")]
  [Route("items")]
  [ApiController]
  public class ItemsController : ControllerBase
  {
    private readonly IItemsRepository repository;
    private readonly ILogger<ItemsController> logger;

    public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger)
    {
      this.repository = repository;
      this.logger = logger;
    }

    // GET /items
    [Authorize(Roles = "Admin,Cliente")]
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetItemsAsync(string name = null)
    {
      var items = (await repository.GetItemsAsync())
                  .Select(item => item.AsDto());

      if (!string.IsNullOrWhiteSpace(name))
      {
        items = items.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
      }

      return items;
    }

    // GET /items/{id}
    [Authorize(Roles = "Admin,Cliente")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
    {
      var item = await repository.GetItemAsync(id);

      if (item is null)
      {
        return NotFound();
      }

      return Ok(item.AsDto());
    }

    // POST /items
    [Authorize(Roles = "Admin,Cliente")]
    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
    {
      try
      {
        Item item = new()
        {
          Id = Guid.NewGuid(),
          Name = itemDto.Name,
          Description = itemDto.Description,
          Price = itemDto.Price,
          CreatedDate = DateTimeOffset.UtcNow
        };

        await repository.CreateItemAsync(item);

        return Ok(item.AsDto());
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Erro inesperado. Tente novamente.");
      }
    }

    // PUT /items/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
    {
      try
      {
        var existingItem = await repository.GetItemAsync(id);
        if (existingItem is null)
        {
          return NotFound();
        }

        existingItem.Name = itemDto.Name;
        existingItem.Description = itemDto.Description;
        existingItem.Price = itemDto.Price;

        await repository.UpdateItemAsync(existingItem);

        return Ok();
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Erro inesperado. Tente novamente.");
      }
    }

    // DELETE /items/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItemAsync(Guid id)
    {
      var existingItem = await repository.GetItemAsync(id);
      if (existingItem is null)
      {
        return NotFound();
      }

      await repository.DeleteItemAsync(id);

      return Ok();
    }

    private bool IsValidItem(string name, string description, decimal price)
    {
      bool isValid = true;

      if (!Utils.String.InputIsValid(name))
      {
        ModelState.AddModelError(nameof(name), DomainErrors.Item.InvalidName.Message);
        isValid = false;
      }

      if (!Utils.String.InputIsValid(description))
      {
        ModelState.AddModelError(nameof(description), DomainErrors.Item.InvalidDescription.Message);
        isValid = false;
      }

      if (price <= 0)
      {
        ModelState.AddModelError(nameof(price), DomainErrors.Item.InvalidPrice.Message);
        isValid = false;
      }

      return isValid;
    }
  }
}