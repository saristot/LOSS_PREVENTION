using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LossPrevention.Application.Handlers.Requests.User.Users
{
    public sealed class ForgotPasswordRequest
    {
        public required string UsernameOrEmail { get; set; }
    }
}
