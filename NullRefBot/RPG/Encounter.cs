using System;
using System.Text;
using DSharpPlus.Entities;

namespace NullRefBot.RPG {
	[Flags]
	public enum EncounterFlags {
		IsExit = 0x1
	}

	public class Encounter {
		const string claimLootString = "You found some loot! Be sure to claim it!";

		public string id;

		public string title;
		public string text;
		public string author;

		public EncounterFlags flags;

		public bool listOptions;

		public EncounterOption[] options;
		public EncounterAction lootAction;
		public EncounterTriggers triggers;

		public void OnEnter ( DiscordChannel channel, DiscordUser user ) {
			EncounterManager.StartActiveEncounter( this, channel, user );
			if( triggers != null ) triggers.OnEnter( channel, user );
		}

		public void OnExit ( DiscordChannel channel, DiscordUser user ) {
			EncounterManager.FinishActiveEncounter( user );
			if( triggers != null ) triggers.OnExit( channel, user );
		}

		public DiscordEmbed ToEmbed ( DiscordUser owner ) {
			var builder = new DiscordEmbedBuilder();

			builder.Title = title;
			builder.Footer = new DiscordEmbedBuilder.EmbedFooter();
			builder.Author = DiscordEmbedUtils.MakeUserAuther( owner );
			//if( owner != null ) builder.Footer.Text = $"This encounter can only be completed by {owner.Username}";

			var descriptionBuilder = new StringBuilder();

			descriptionBuilder.Append( text );

			if( listOptions ) {
				descriptionBuilder.Append( "\n\n" );
				descriptionBuilder.Append( CreateOptionsString() );
			}

			if( lootAction != null ) {
				builder.Footer.Text = claimLootString;
			}

			builder.Description = descriptionBuilder.ToString();

			return builder;
		}

		private string CreateOptionsString () {
			var builder = new StringBuilder();

			for( int i = 0; i < options.Length; i++ ) {
				var option = options[ i ];

				builder.Append( option.displayIcon.GetDiscordName() ).Append( " **" ).Append( option.description ).Append( "**" );
				builder.Append( "\n" );
			}

			return builder.ToString();
		}

		public EncounterOption GetOptionFromEmoji ( DiscordEmoji emoji ) {
			if( options == null ) return null;
			for( int i = 0; i < options.Length; i++ ) {
				var option = options[ i ];


				if( option.displayIcon == null ) {
					if( EmojiUtils.emojiNumbers[ i ] == emoji ) {
						return option;
					}
				} else if( option.displayIcon == emoji ) {
					return option;
				}
			}

			return null;
		}
	}
}
