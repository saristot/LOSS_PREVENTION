using LossPrevention.Application.DTO.Workspaces;

namespace LossPrevention.Application.Interfaces.Workspaces
{
    public interface IWorkspaceService
    {
        Task<List<WorkspaceDTO>> GetAllAsync(string? search = null);
        Task<WorkspaceDTO?> GetByIdAsync(string? id);
        Task<WorkspaceDTO> AddAsync(WorkspaceDTO dto);
        Task<WorkspaceDTO> UpdateAsync(string id, WorkspaceDTO dto);
        Task DeleteAsync(string id);
    }

}
