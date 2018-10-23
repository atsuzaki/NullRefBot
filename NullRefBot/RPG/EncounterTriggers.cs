using DSharpPlus.Entities;
using System;

namespace NullRefBot.RPG {
	public class EncounterTriggers {
		internal EncounterAction exit;
		internal EncounterAction enter;

		public void OnEnter ( DiscordChannel channel, DiscordUser user ) {
			if( enter != null ) enter.Execute( channel, user );
		}

		public void OnExit ( DiscordChannel channel, DiscordUser user ) {
			if( exit != null ) exit.Execute( channel, user );
		}
	}
}
