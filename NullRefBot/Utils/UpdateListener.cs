using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace NullRefBot.Utils
{
	public static class UpdateListener
	{
		private const ulong TRAVIS_CHANNEL = 504110403467476994;
		private static DiscordColor successColor = new DiscordColor("#2ECC71");

		public static void CheckMessage(MessageCreateEventArgs e)
		{
			if (e.Channel.Id != TRAVIS_CHANNEL)
				return;

			if (e.Message.Embeds.Count == 0 || !e.Message.Embeds[0].Author.Name.ToLower().Contains("passed"))
				return;

			Bot.Instance.Client.UpdateStatusAsync(new DiscordActivity("with Continous Integration", ActivityType.Playing));
			Bot.Instance.Client.DebugLogger.LogMessage(LogLevel.Info, "CI", "Updating bot", DateTime.Now);

			// Update the bot
			Process.Start("update.bat");
			Process.GetCurrentProcess().Kill();
		}
	}
}