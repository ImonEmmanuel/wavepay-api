using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaywaveAPIData.Model
{
    public class Account
    {
        [BsonId]
        public string ID;
        [JsonIgnore]
        public string ClientID; // This is the Id Key of the account
        public string AccountName;
        public decimal? PendingBalance; // for Settlement
        public decimal? AvailableBalance; //Balance The Customer have that can be withdarwal at that time
        public DateTime? LastRefreshed;
    }
}
