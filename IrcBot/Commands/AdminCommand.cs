using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TF2Pug;

namespace IrcBot.Commands
{
	public class AdminCommand : PlayerCommand
	{
		public AdminCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{

		}

		protected override int UserLevel
		{
			get
			{
				return 2;
			}
		}
	}
}
