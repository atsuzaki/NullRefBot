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
		private const int SUCCESS_COLOR = 3066993;

		public static void CheckMessage(MessageCreateEventArgs e)
		{
			Console.WriteLine("CheckMessage called. channel id = " + e.Channel.Id);
			if (e.Channel.Id != TRAVIS_CHANNEL)
			{
				Console.WriteLine("Returning due to invalid channel id. Required ID: " + TRAVIS_CHANNEL + " actual ID: " + e.Channel.Id);
				return;
			}

			if (e.Message.Embeds.Count == 0 || e.Message.Embeds[0].Color != SUCCESS_COLOR)
			{
				Console.WriteLine("Returning due to invalid embeds. Count = " + e.Message.Embeds.Count);
				if (e.Message.Embeds.Count > 0)
					Console.WriteLine("\tInvalid color: " + e.Message.Embeds[0].Color + " Required color: " + SUCCESS_COLOR);
				return;
			}

			Bot.Instance.Client.UpdateStatusAsync(new DiscordActivity("with Continous Integration", ActivityType.Playing));
			Bot.Instance.Client.DebugLogger.LogMessage(LogLevel.Info, "CI", "Updating bot", DateTime.Now);

			// Update the bot
			Process.Start("update.bat");
			Process.GetCurrentProcess().Kill();
		}
	}
}