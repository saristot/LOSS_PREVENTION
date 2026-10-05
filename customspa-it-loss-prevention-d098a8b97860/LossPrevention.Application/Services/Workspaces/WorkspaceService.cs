using LossPrevention.Application.DTO.Workspaces;
using LossPrevention.Application.Entities.Workspaces;
using LossPrevention.Application.Helpers;
using LossPrevention.Application.Interfaces.Workspaces;
using LossPrevention.Domain.Entities.Workspaces;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Workspaces;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IMongoRepository<Workspace> _workspaceRepository;

    public WorkspaceService(IMongoRepository<Workspace> workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<List<WorkspaceDTO>> GetAllAsync(string? search = null)
    {
        List<Workspace> entities;

        if (!string.IsNullOrWhiteSpace(search))
            entities = await _workspaceRepository.FindManyAsync(w => w.Name.ToLower().Contains(search.ToLower()));
        else
            entities = await _workspaceRepository.GetAllAsync();

        return entities.Select(ToDto).ToList();
    }

    public async Task<WorkspaceDTO?> GetByIdAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var filter = Builders<Workspace>.Filter.Eq(x => x.Id, id);
        var entity = await _workspaceRepository.FindOneAsync(filter);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<WorkspaceDTO> AddAsync(WorkspaceDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Workspace name is required.");

        var entity = ToEntity(dto);
        entity.Id = ObjectId.GenerateNewId().ToString();
        await _workspaceRepository.InsertOneAsync(entity);
        return ToDto(entity);
    }

    public async Task<WorkspaceDTO> UpdateAsync(string id, WorkspaceDTO dto)
    {
        if (!ObjectId.TryParse(id, out _))
            throw new ArgumentException("Invalid Workspace ID.");

        var entity = ToEntity(dto);
        entity.Id = id;

        var filter = Builders<Workspace>.Filter.Eq(x => x.Id, id);
        var update = Builders<Workspace>.Update
            .Set(x => x.Name, entity.Name)
            .Set(x => x.Description, entity.Description)
            .Set(x => x.Tabs, entity.Tabs);

        var result = await _workspaceRepository.UpdateOneAsync(filter, update);
        if (result.MatchedCount == 0)
            throw new KeyNotFoundException("Workspace not found.");

        return ToDto(entity);
    }

    public async Task DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            throw new ArgumentException("Invalid Workspace ID.");

        var filter = Builders<Workspace>.Filter.Eq(x => x.Id, id);
        var result = await _workspaceRepository.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
            throw new KeyNotFoundException("Workspace not found.");
    }

    // ── Mappers ──────────────────────────────────────────────────────────────

    private static WorkspaceDTO ToDto(Workspace w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Description = w.Description,
        Tabs = w.Tabs?.Select(TabToDto).ToList() ?? []
    };

    private static TabDTO TabToDto(Tab t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        SelectedFields = t.SelectedFields?.Select(FieldToDto).ToList() ?? [],
        GroupByField = t.GroupByField ?? string.Empty,
        Query = t.Query is null ? null : QueryToDto(t.Query),
        DesignMode = t.DesignMode
    };

    private static FieldDTO FieldToDto(Field f) => new()
    {
        Name = f.Name ?? string.Empty,
        Alias = f.Alias ?? string.Empty,
        DataType = f.DataType ?? string.Empty,
        GroupBy = f.GroupBy,
        Aggregation = f.Aggregation ?? string.Empty,
        IsCalculated = f.IsCalculated,
        Expression = f.Expression ?? string.Empty,
        Prefix = f.Prefix ?? string.Empty,
        Suffix = f.Suffix ?? string.Empty,
        Visible = f.Visible
    };

    private static QueryDTO QueryToDto(Query q) => new()
    {
        Id = q.Id ?? string.Empty,
        Type = q.Type ?? string.Empty,
        Conditions = q.Conditions?.Select(c => new ConditionDTO
        {
            Field = c.Field ?? string.Empty,
            Operator = c.Operator ?? string.Empty,
            Value = c.Value
        }).ToList() ?? []
    };

    private static Workspace ToEntity(WorkspaceDTO dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Tabs = dto.Tabs?.Select(TabToEntity).ToList() ?? []
    };

    private static Tab TabToEntity(TabDTO dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Description = dto.Description,
        SelectedFields = dto.SelectedFields?.Select(FieldToEntity).ToList() ?? [],
        GroupByField = dto.GroupByField,
        Query = dto.Query is not null ? QueryToEntity(dto.Query) : new Query(),
        DesignMode = dto.DesignMode
    };

    private static Field FieldToEntity(FieldDTO dto) => new()
    {
        Name = dto.Name,
        Alias = dto.Alias,
        DataType = dto.DataType,
        GroupBy = dto.GroupBy,
        Aggregation = dto.Aggregation,
        IsCalculated = dto.IsCalculated,
        Expression = dto.Expression,
        Prefix = dto.Prefix,
        Suffix = dto.Suffix,
        Visible = dto.Visible
    };

    private static Query QueryToEntity(QueryDTO dto) => new()
    {
        Id = dto.Id,
        Type = dto.Type,
        Conditions = dto.Conditions?.Select(c => new Condition
        {
            Field = c.Field,
            Operator = c.Operator,
            Value = JsonHelper.ConvertJsonElement(c.Value)
        }).ToList() ?? []
    };
}
