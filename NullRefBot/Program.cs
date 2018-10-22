using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace NullRefBot
{
	public class Program
	{
		static void Main(string[] args)
		{
			Bot.Instance.RunAsync().Wait();
			Console.ReadLine();
		}
	}
}
