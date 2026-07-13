using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaEra.API.Data;
using NovaEra.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace NovaEra.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
private readonly IWebHostEnvironment _environment;

public ProductsController(
    ApplicationDbContext context,
    IWebHostEnvironment environment)
{
    _context = context;
    _environment = environment;
}


    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
      var products = await _context.Products
    .Select(p => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.Stock,
        ImageUrl = string.IsNullOrEmpty(p.ImageUrl)
            ? null
            : $"{Request.Scheme}://{Request.Host}{p.ImageUrl}"
    })
    .ToListAsync();

return Ok(products);
    }


    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return Ok(product);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
{
    if (id != product.Id)
        return BadRequest();

    _context.Entry(product).State = EntityState.Modified;

    await _context.SaveChangesAsync();

    return Ok(product);
}
[HttpPost("{id}/image")]
public async Task<IActionResult> UploadImage(
    int id,
    IFormFile file)
{
    var product = await _context.Products.FindAsync(id);

    if (product == null)
        return NotFound("Product not found");


    if (file == null || file.Length == 0)
        return BadRequest("No image uploaded");


    var uploadsFolder = Path.Combine(
        _environment.WebRootPath,
        "images"
    );


    if (!Directory.Exists(uploadsFolder))
        Directory.CreateDirectory(uploadsFolder);


    var fileName = Guid.NewGuid().ToString()
        + Path.GetExtension(file.FileName);


    var filePath = Path.Combine(
        uploadsFolder,
        fileName
    );


    using var stream = new FileStream(
        filePath,
        FileMode.Create
    );

    await file.CopyToAsync(stream);


    product.ImageUrl = "/images/" + fileName;


    await _context.SaveChangesAsync();


    return Ok(new
    {
        message = "Image uploaded successfully",
        imageUrl = product.ImageUrl
    });
}
}