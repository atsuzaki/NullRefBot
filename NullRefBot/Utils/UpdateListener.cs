using System;
using System.Collections.Generic;
using System.Diagnostics;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace NullRefBot.Utils
{
	public static class UpdateListener
	{
		private const ulong TRAVIS_CHANNEL = 504110403467476994;
		private const int SUCCESS_COLOR = 3066993;

		public static void CheckMessage(MessageCreateEventArgs e)
		{
			if (e.Channel.Id != TRAVIS_CHANNEL)
				return;

			if (e.Message.Embeds[0].Color != SUCCESS_COLOR)
				return;

			Bot.Instance.Client.UpdateStatusAsync(new DiscordActivity("with Continous Integration", ActivityType.Playing));

			// Update the bot
			Process.Start("update.bat");
			Process.GetCurrentProcess().Kill();
		}
	}
}