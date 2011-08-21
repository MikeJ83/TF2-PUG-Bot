using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TF2Pug;

namespace IrcBot.Commands
{
	public class HelpCommand : PlayerCommand
	{
		public HelpCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^help|^man", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, @"For information about the PUG process visit: http://tf2pug.us/" );
				return true;
			}

			return false;
		}
	}
}
