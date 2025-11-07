using System;

namespace Beanstalk.Database.Entities;

public class InviteLink
{
    public Guid Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    
    public Guid CreatedByUserId { get; set; }
    
    public ApplicationUser CreatedBy { get; set; } = null!;
    
    public DateTime CreatedUtc { get; set; }
    
    public int? MaxUses { get; set; }
    
    public int CurrentUses { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpiresUtc { get; set; }
}

