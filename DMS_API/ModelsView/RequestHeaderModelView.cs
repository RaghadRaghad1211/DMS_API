using Microsoft.AspNetCore.Mvc;

namespace DMS_API.ModelsView
{
    public class RequestHeaderModelView
    {
        [FromHeader]
        public string? Token { get; set; }
        [FromHeader]
        public string? Lang { get; set; } = "Ar";
    }
}