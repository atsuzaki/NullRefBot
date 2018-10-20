using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using RestSharp;

namespace NullRefBot {
	public class TaskFactory {
		public static Task Run ( Action action ) {
			return Task.Factory.StartNew( action ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					throw e;
				}
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
	}

	public class ExperienceManager {
		static readonly Regex detectThanksRegex = new Regex( @"(?:\s|^)(thanks|thank you)(?:\s|$)", RegexOptions.IgnoreCase );
		static readonly StringBuilder messageBuilder = new StringBuilder();

		public struct InvalidExpReciever {
			public DiscordUser dUser;
			public string reason;

			public InvalidExpReciever ( DiscordUser user, string reason ) {
				this.dUser = user;
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

		static Task PostExpAsync ( DiscordUser dUser, int amount ) {
			var req = new RestRequest();
			req.Resource = "/users/{discord_id}/karma/{amount}";
			req.AddParameter( "discord_id", dUser.Id, ParameterType.UrlSegment );
			req.AddParameter( "amount", amount, ParameterType.UrlSegment );
			req.Method = Method.PUT;
			req.RequestFormat = DataFormat.Json;

			Console.WriteLine( req.ToString() );

			return Task.Run( async () => {
				var user = await CreateUserAsync( dUser ); // temp

				if( user == null ) {
					throw new Exception( "User is null" );
				}

				user = await ExecuteAsync<User>( req );

				if( user == null ) {
					throw new Exception( "User is null" );
				}
			} );
		}

		static Task<User> CreateUserAsync ( DiscordUser dUser ) {
			var req = new RestRequest();
			req.Resource = "/users/{discord_id}";
			req.AddParameter( "discord_id", dUser.Id, ParameterType.UrlSegment );
			req.Method = Method.PUT;
			req.RequestFormat = DataFormat.Json;

			return Task.Run( () => ExecuteAsync<User>( req ) );
		}

		public static bool IsExpMessage ( DiscordMessage message ) {
			if( message.MentionedUsers.Count == 0 ) return false;
			return detectThanksRegex.IsMatch( message.Content );
		}

		public static bool GiveExp ( DiscordChannel channel, DiscordUser author, IEnumerable<DiscordUser> recipients, int amount = 1 ) {
			var invalidUsers = new List<InvalidExpReciever>();
			var validUsers = new List<DiscordUser>();

			messageBuilder.Length = 0;

			foreach( var user in recipients ) {
				messageBuilder.Append( user ).Append( "," );

				if( user.IsBot ) {
					if( !invalidUsers.Exists( i => i.dUser == user ) ) invalidUsers.Add( new InvalidExpReciever( user, "bots don't need experience." ) );
					continue;
				} else if( user == author ) {
					if( !invalidUsers.Exists( i => i.dUser == user ) ) invalidUsers.Add( new InvalidExpReciever( user, "you cannot give experience to yourself!" ) );
					continue;
				}
				if( !validUsers.Contains( user ) ) validUsers.Add( user );
			}

			Bot.Logger.LogMessage( LogLevel.Info, "Karma", $"{author} tries giving experience to {messageBuilder.ToString()}", DateTime.Now );

			TaskFactory.Run( async () => {
				var success = true;
				if( validUsers.Count > 0 ) {
					var tasks = new Task[ validUsers.Count ];
					for( var i = 0; i < tasks.Length; i++ ) {
						tasks[ i ] = PostExpAsync( validUsers[ i ], amount );
					}

					try {
						await Task.WhenAll( tasks );
					} catch( Exception e ) {
						Console.WriteLine( e.ToString() );
						success = false;
					}
				}

				if( success ) {
					await SendExpMessagesAsync( channel, author, validUsers, invalidUsers );
				} else {
					await channel.SendMessageAsync( "Failed to give experience due to an internal server error. Please try again later." );
				}
			} );

			return validUsers.Count > 0;
		}

		public static Task SendExpMessagesAsync ( DiscordChannel channel, DiscordUser author, IList<DiscordUser> experienceRecievers, IList<InvalidExpReciever> invalidUsers ) {
			return Task.Run( async () => {
				await channel.TriggerTypingAsync();

				await SendGaveExpMessageAsync( channel, author, experienceRecievers );
				await SendInvalidExpMessageAsync( channel, author, invalidUsers );
			} );
		}

		public static Task SendGaveExpMessageAsync ( DiscordChannel channel, DiscordUser author, IList<DiscordUser> experienceRecievers ) {
			if( experienceRecievers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.AppendFormat( "{0} gave experience to:\n", author.Username );

				foreach( var user in experienceRecievers ) {
					messageBuilder.AppendFormat( "  **{0}**\n", user.Username );
				}

				Bot.Logger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}

		public static Task SendInvalidExpMessageAsync ( DiscordChannel channel, DiscordUser author, IList<InvalidExpReciever> invalidUsers ) {
			if( invalidUsers.Count == 0 ) return Task.CompletedTask;

			return Task.Run( async () => {
				messageBuilder.Length = 0;
				messageBuilder.Append( "Could not give experience to the following users:\n" );

				foreach( var user in invalidUsers ) {
					messageBuilder.AppendFormat( "  **{0}**, because {1}\n", user.dUser.Username, user.reason );
				}

				Bot.Instance.Client.DebugLogger.LogMessage( LogLevel.Info, "Karma", messageBuilder.ToString(), DateTime.Now );

				await channel.SendMessageAsync( messageBuilder.ToString() );
			} );
		}
	}
}
