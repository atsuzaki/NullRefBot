using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using RestSharp;

namespace NullRefBot {
	public class TaskFactory {
		public static Task Run ( Action action ) {
			return Task.Factory.StartNew( action ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
		public static Task<T> Run<T> ( Func<T> func ) {
			return Task.Factory.StartNew( func ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
				return default(T);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
		public static Task<T> Run<T> ( Func<Task<T>> func ) {
			return Task.Factory.StartNew( func ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
				return default(T);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
	}

	public class ExperienceCommands : BaseCommandModule {
		[Command("xp")]
		public async Task DisplayExp( CommandContext c, DiscordMember member ) {
			await c.TriggerTypingAsync();

			var exp = await ExperienceManager.GetExpAsync( member );

			await c.RespondAsync( string.Format( "**{0}** has {1} experience.", member.Username, exp ) );
		}
	}

	public class ExperienceManager {
		static readonly Regex detectThanksRegex = new Regex( @"(?:\s|^)(thanks|thank you)(?:\s|$)", RegexOptions.IgnoreCase );
		static readonly StringBuilder messageBuilder = new StringBuilder();

		public struct InvalidExpReciever {
			public DiscordMember dMember;
			public string reason;

			public InvalidExpReciever ( DiscordMember member, string reason ) {
				this.dMember = member;
				this.reason = reason;
			}
		}

		static async Task<T> ExecuteAsync<T> ( RestRequest req ) where T : new() {
			var client = new RestClient();
			client.BaseUrl = new Uri( Bot.Instance.Config.DatabaseIP );

			var response = await client.ExecuteTaskAsync<T>( req );

			if( response.ErrorException != null ) {
				const string message = "Error retrieving response.  Check inner details for more info.";
				var twilioException = new ApplicationException( message, response.ErrorException );
				throw twilioException;
			}

			return response.Data;
		}

		static Task<Member> PostExpAsync ( DiscordMember dMember, int amount ) {
			var req = new RestRequest();
			req.Resource = "/users/{discord_id}/addexperience";
			req.RequestFormat = DataFormat.Json;
			req.Method = Method.PATCH;
			req.Timeout = 5 * 1000;
			
			req.AddParameter( "discord_id", dMember.Id, ParameterType.UrlSegment );

			req.AddBody( new { experience = amount } );

			Console.WriteLine( req.ToString() );

			return Task.Run( async () => {
				var member = await ExecuteAsync<Member>( req );

				if( member == null ) {
					throw new Exception( "User is null" );
				}

				member.dMember = dMember;

				return member;
			} );
		}

		static Task<Member> GetUserAsync ( DiscordMember dMember ) {
			var req = new RestRequest();
			req.Resource = "/users/{discord_id}";
			req.AddParameter( "discord_id", dMember.Id, ParameterType.UrlSegment );

			req.Method = Method.GET;
			req.RequestFormat = DataFormat.Json;
			req.Timeout = 5 * 1000;

			return Task.Run( () => ExecuteAsync<Member>( req ) );
		}

		public static bool IsExpMessage ( DiscordMessage message ) {
			if( message.MentionedUsers.Count == 0 ) return false;
			return detectThanksRegex.IsMatch( message.Content );
		}

		static readonly DiscordMember[] singleUserArr = new DiscordMember[ 1 ];

		public static Task<int> GetExpAsync ( DiscordMember dMember ) {
			return Task.Run( async () => {
				var member = await GetUserAsync( dMember );

				return member.Experience;
			} );
		}

		public static bool GiveExp ( DiscordChannel channel, DiscordMember author, IEnumerable<DiscordMember> recipients, int amount = 1 ) {
			var invalidUsers = new List<InvalidExpReciever>();
			var validUsers = new List<DiscordMember>();

			messageBuilder.Length = 0;

			foreach( var member in recipients ) {
				messageBuilder.Append( member ).Append( "," );

				if( member.IsBot ) {
					if( !invalidUsers.Exists( i => i.dMember == member ) ) invalidUsers.Add( new InvalidExpReciever( member, "bots don't need experience." ) );
					continue;
				} else if( member == author ) {
					if( !invalidUsers.Exists( i => i.dMember == member ) ) invalidUsers.Add( new InvalidExpReciever( member, "you cannot give experience to yourself!" ) );
					continue;
				}
				if( !validUsers.Contains( member ) ) validUsers.Add( member );
			}

			Bot.Logger.LogMessage( LogLevel.Info, "Karma", $"{author} tries giving experience to {messageBuilder.ToString()}", DateTime.Now );

			TaskFactory.Run( async () => {
				var success = true;
				Member[] retrievedUsers = null;
				if( validUsers.Count > 0 ) {
					var tasks = new Task<Member>[ validUsers.Count ];
					for( var i = 0; i < tasks.Length; i++ ) {
						tasks[ i ] = PostExpAsync( validUsers[ i ], amount );
					}

					try {
						retrievedUsers = await Task.WhenAll( tasks );
					} catch( Exception e ) {
						Console.WriteLine( e.ToString() );
						success = false;
					}
				}

				if( success ) {
					await SendExpMessagesAsync( channel, author, retrievedUsers, invalidUsers );
				} else {
					await channel.SendMessageAsync( "Failed to give experience due to an internal server error. Please try again later." );
				}
			} );

			return validUsers.Count > 0;
		}

		public static Task SendExpMessagesAsync ( DiscordChannel channel, DiscordMember author, IList<Member> experienceRecievers, IList<InvalidExpReciever> invalidUsers ) {
			return Task.Run( async () => {
				await channel.TriggerTypingAsync();

				await SendGaveExpMessageAsync( channel, author, experienceRecievers );
				await SendInvalidExpMessageAsync( channel, author, invalidUsers );
			} );
		}

		public static Task SendGetExpMessageAsync ( DiscordChannel channel, IList<Member> users ) {
			if( users.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				foreach( var member in users ) {
					messageBuilder.AppendFormat( "  **{0}**\n", member.Experience );
				}

				Bot.Logger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}

		public static Task SendGaveExpMessageAsync ( DiscordChannel channel, DiscordMember author, IList<Member> experienceRecievers ) {
			if( experienceRecievers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.AppendFormat( "{0} gave experience to:\n", author.Username );

				foreach( var member in experienceRecievers ) {
					messageBuilder.AppendFormat( "  **{0}**, {1}\n", member.dMember.Username, member.Experience );
				}

				Bot.Logger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}

		public static Task SendInvalidExpMessageAsync ( DiscordChannel channel, DiscordMember author, IList<InvalidExpReciever> invalidUsers ) {
			if( invalidUsers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.Append( "Could not give experience to the following users:\n" );

				foreach( var member in invalidUsers ) {
					messageBuilder.AppendFormat( "  **{0}**, because {1}\n", member.dMember.Username, member.reason );
				}

				Bot.Instance.Client.DebugLogger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}
	}
}
