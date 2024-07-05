using Azure.Storage.Blobs;
using AzureAnalyzeImage.Services;
using AzureImageSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureImageSearch.Controllers;

public class ImageController : Controller
{
    private readonly BlobServiceClient _blobServiceClient;

    private readonly ComputerVisionService _computerVisionService;

    private readonly DbImageContext _dbImageContext;

    public ImageController(IConfiguration configuration, BlobServiceClient blobServiceClient, DbImageContext dbImageContext)
    {
        _blobServiceClient = blobServiceClient;
        var subscriptionKey = configuration["AzureCognitiveServices:ComputerVision:SubscriptionKey"];
        var endpoint = configuration["AzureCognitiveServices:ComputerVision:Endpoint"];
        _computerVisionService = new ComputerVisionService(subscriptionKey, endpoint);
        _dbImageContext = dbImageContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("/postImage")]
    public async Task<ActionResult> PostImageAsync(IFormFile file)
    {
        var container = _blobServiceClient.GetBlobContainerClient("searchimages");
        await container.CreateIfNotExistsAsync();
        var client = container.GetBlobClient(file.FileName);
        await client.UploadAsync(file.OpenReadStream(), true);
        var serviceResult = await _computerVisionService.AnalyzeImageAsync(file.OpenReadStream());
        if (serviceResult.Adult.IsAdultContent || serviceResult.Tags.Any(x => x.Name == "cigarette" || x.Name == "alcohol"))
        {
            return BadRequest();
        }
        List<Tag> imageTags = new List<Tag>();
        serviceResult.Tags.ToList().ForEach(tagFromImage =>
        {
            Tag tag = new Tag
            {
                Name = tagFromImage.Name,
            };
            imageTags.Add(tag);
            _dbImageContext.Add(tag);
        });
        _dbImageContext.Images.Add(new()
        {
            Url = client.Uri.AbsoluteUri,
            Tags = imageTags,
        });
        _dbImageContext.SaveChanges();
        return Ok();
    }

    [HttpPost("/searchImage")]
    public async Task<IActionResult> SearchImage(string query)
    {
        var images = await _dbImageContext.Images
            .Include(i => i.Tags)
            .Where(i => i.Tags.Any(t => t.Name.Contains(query)))
            .ToListAsync();
        var prefearedSort = images.Select(i => i.Tags.Where(t=> t.Name == query).Count() > 0).ToList();
        if (prefearedSort != null)
        {
            return View("Index", prefearedSort);
        }
        return View("Index", images);
    }
}
