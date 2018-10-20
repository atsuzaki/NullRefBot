using System;
using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace NullRefBot
{
	public class HelpFormatter : BaseHelpFormatter
	{
		public HelpFormatter(CommandContext ctx) : base(ctx) { }

		public override BaseHelpFormatter WithCommand(Command command)
		{
			throw new NotImplementedException();
		}

		public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
		{
			throw new NotImplementedException();
		}

		public override CommandHelpMessage Build()
		{
			throw new NotImplementedException();
		}
	}
}