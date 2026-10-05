using Microsoft.AspNetCore.Http;

namespace LossPrevention.Application.Handlers.Requests.Data
{
    public sealed class CreateTransactionRequest
    {
        public required List<IFormFile> Files { get; set; }
    }
}
