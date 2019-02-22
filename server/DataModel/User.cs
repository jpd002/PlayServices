using System;

namespace PlayServices.DataModel
{
    public class User
    {
        public Guid Id { get; set; } = Guid.Empty;
        public uint PatreonId { get; set; }
    };
}
