using System;
using System.Runtime.Serialization;

namespace NullRefBot.RPG {
	[Serializable]
	internal class InvalidEncounterException : Exception {
		public InvalidEncounterException () {
		}

		public InvalidEncounterException ( string message ) : base( message ) {
		}

		public InvalidEncounterException ( string message, Exception innerException ) : base( message, innerException ) {
		}

		protected InvalidEncounterException ( SerializationInfo info, StreamingContext context ) : base( info, context ) {
		}
	}
}
