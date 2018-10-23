﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NullRefBot.Utils;
using RestSharp;

namespace NullRefBot.Commands
{
    [RequirePermissions(Permissions.ManageRoles)]
    public class Moderation : BaseCommandModule
	{
        const ulong MUTED_ROLE_ID = 503356983353802752; //TODO: TEMP

        [Command("testgetmute")]
		public async Task TestGetMute(CommandContext ctx, DiscordMember member) {
			await ctx.TriggerTypingAsync();

		    try {
		        var res = await GetMute(member.Id);
		        Console.WriteLine(res == null);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

		[Command("mute")]
		public async Task Mute(CommandContext ctx, DiscordMember member, int duration = 5) {
			await ctx.TriggerTypingAsync();

		    int durationInSeconds = duration * 60;
		    int durationInMs = duration * 1000 * 60;

		    try {
		        await PutMute(member.Id, durationInSeconds);
                await member.GrantRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
                await ctx.RespondAsync($"**{member.DisplayName}** is muted for {duration} minutes");

                TimeoutUtils.SetTimeout(() => TryUnmuteAsync(member), durationInMs);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                await ctx.RespondAsync($"Error: {e.Message}");
            }
        }

        [Command("unmute")]
		public async Task Unmute(CommandContext ctx, DiscordMember member) {
			await ctx.TriggerTypingAsync();

            try {
                await PutMute(member.Id, -1);
                await member.RevokeRoleAsync(ctx.Guild.GetRole(MUTED_ROLE_ID));
                await ctx.RespondAsync($"**{member.DisplayName}** is unmuted");
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                await ctx.RespondAsync($"Error: {e.Message}");
            }
		}

	    private async Task<bool> TryUnmuteAsync(DiscordMember member) {
	        Console.WriteLine("trying to unmute");

	        var mute = await GetMute(member.Id);

	        if (mute == null) {
		        await member.RevokeRoleAsync(member.Guild.GetRole(MUTED_ROLE_ID));
	            return true;
	        }
	        if (mute.mutedUntil < DateTime.Now) { //TODO: how?
                await PutMute(member.Id, -1);
		        await member.RevokeRoleAsync(member.Guild.GetRole(MUTED_ROLE_ID));
                return true;
            }
	        return false;
	    }

        private Task<Mute> GetMute(ulong discordID) { 
            var req = new RestRequest();

            req.Resource = "/mutes/muted/{discord_id}";
            req.AddParameter("discord_id", discordID, ParameterType.UrlSegment);

            req.Method = Method.GET;
	        req.Timeout = 30 * 1000;
            req.RequestFormat = DataFormat.Json;
            
            return Task.Run(async () => {
                var res = await RequestUtils.ExecuteAsyncRaw<Mute>(req);
                Console.WriteLine(res.Data);
                return res.Data;
            });
	    }

	    private Task PutMute(ulong discordID, int durationInSeconds) {
            var req = new RestRequest();

            req.Resource = "/mutes/{discord_id}/muteFor/{amount}";
            req.AddParameter("discord_id", discordID, ParameterType.UrlSegment);
            req.AddParameter("amount", durationInSeconds, ParameterType.UrlSegment);

            req.Method = Method.PUT;
	        req.Timeout = 30 * 1000;
            req.RequestFormat = DataFormat.Json;

            return Task.Run( async () => {
                var res = await RequestUtils.ExecuteAsyncRaw<Mute>( req );

                Console.WriteLine(res.StatusCode);
                if (res.StatusCode != HttpStatusCode.OK) {
                    throw new Exception("Server returns" + res.StatusCode);
                }
            });
	    }
	}
}