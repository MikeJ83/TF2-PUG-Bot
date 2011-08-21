using System;
using System.Text.RegularExpressions;
using TF2Pug;

namespace IrcBot.Commands
{
	public class RemoveCommand : PlayerCommand
	{
		public RemoveCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{ }

		protected override Regex Command
		{
			get { return new Regex( @"^remove|^rem|^del", RegexOptions.IgnoreCase ); }
		}

		public override bool ProcessCommand( string command, IrcPlayer player )
		{
			if (base.ProcessCommand( command, player ))
			{
				CurrentPug.RemovePlayer( player );
				return true;
			}

			return false;
		}
	}
}
