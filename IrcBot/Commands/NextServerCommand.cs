using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;

namespace IrcBot.Commands
{
	public class NextServerCommand : AdminCommand
	{
		public NextServerCommand( TF2Pug.Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^nextserver", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				IrcBot.Servers.Enqueue( IrcBot.Servers.Dequeue() );
				IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, @"The next PUG will be played on " + IrcBot.Servers.Peek().FriendlyName );
				return true;
			}

			return false;
		}
	}
}
