using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TF2Pug;

namespace IrcBot.Commands
{
	public class StatusCommand : PlayerCommand
	{
		public StatusCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^status", RegexOptions.Compiled | RegexOptions.IgnoreCase );
		}

		Regex b_command;
		protected override Regex Command
		{
			get
			{
				return b_command;
			}
		}

		public override bool ProcessCommand( string command, IrcPlayer player )
		{
			if (base.ProcessCommand( command, player ))
			{
				foreach (Server currentServer in IrcBot.Servers)
				{
					currentServer.RefreshStatus();
					System.Threading.Thread.Sleep( 500 );
					if (currentServer.PlayerCount >= 0)
						IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, String.Format( "{0} player(s) on {1}.", currentServer.PlayerCount, currentServer.FriendlyName ) );
					else
						IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, String.Format( "{0} is unreachable.", currentServer.FriendlyName ) );
				}

				return true;
			}

			return false;
		}
	}
}
