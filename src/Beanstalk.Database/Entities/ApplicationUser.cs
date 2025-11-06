using System;
using Microsoft.AspNetCore.Identity;

namespace Beanstalk.Database.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsEnabled { get; set; } = true;
}