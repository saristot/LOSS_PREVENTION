using LossPrevention.Application.Handlers.Requests.Groups;
using LossPrevention.Application.Handlers.Responses.Groups;
using LossPrevention.Domain.Groups;
using MongoDB.Bson;

namespace LossPrevention.Application.Mappings
{
    public static class GroupMapping
    {
        public static GroupResponse ToResponse(this GroupDocument g) => new()
        {
            Id = g.Id.ToString(),
            Name = g.Name,
            Description = g.Description,
            Members = g.Members?.ConvertAll(m => m.ToString()) ?? new(),
            CreatedAtUtc = g.CreatedAtUtc.ToString("o"),
            UpdatedAtUtc = g.UpdatedAtUtc.ToString("o")
        };

        public static GroupDocument ToDocument(this CreateGroupRequest r, DateTime nowUtc)
        {
            return new()
            {
                Id = ObjectId.GenerateNewId(),
                Name = r.Name?.Trim() ?? "",
                Description = r.Description?.Trim() ?? "",
                Members = r.Members?.ConvertAll(m => ObjectId.Parse(m)) ?? new(),
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            };
        }

        public static void Apply(this GroupDocument g, UpdateGroupRequest r, DateTime nowUtc)
        {
            g.Name = r.Name?.Trim() ?? "";
            g.Description = r.Description?.Trim() ?? "";
            g.Members = r.Members?.ConvertAll(m => ObjectId.Parse(m)) ?? new();
            g.UpdatedAtUtc = nowUtc;
        }
    }
}
