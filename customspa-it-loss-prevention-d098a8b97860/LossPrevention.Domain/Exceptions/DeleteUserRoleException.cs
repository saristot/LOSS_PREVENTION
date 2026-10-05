namespace LossPrevention.Domain.Exceptions
{
    public sealed class DeleteUserRoleException : Exception
    {
        public DeleteUserRoleException(string? message) : base(message)
        {
        }
    }
}
