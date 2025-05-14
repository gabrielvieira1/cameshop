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
        logger.LogWarning("Erro {Code}: {Message} - ID: {ItemId}", DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message, id);
        return NotFound(new { DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message });
      }

      return Ok(item.AsDto());
    }

    // POST /items
    [Authorize(Roles = "Admin,Cliente")]
    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
    {
      if (!IsValidItem(itemDto.Name, itemDto.Description, itemDto.Price))
      {
        logger.LogWarning("Erro {Code}: {Message}",
            DomainErrors.Item.InvalidCreatedItem.Code,
            DomainErrors.Item.InvalidCreatedItem.Message);

        return BadRequest(new
        {
          Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
          DomainError = new
          {
            DomainErrors.Item.InvalidCreatedItem.Code,
            DomainErrors.Item.InvalidCreatedItem.Message
          }
        });
      }

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

        //return Created("ok", CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto()));
        return Ok(item.AsDto());
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Erro {Code}: {Message}", DomainErrors.Item.UnexpectedError.Code, DomainErrors.Item.UnexpectedError.Message);
        return StatusCode(500, new { DomainErrors.Item.UnexpectedError.Code, DomainErrors.Item.UnexpectedError.Message });
      }
    }

    // PUT /items/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
    {
      if (!IsValidItem(itemDto.Name, itemDto.Description, itemDto.Price))
      {
        logger.LogWarning("Erro {Code}: {Message} - ID: {ItemId}",
            DomainErrors.Item.InvalidUpdatedeItem.Code,
            DomainErrors.Item.InvalidUpdatedeItem.Message,
            id);

        return BadRequest(new
        {
          Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
          DomainError = new
          {
            DomainErrors.Item.InvalidUpdatedeItem.Code,
            DomainErrors.Item.InvalidUpdatedeItem.Message
          }
        });
      }

      try
      {
        var existingItem = await repository.GetItemAsync(id);
        if (existingItem is null)
        {
          logger.LogWarning("Erro {Code}: {Message} - ID: {ItemId}", DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message, id);
          return NotFound(new { DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message });
        }

        existingItem.Name = itemDto.Name;
        existingItem.Description = itemDto.Description;
        existingItem.Price = itemDto.Price;

        await repository.UpdateItemAsync(existingItem);

        logger.LogInformation("Item atualizado: {ItemId}", id);
        return Ok();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Erro {Code}: {Message}", DomainErrors.Item.UnexpectedError.Code, DomainErrors.Item.UnexpectedError.Message);
        return StatusCode(500, new { DomainErrors.Item.UnexpectedError.Code, DomainErrors.Item.UnexpectedError.Message });
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
        logger.LogWarning("Erro {Code}: {Message} - ID: {ItemId}", DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message, id);
        return NotFound(new { DomainErrors.Item.NotFound.Code, DomainErrors.Item.NotFound.Message });
      }

      await repository.DeleteItemAsync(id);

      logger.LogInformation("Item deletado com sucesso: {ItemId}", id);
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