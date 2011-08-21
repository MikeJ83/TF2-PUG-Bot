using System;
using System.Text.RegularExpressions;

namespace IrcBot.Commands
{
	public class QuitCommand : AdminCommand
	{
		public QuitCommand( TF2Pug.Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^quit", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				IrcBot.IrcClient.RfcQuit( "Good Day!" );
				
				return true;
			}

			return false;
		}
	}
}
