using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AzureImageSearch.Models;

public class Image
{
    public int Id { get; set; }

    public string Url { get; set; }

    public List<Tag> Tags { get; set; }
}
