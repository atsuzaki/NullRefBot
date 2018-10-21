using RestSharp.Serializers;
using RestSharp.Deserializers;
using RestSharp.Authenticators;
using System;
using DSharpPlus.Entities;

namespace NullRefBot {
	public class Member {
		[DeserializeAs( Name = "discord_id" )]
		public ulong Id { get; set; }
		[DeserializeAs( Name = "experience" )]
		public int Experience { get; set; }

		public DiscordUser dMember { get; set; }
	}
}
