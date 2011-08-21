using System;
using System.Text.RegularExpressions;
using TF2Pug;

namespace IrcBot.Commands
{
	public class MumbleCommand : PlayerCommand
	{
		public MumbleCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^mumble|^vent", RegexOptions.Compiled | RegexOptions.IgnoreCase );
		}

		Regex b_command;
		protected override Regex Command { get { return b_command; } }

		public override bool ProcessCommand( string command, IrcPlayer player )
		{
			if (base.ProcessCommand( command, player ))
			{
				IrcBot.IrcClient.SendMessage( Meebey.SmartIrc4net.SendType.Message, IrcBot.Channel, @"Mumble info here." );
				return true;
			}

			return false;
		}
	}
}
