using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NullRefBot.Utils;
using RestSharp;

namespace NullRefBot.Commands
{
	public class TestCommands : BaseCommandModule
	{
		[Command("ping")]
		public async Task Ping(CommandContext ctx)
		{
			if (ctx.Channel.Id != 502928327766704130 && ctx.Channel.Id != 502646757759385602)
				return;

			await ctx.TriggerTypingAsync();

			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
		}

		[Command("tryadduser")]
		public async Task CreateUser(CommandContext ctx)
		{
			if (ctx.Channel.Id != 502928327766704130 && ctx.Channel.Id != 502646757759385602)
				return;

			await ctx.TriggerTypingAsync();

			RestClient client = new RestClient($"http://{Bot.Instance.Config.DatabaseIP}:{Bot.Instance.Config.DatabasePort}");

			RestRequest req = new RestRequest("/users/fuckYouApheAndYourConfusingAssSchemas", Method.PUT, DataFormat.Json);

			IRestResponse response = client.Execute(req);

			await ctx.RespondAsync($"```\n==RESPONSE==\n{response.Content}\n============```");
		}

		[Command("logdebugchannels")]
		public async Task LogDebugChannels(CommandContext ctx)
		{
			if (!ctx.IsAllowedInChannel())
				return;

			await ctx.TriggerTypingAsync();

			string s = "In Debug Channel? " + (Bot.Instance.Config.DebugChannels.Contains(ctx.Channel.Id)) + "\n";
			foreach (ulong chan in Bot.Instance.Config.DebugChannels)
				s += chan + "\n";
			await ctx.RespondAsync(s);
		}
	}
}