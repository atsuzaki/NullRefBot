using DSharpPlus.Entities;
using static NullRefBot.Bot;

namespace NullRefBot.RPG {
	public class EncounterOption {
		public string description;
		public DiscordEmoji displayIcon;

		public EncounterAction action;

		public EncounterOption () { }

		public EncounterOption ( string description, EncounterAction action, string emojiOverrideName = null ) {
			this.description = description;
			this.action = action;
			if( emojiOverrideName != null ) {
				displayIcon = DiscordEmoji.FromName( Instance.Client, emojiOverrideName );
			}
		}

		public EncounterOption ( string description, EncounterAction action, DiscordEmoji emojiOverride ) {
			this.description = description;
			this.action = action;
			this.displayIcon = emojiOverride;
		}
	}
}
