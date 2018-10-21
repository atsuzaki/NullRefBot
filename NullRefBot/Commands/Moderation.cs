using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace NullRefBot.Commands
{
	public class Moderation : BaseCommandModule
	{
		[Command("mute")]
		public async Task Mute(CommandContext ctx, DiscordMember member, int duration = 5) {
		    const ulong MUTED_ROLE_ID = 503356983353802752; //TEMP

			await ctx.TriggerTypingAsync();
			await ctx.RespondAsync($"*{member.DisplayName}* is muted for {duration} minutes");

		    await member.GrantRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
            //set timeout to check db later
		}

		[Command("unmute")]
		public async Task Unmute(CommandContext ctx, DiscordMember member) {
		    const ulong MUTED_ROLE_ID = 503356983353802752; //TEMP

			await ctx.TriggerTypingAsync();
			await ctx.RespondAsync($"*{member.DisplayName}* is unmuted");

		    await member.RevokeRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
            //reset db
		}
	}
}