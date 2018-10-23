using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace NullRefBot.RPG {
	[XmlRoot( "Encounters" )]
	public class XmlEncounters {
		[XmlElement( "Encounter" )]
		public XmlEncounter[] encounters { get; set; }


		public Encounter[] ToEncounters ( Encounter parent = null ) {
			var results = new List<Encounter>();

			for( int i = 0; i < encounters.Length; i++ ) {
				var enc = encounters[ i ];
				var res = enc.ToEncounter( parent );
				results.Add( res );
				if( enc.subEncounters != null ) {
					results.AddRange( enc.subEncounters.ToEncounters( res ) );
				}
			}

			return results.ToArray();
		}
	}

	public class XmlEncounterBody {
		[XmlElement( "Description" )]
		public string description { get; set; }
	}

	public class XmlEncounterHeader {
		[XmlElement( "Title" )]
		public string title { get; set; }
		[XmlElement( "Author" )]
		public string author { get; set; }
	}

	public class XmlEncounterOption {
		[XmlElement( "Display" )]
		public string display { get; set; }
		[XmlElement( "Description" )]
		public string description { get; set; }
		[XmlElement( "Action" )]
		public XmlEncounterAction action { get; set; }


		public EncounterOption ToEncounterOption ( int index, EncounterOptionDisplayType displayType ) {
			var option = new EncounterOption();
			option.description = description;

			switch( displayType ) {
				case EncounterOptionDisplayType.Numbers:
					option.displayIcon = EmojiUtils.emojiNumbers[ index ];
					break;
				case EncounterOptionDisplayType.Next:
					option.displayIcon = EmojiUtils.ArrowRight;
					break;
				case EncounterOptionDisplayType.Explicit:
					if( display == null ) throw new InvalidEncounterException( $"Explicit icon display was chosen but no icon was present in option {index}." );
					option.displayIcon = DiscordEmoji.FromName( Bot.Instance.Client, $":{display}:" );
					break;
			}

			option.action = action.ToEncounterAction();

			return option;
		}
	}

	public class XmlEncounterAction {
		[XmlElement( "Go" )]
		public XmlEncounterGo go { get; set; }
		[XmlElement( "Loot" )]
		public XmlEncounterLoot loot { get; set; }


		public EncounterAction ToEncounterAction () {
			var action = new EncounterAction();

			if( go != null ) {
				action.encounterId = go.id;
			}

			if( loot != null ) {
				action.loot = loot.ToEncounterLoot();
			}

			return action;
		}
	}

	public class XmlEncounterLoot {
		[XmlElement( "Experience" )]
		public int experience { get; set; }

		public EncounterLoot ToEncounterLoot () {
			var loot = new EncounterLoot( experience );

			return loot;
		}
	}

	public class XmlEncounterGo {
		[XmlAttribute( "id" )]
		public string id { get; set; }
	}

	public enum EncounterOptionDisplayType {
		[XmlEnum( "numbers" )]
		Numbers,
		[XmlEnum( "next" )]
		Next,
		[XmlEnum( "explicit" )]
		Explicit
	}

	public class XmlEncounterOptions {
		[XmlElement( "Option" )]
		public XmlEncounterOption[] options { get; set; }

		[XmlAttribute( "print" )]
		public bool print { get; set; } = true;
		[XmlAttribute( "display" )]
		public EncounterOptionDisplayType displayType { get; set; }


		public EncounterOption[] ToEncounterOptions () {
			var results = new EncounterOption[ options.Length ];

			for( int i = 0; i < options.Length; i++ ) {
				results[ i ] = options[ i ].ToEncounterOption( i, displayType );
			}

			return results;
		}
	}

	public class XmlEncounter {
		[XmlElement( "Header" )]
		public XmlEncounterHeader header { get; set; }
		[XmlElement( "Body" )]
		public XmlEncounterBody body { get; set; }
		[XmlElement( "Options" )]
		public XmlEncounterOptions options { get; set; }
		[XmlElement( "LootAction" )]
		public XmlEncounterAction lootAction { get; set; }
		[XmlElement( "Triggers" )]
		public XmlEncounterTriggers triggers { get; set; }

		[XmlAttribute]
		public string id { get; set; }

		[XmlElement( "SubEncounters" )]
		public XmlEncounters subEncounters { get; set; }

		public Encounter ToEncounter ( Encounter parent = null ) {
			var enc = new Encounter();
			if( id == null ) throw new InvalidEncounterException( "Encounter id is required." );
			if( parent != null ) enc.id = $"{parent.id}:{id}";
			else enc.id = id;

			if( header != null ) {
				enc.title = header.title;
				enc.author = header.author;
			}

			if( body != null ) {
				enc.text = body.description;
			}

			if( options != null ) {
				enc.listOptions = options.print;
				enc.options = options.ToEncounterOptions();
			} else {
				enc.flags |= EncounterFlags.IsExit;
			}

			if( lootAction != null ) {
				enc.lootAction = lootAction.ToEncounterAction();
			}

			if( triggers != null ) {
				enc.triggers = triggers.ToEncounterTriggers();
			}

			return enc;
		}
	}

	public class XmlEncounterTriggers {
		[XmlElement( "OnEnter" )]
		public XmlEncounterAction onEnter { get; set; }
		[XmlElement( "OnExit" )]
		public XmlEncounterAction onExit { get; set; }

		public EncounterTriggers ToEncounterTriggers () {
			var triggers = new EncounterTriggers();

			if( onEnter != null ) triggers.enter = onEnter.ToEncounterAction();
			if( onExit != null ) triggers.exit = onExit.ToEncounterAction();

			return triggers;
		}
	}

	public partial class EncounterManager {
		static Dictionary<string, Encounter> loadedEncounters = new Dictionary<string, Encounter>();

		public static void RegisterEncounter ( params Encounter[] toAdd ) {
			for( int i = 0; i < toAdd.Length; i++ ) {
				var enc = toAdd[ i ];
				loadedEncounters.Add( enc.id, enc );
			}
		}

		public static Encounter GetEncounter ( string encounterId ) {
			Encounter enc;

			loadedEncounters.TryGetValue( encounterId, out enc );

			return enc;
		}
	}

	public class EncounterImporter {
		static readonly XmlSerializer serializer = new XmlSerializer( typeof( XmlEncounters ) );

		public static void ImportFromXML ( string path ) {
			XmlEncounters encRes;

			using( var fs = File.OpenRead( path ) )
			using( var sr = new StreamReader( fs, new UTF8Encoding( true ) ) ) {
				encRes = (XmlEncounters)serializer.Deserialize( sr );
			}

			EncounterManager.RegisterEncounter( encRes.ToEncounters() );
		}
	}
}
