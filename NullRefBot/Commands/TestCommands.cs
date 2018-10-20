using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using RestSharp;

namespace NullRefBot.Commands
{
	public class TestCommands : BaseCommandModule
	{
		[Command("ping")]
		public async Task Ping(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync();

			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
		}

		[Command("tryget")]
		public async Task CreateUser(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync();

			RestClient client = new RestClient($"http://{Bot.Instance.Config.DatabaseIP}:{Bot.Instance.Config.DatabasePort}");

			RestRequest req = new RestRequest(Method.GET);

			IRestResponse response = client.Execute(req);

			await ctx.RespondAsync($"==RESPONSE==\n{response.Content}");
		}
	}
}
