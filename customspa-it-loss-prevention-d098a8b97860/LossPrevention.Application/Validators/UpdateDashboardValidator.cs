using FluentValidation;
using MongoDB.Bson;

namespace LossPrevention.Application.Handlers.Requests.Dashboard
{
    public sealed class UpdateDashboardValidator : AbstractValidator<UpdateDashboardRequest>
    {
        public UpdateDashboardValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.WorkspaceId)
                .NotEmpty()
                .WithMessage("workspaceId is required")
                .Must(x => ObjectId.TryParse(x, out _))
                .WithMessage("workspaceId must be a valid ObjectId");
            RuleFor(x => x.TabId).NotEmpty();
        }
    }
}
