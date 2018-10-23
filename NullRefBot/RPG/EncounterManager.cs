using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using static NullRefBot.Bot;

namespace NullRefBot.RPG {
	public partial class EncounterManager {

		public static Dictionary<string, ActiveEncounter> activeEncounters = new Dictionary<string, ActiveEncounter>();

		public class ActiveEncounter {
			public DateTime startTime;
			public DiscordUser user;
			public DiscordChannel channel;
			public Encounter encounter;

			public bool lootCollected;

			public ActiveEncounter ( Encounter encounter, DiscordChannel channel, DiscordUser user ) {
				this.user = user;
				this.channel = channel;
				this.encounter = encounter;
				this.startTime = DateTime.Now;
			}
		}

		public static bool TrySpawnEncounterForUser ( DiscordMember member, out string encounterId ) {
			ActiveEncounter existing;
			if( activeEncounters.TryGetValue( member.Id.ToString(), out existing ) ) {
				encounterId = existing.encounter.id;
				return false;
			}

			encounterId = "test_encounter";
			return true;
//			var subEncounter = new Encounter();
//			subEncounter.title = "Suddenly and Without Warning";

//			subEncounter.text =
//@"As soon as you push the **white** button, the floor silently opens up beneath you! You fall in a rather silly manner, your arms and legs flailing comically. 

//After falling for what seems a bit longer than necessary, you land sprawling on a giant stack of pillows. After taking a moment to make sure you're still in one piece, you pick yourself up and dust yourself off, no worse for wear besides your tarnished dignity.

//Your new surroundings are what appears to be a tall concrete room. You can see the chute you fell out of above you, and a single door on the wall.

//There's more than one doorframe, mind you, but all the others have nothing but concrete behind them. You have the feeling that the other doors were cut for budgetary reasons. You steel yourself and step through the available door.";

//			subEncounter.options = new [] {
//				new EncounterOption( "Continue", new EncounterResult( "Hmm...", "You find yourself blinking in sudden harsh sunlight. It seems this door led to the exit of the test encounter. You have a feeling that the rest of the encounter was cut for budgetary reasons. 'Bloody beauracrats,' you mutter to yourself." ), EmojiUtils.ArrowRight ),
//			};
//			subEncounter.listOptions = true;




//			var testEncounter = new Encounter();

//			testEncounter.title = "A Test Encounter";
//			testEncounter.text = 
//@"You seem to have stumbled into a test encounter. You're not entirely sure what that means, but you do know that you seem to be in an entirely white room with no doors or windows.

//It's well lit, but you're not sure from where. The only other objects in the room are three ornate pedastals, each topped with a large cartoony button.

//* The left button is **black**
//* The middle button is **white**
//* And the right button is **red**.

//You get the feeling that your only options involve pressing one of the buttons. Which button do you choose?";

//			testEncounter.author = "The Narrator";

//			testEncounter.options = new[] {
//				new EncounterOption( "Press the black button", new EncounterResult( "Hmm...", "You push the **black** button, and are suddenly and violently teleported out of the test encounter. How anticlimatic." ), ":black_circle:" ),
//				new EncounterOption( "Press the white button", new EncounterResult( subEncounter ), ":white_circle:" ),
//				new EncounterOption( "Press the red button",  new EncounterResult( new EncounterLoot( 2 ), title: null, text: "As soon as you push the **red** button, a warm sensation wells up inside you. You can feel yourself getting more powerful! A reddish mist faintly wafts off your skin. After a moment, both the mist and sensation dissipate, but you still feel stronger." ), ":red_circle:" ),
//			};

//			testEncounter.listOptions = false;

//			return testEncounter;
		}

		internal static void FinishActiveEncounter ( DiscordUser user ) {
			activeEncounters.Remove( user.Id.ToString() );
		}

		internal static void StartActiveEncounter ( Encounter encounter, DiscordChannel channel, DiscordUser user ) {
			var active = new ActiveEncounter( encounter, channel, user );
			activeEncounters.Add( user.Id.ToString(), active );
		}

		private static ActiveEncounter GetActiveEncounter ( DiscordUser user ) {
			ActiveEncounter active;
			activeEncounters.TryGetValue( user.Id.ToString(), out active );
			return active;
		}

		public static Task SpawnEncounter ( DiscordChannel channel, DiscordUser user, string encounterId ) {
			var encounter = EncounterManager.GetEncounter( encounterId );
			return Task.Run( async () => {
				var message = await channel.SendMessageAsync( embed: encounter.ToEmbed( user ) );

				encounter.OnEnter( channel, user );

				Logger.LogMessage( LogLevel.Info, "Encounter", $"{user} is beginning encounter {encounter.id}.", DateTime.Now );

				var trigger = Instance.AddReactionTrigger( message, user, ( u, selection ) => {
					var activeEncounter = EncounterManager.GetActiveEncounter( user );

					var selectedOption = encounter.GetOptionFromEmoji( selection );
					if( selectedOption != null ) {
						Instance.RemoveReactionTrigger( message );
						message.DeleteAllReactionsAsync();
						if( selectedOption != null ) selectedOption.action.Execute( channel, user );

						encounter.OnExit( channel, user );
					} else if( !activeEncounter.lootCollected && encounter.lootAction != null ) {
						activeEncounter.lootCollected = true;
						if( encounter.flags.HasFlag( EncounterFlags.IsExit ) ) {
							message.DeleteAllReactionsAsync();
							encounter.OnExit( channel, user );
						} else {
							Task.Run( async () => {
								var users = await message.GetReactionsAsync( EmojiUtils.moneybag );
								for( int i = 0; i < users.Count; i++ ) {
									var task = message.DeleteReactionAsync( EmojiUtils.moneybag, users[ i ] );
								}
							} );
						}
						encounter.lootAction.Execute( channel, user );
					}
				} );

				if( encounter.flags.HasFlag( EncounterFlags.IsExit ) ) {
					if( encounter.lootAction == null ) {
						encounter.OnExit( channel, user );
					}
				} else {

					if( encounter.options != null ) {
						var tasks = new Task[ encounter.options.Length ];
						for( int i = 0; i < encounter.options.Length; i++ ) {
							var option = encounter.options[ i ];

							DiscordEmoji emoji;
							if( option.displayIcon != null ) emoji = option.displayIcon;
							else emoji = EmojiUtils.emojiNumbers[ i ];

							await message.CreateReactionAsync( option.displayIcon );
						}
					}
				}

				if( encounter.lootAction != null ) {
					await message.CreateReactionAsync( EmojiUtils.moneybag );
				}
			} );
		}
	}
}
