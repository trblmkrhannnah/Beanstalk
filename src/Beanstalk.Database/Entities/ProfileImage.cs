using System;

namespace Beanstalk.Database.Entities;

public class ProfileImage
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public UserProfile? Profile { get; set; }
}