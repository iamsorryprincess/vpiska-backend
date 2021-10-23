using System;

namespace Vpiska.Domain.EventAggregate
{
    public sealed class UserInfo
    {
        public Guid Id { get; }

        public string Name { get; }

        public string ImageId { get; }

        public UserInfo(Guid id, string name, string imageId)
        {
            Id = id;
            Name = name;
            ImageId = imageId;
        }
    }
}