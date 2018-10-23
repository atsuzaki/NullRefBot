using DSharpPlus.Entities;

namespace NullRefBot.RPG {
	public class EncounterAction {
		public string encounterId;
		public Encounter encounter;
		public EncounterLoot loot;

		internal void Execute ( DiscordChannel channel, DiscordUser user ) {
			if( encounterId != null ) {
				EncounterManager.SpawnEncounter( channel, user, encounterId );
				return;
			}

			if( loot != null ) {
				ExperienceManager.GiveExpAndNotifyAsync( channel, user, loot.experience );
			}
		}
	}
}
