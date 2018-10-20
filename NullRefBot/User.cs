using RestSharp.Serializers;
using RestSharp.Deserializers;
using RestSharp.Authenticators;

namespace NullRefBot {
	public class User {
		[DeserializeAs( Name = "discord_id" )]
		public ulong Id { get; set; }
		[DeserializeAs( Name = "experience" )]
		public int Experience { get; set; }
	}
}
