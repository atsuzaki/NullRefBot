using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NullRefBot.Utils;
using RestSharp;

namespace NullRefBot.Commands
{
	public class Moderation : BaseCommandModule
	{
        //TODO: figure out how to restrict to admin/mods/owner only
		[Command("mute")]
		public async Task Mute(CommandContext ctx, DiscordMember member, int duration = 5) {
		    const ulong MUTED_ROLE_ID = 503356983353802752; //TEMP

			await ctx.TriggerTypingAsync();
			await ctx.RespondAsync($"**{member.DisplayName}** is muted for {duration} minutes");

		    int durationInMs = duration * 1000 * 60;
		    Helpers.SetTimeout(() => {
		        PostMute(member.Id, -1);
		        member.RevokeRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
		    }, durationInMs);

            await member.GrantRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));

		    //check db later
		}

		[Command("unmute")]
		public async Task Unmute(CommandContext ctx, DiscordMember member) {
		    const ulong MUTED_ROLE_ID = 503356983353802752; //TEMP

			await ctx.TriggerTypingAsync();
			await ctx.RespondAsync($"**{member.DisplayName}** is unmuted");

		    PostMute(member.Id, -1);
		    await member.RevokeRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
		}

	    private async Task<bool> TryUnmuteAsync(DiscordMember member) {
            //ask permission to unmute 
            PostMute(member.Id, -1);
	        return true;
	    }

	    private void PostMute(ulong discordID, int durationInSeconds) {
            var req = new RestRequest();
            req.Resource = "/mute/{discord_id}?mute_time={amount}"; //TODO: temp
            req.AddParameter("discord_id", discordID);
            req.AddParameter("amount", durationInSeconds);

            req.Method = Method.PUT;
            req.RequestFormat = DataFormat.Json;
        }
	}
}