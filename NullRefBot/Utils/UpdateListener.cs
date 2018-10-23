using System;
using System.Collections.Generic;
using DSharpPlus.EventArgs;

namespace NullRefBot.Utils
{
	public static class UpdateListener
	{
		private const ulong TRAVIS_CHANNEL = 504110403467476994;
		public static void CheckMessage(MessageCreateEventArgs e)
		{
			if (e.Channel.Id != TRAVIS_CHANNEL)
				return;


		}
	}
}