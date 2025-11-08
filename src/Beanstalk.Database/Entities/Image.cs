using System;

namespace Beanstalk.Database.Entities;

public class Image
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    public byte[]? ThumbnailData { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public UserProfile? Profile { get; set; }
}

