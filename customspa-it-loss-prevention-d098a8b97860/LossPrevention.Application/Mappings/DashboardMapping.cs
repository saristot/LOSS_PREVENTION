using LossPrevention.Application.Dashboards.DTO;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Application.Handlers.Responses.Dashboard;
using LossPrevention.Domain.Dashboards;
using MongoDB.Bson;

namespace LossPrevention.Application.Mappings
{
    public static class DashboardMapping
    {
        public static DashboardResponse ToResponse(this DashboardDocument d) => new()
        {
            Id = d.Id.ToString(), // Convert ObjectId to string using .ToString()
            Name = d.Name,
            WorkspaceId = d.WorkspaceId.ToString(), // Convert ObjectId to string using .ToString()
            TabId = d.TabId,
            Blocks = d.Blocks?.ConvertAll(b => new DashboardBlockDto
            {
                i = b.I,
                x = b.X,
                y = b.Y,
                w = b.W,
                h = b.H,
                type = b.Type,
                data = b.Data?.ToDictionary()
            }) ?? new(),
            CreatedAtUtc = d.CreatedAtUtc.ToString("o"),
            UpdatedAtUtc = d.UpdatedAtUtc.ToString("o")
        };

        public static DashboardDocument ToDocument(this CreateDashboardRequest r, DateTime nowUtc)
        {
            return new()
            {
                Id = ObjectId.GenerateNewId(), // Fix: Use ObjectId directly instead of converting to string
                Name = r.Name?.Trim() ?? "",
                WorkspaceId = ObjectId.Parse(r.WorkspaceId), // Fix: Parse string to ObjectId
                TabId = r.TabId, // keep plain string (e.g. "tab-1")
                Blocks = r.Blocks?.ConvertAll(b => new DashboardBlock
                {
                    I = b.i,
                    X = b.x,
                    Y = b.y,
                    W = Math.Max(1, b.w),
                    H = Math.Max(1, b.h),
                    Type = string.IsNullOrWhiteSpace(b.type) ? "text" : b.type,
                    Data = CreateBsonDocument(b.data)
                }) ?? new(),
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            };
        }

        public static void Apply(this DashboardDocument d, UpdateDashboardRequest r, DateTime nowUtc)
        {
            d.Name = r.Name?.Trim() ?? "";
            d.WorkspaceId = ObjectId.Parse(r.WorkspaceId);
            d.TabId = r.TabId;
            d.Blocks = r.Blocks?.ConvertAll(b => new DashboardBlock
            {
                I = b.i,
                X = b.x,
                Y = b.y,
                W = Math.Max(1, b.w),
                H = Math.Max(1, b.h),
                Type = string.IsNullOrWhiteSpace(b.type) ? "text" : b.type,
                Data = b.data is null ? new BsonDocument() : BsonDocument.Create(b.data)
            }) ?? new();
            d.UpdatedAtUtc = nowUtc;
        }

        private static Dictionary<string, object> ToDictionary(this BsonDocument doc)
        {
            var dict = new Dictionary<string, object>();
            foreach (var el in doc.Elements)
                dict[el.Name] = BsonTypeMapper.MapToDotNetValue(el.Value);
            return dict;
        }

        private static BsonDocument CreateBsonDocument(Dictionary<string, object>? data)
        {
            if (data is null || data.Count == 0)
                return new BsonDocument();

            try
            {
                return BsonDocument.Create(data);
            }
            catch
            {
                return new BsonDocument();
            }
        }
    }
}
