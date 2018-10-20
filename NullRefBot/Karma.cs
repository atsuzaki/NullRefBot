using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace NullRefBot {
	public class Karma {
		static readonly Regex detectThanksRegex = new Regex( @"(?:\s|^)(thanks|thank you)(?:\s|$)", RegexOptions.IgnoreCase );
		static readonly StringBuilder messageBuilder = new StringBuilder();

		public struct InvalidKarmaReciever {
			public DiscordUser dUser;
			public string reason;

			public InvalidKarmaReciever ( DiscordUser user, string reason ) {
				this.dUser = user;
				this.reason = reason;
			}
		}

		public static bool IsKarmaMessage ( DiscordMessage message ) {
			if( message.MentionedUsers.Count == 0 ) return false;
			return detectThanksRegex.IsMatch( message.Content );
		}

		public static bool GiveKarma ( DiscordChannel channel, DiscordUser author, IEnumerable<DiscordUser> recipients ) {
			var invalidUsers = new List<InvalidKarmaReciever>();
			var validUsers = new List<DiscordUser>();

			messageBuilder.Length = 0;

			foreach( var user in recipients ) {
				messageBuilder.Append( user ).Append( "," );

				Console.WriteLine( user.IsCurrent + " " + ( user == author ) );

				if( user.IsBot ) {
					if( !invalidUsers.Exists( i => i.dUser == user ) ) invalidUsers.Add( new InvalidKarmaReciever( user, "bots don't need karma." ) );
					continue;
				} else if( user == author ) {
					if( !invalidUsers.Exists( i => i.dUser == user ) ) invalidUsers.Add( new InvalidKarmaReciever( user, "you cannot give karma to yourself!" ) );
					continue;
				}
				if( !validUsers.Contains( user ) ) validUsers.Add( user );
			}

			Console.WriteLine( validUsers.Count );

			Bot.Instance.Client.DebugLogger.LogMessage( LogLevel.Info, "Karma", $"{author} tries giving karma to {messageBuilder.ToString()}", DateTime.Now );

			SendKarmaMessagesAsync( channel, author, validUsers, invalidUsers );

			return validUsers.Count > 0;
		}

		public static Task SendKarmaMessagesAsync ( DiscordChannel channel, DiscordUser author, IList<DiscordUser> karmaRecievers, IList<InvalidKarmaReciever> invalidUsers ) {
			return Task.Run( async () => {
				await channel.TriggerTypingAsync();

				await SendGaveKarmaMessageAsync( channel, author, karmaRecievers );
				await SendInvalidKarmaMessageAsync( channel, author, invalidUsers );
			} );
		}

		public static Task SendGaveKarmaMessageAsync ( DiscordChannel channel, DiscordUser author, IList<DiscordUser> karmaRecievers ) {
			if( karmaRecievers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.AppendFormat( "{0} gave karma to:\n", author.Username );

				foreach( var user in karmaRecievers ) {
					Console.WriteLine( user );
					messageBuilder.AppendFormat( "  **{0}**\n", user.Username );
				}

				Bot.Instance.Client.DebugLogger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}

		public static Task SendInvalidKarmaMessageAsync ( DiscordChannel channel, DiscordUser author, IList<InvalidKarmaReciever> invalidUsers ) {
			if( invalidUsers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.Append( "Could not give karma to the following users:\n" );

				foreach( var user in invalidUsers ) {
					messageBuilder.AppendFormat( "  **{0}**, because {1}\n", user.dUser.Username, user.reason );
				}

				Bot.Instance.Client.DebugLogger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}
	}
}
