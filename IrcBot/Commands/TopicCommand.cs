using System;
using System.Text.RegularExpressions;

namespace IrcBot.Commands
{
	public class TopicCommand : AdminCommand
	{
		public TopicCommand( TF2Pug.Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^topic .*", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				IrcBot.Topic = command.Substring( 6 );
				return true;
			}

			return false;
		}
	}
}
