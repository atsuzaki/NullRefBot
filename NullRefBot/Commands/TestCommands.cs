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
			await ctx.TriggerTypingAsync();

			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
		}

    [Command("popcorn")]
    public async Task Popcorn(CommandContext ctx) {

      await ctx.TriggerTypingAsync();

      DiscordEmbedBuilder embed;
      embed = new DiscordEmbedBuilder();
      embed.ImageUrl = "https://media1.tenor.com/images/54451401d52c0dd2fe9ee5752857d53c/tenor.gif";

      await ctx.RespondAsync(embed: embed);
    }

    [Command("banned")]
    public async Task Banned(CommandContext ctx) {
      await ctx.TriggerTypingAsync();

      DiscordEmbedBuilder embed;
      embed = new DiscordEmbedBuilder();
      embed.ImageUrl = "https://media1.tenor.com/images/66b9e27c779a1a314f0a8b31bb5609f7/tenor.gif";

      await ctx.RespondAsync(embed: embed);
    }
  }
}
