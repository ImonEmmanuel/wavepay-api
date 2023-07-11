using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaywaveAPIData.Model
{
    public class RefreshToken
    {
        [BsonId]
        public string ID;

        public int ClientId { get; set; }

        public string ClientEmail { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpireAt { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
