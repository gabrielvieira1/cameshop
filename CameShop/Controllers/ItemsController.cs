﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cameshop.Entities;
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
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetItemsAsync(string name = null)
    {
      var items = (await repository.GetItemsAsync())
                  .Select(item => item.AsDto());

      if (!string.IsNullOrWhiteSpace(name))
      {
        items = items.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
      }

      logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");

      return items;
    }

    // GET /items/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
    {
      var item = await repository.GetItemAsync(id);

      if (item is null)
      {
        return NotFound();
      }

      return item.AsDto();
    }

    // POST /items
    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
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


      return Created("ok", CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto()));
    }

    // PUT /items/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
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

    // DELETE /items/{id}
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
  }

}