using System;

namespace PlayServices.DataModel
{
    public class GameDataInfo
    {
        public string GameId { get; set; }
        public uint CurrentIndex { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string IconUrl { get; set; }
    };
}
