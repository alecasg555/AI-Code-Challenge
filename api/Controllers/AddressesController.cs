using Addresses.Api.Auth;
using Addresses.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Addresses.Api.Controllers;

public record AddressDto(int Id, string Label, string Street, string City, bool IsDefault);

[ApiController]
[Authorize]
[Route("api/addresses")]
public class AddressesController(AppDbContext db) : ControllerBase
{
    // GET /api/addresses — the signed-in customer's addresses, default first.
    [HttpGet]
    public async Task<List<AddressDto>> GetAddresses()
    {
        var customerId = User.GetCustomerId();
        return await db.Addresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .Select(a => new AddressDto(a.Id, a.Label, a.Street, a.City, a.IsDefault))
            .ToListAsync();
    }

    // PUT /api/addresses/{id}/default — marks an address as the default one.
    [HttpPut("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var customerId = User.GetCustomerId();
        var address = await db.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);
        if (address is null)
        {
            return NotFound();
        }

        var customerAddresses = await db.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
        foreach (var customerAddress in customerAddresses)
        {
            customerAddress.IsDefault = customerAddress.Id == id;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/addresses/{id} — deletes an address belonging to the signed-in customer.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var customerId = User.GetCustomerId();
        var address = await db.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);
        if (address is null)
        {
            return NotFound();
        }

        db.Addresses.Remove(address);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
