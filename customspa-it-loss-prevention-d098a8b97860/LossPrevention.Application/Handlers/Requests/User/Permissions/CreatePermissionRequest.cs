using MongoDB.Bson;

namespace LossPrevention.API.Handlers.Requests.User.Permissions
{
    public sealed class CreatePermissionRequest
{
    public required string PermissionName { get; set; }
    public required string PermissionText { get; set; }
    public required string Description { get; set; }

}
}
