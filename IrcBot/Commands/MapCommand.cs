using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TF2Pug;

namespace IrcBot.Commands
{
	public class MapCommand : PlayerCommand
	{
		public MapCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^map", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, @"The next PUG will play " + IrcBot.Maps.Peek().FriendlyName );
				return true;
			}

			return false;
		}
	}
}
