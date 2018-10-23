using RestSharp.Serializers;
using RestSharp.Deserializers;
using RestSharp.Authenticators;
using System;
using DSharpPlus.Entities;

namespace NullRefBot {
	public class Mute {
		[DeserializeAs( Name = "user_id" )]
		public ulong Id { get; set; }

	    [DeserializeAs(Name = "muted_at")]
	    public DateTime mutedAt { get; set; }
	    [DeserializeAs(Name = "muted_until")]
	    public DateTime mutedUntil { get; set; }
	    [DeserializeAs(Name = "createdAt")]
	    public DateTime createdAt { get; set; }
	    [DeserializeAs(Name = "updatedAt")]
	    public DateTime updatedAt { get; set; }
	}
}
