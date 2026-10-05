using FluentValidation;
using LossPrevention.Application.Dashboards.DTO;
using LossPrevention.Application.Handlers.Requests.Dashboard;

namespace LossPrevention.Application.Dashboards.Validation
{
    public sealed class CreateDashboardValidator : AbstractValidator<CreateDashboardRequest>
    {
        public CreateDashboardValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.TabId).NotEmpty();
            RuleForEach(x => x.Blocks).SetValidator(new DashboardBlockValidator());
        }
    }

    public sealed class UpdateDashboardValidator : AbstractValidator<UpdateDashboardRequest>
    {
        public UpdateDashboardValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.TabId).NotEmpty();
            RuleForEach(x => x.Blocks).SetValidator(new DashboardBlockValidator());
        }
    }

    public sealed class DashboardBlockValidator : AbstractValidator<DashboardBlockDto>
    {
        public DashboardBlockValidator()
        {
            RuleFor(b => b.i).NotEmpty();
            RuleFor(b => b.w).GreaterThan(0);
            RuleFor(b => b.h).GreaterThan(0);
            RuleFor(b => b.type).NotEmpty();
        }
    }
}
