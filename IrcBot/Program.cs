using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Meebey.SmartIrc4net;
using TF2Pug;
using IrcBot.Commands;

namespace IrcBot
{
	class Program
	{
		static IrcBot bot;

		static void Main( string[] args )
		{
			bot = new IrcBot();

			new Thread( new ThreadStart( ReadCommands ) ).Start();
		}

		public static void ReadCommands()
		{
			while (true)
			{
				string cmd = System.Console.ReadLine();
				bot.IrcClient.WriteLine( cmd );
			}
		}
	}
}
