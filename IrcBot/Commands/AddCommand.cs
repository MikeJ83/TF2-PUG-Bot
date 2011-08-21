using System;
using System.Text.RegularExpressions;
using TF2Pug;

namespace IrcBot.Commands
{
	public class AddCommand : PlayerCommand
	{
		public AddCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{ }

		static Regex Demo = new Regex( @"^dem|^demo", RegexOptions.Compiled | RegexOptions.IgnoreCase );
		static Regex Medic = new Regex( @"^med|^medic", RegexOptions.Compiled | RegexOptions.IgnoreCase );
		static Regex Scout = new Regex( @"^sc|^sco|^scou|^scout", RegexOptions.Compiled | RegexOptions.IgnoreCase );
		static Regex Soldier = new Regex( @"^so|^sol|^sold|^soldier", RegexOptions.Compiled | RegexOptions.IgnoreCase );

		public override bool ProcessCommand( string command, IrcPlayer player )
		{
			if (Scout.IsMatch( command ))
			{
				CurrentPug.AddPlayer( player, PlayerClass.Scout );
				return true;
			}

			if (Soldier.IsMatch( command ))
			{
				CurrentPug.AddPlayer( player, PlayerClass.Soldier );
				return true;
			}

			if (Demo.IsMatch( command ))
			{
				CurrentPug.AddPlayer( player, PlayerClass.Demo );
				return true;
			}

			if (Medic.IsMatch( command ))
			{
				CurrentPug.AddPlayer( player, PlayerClass.Medic );
				return true;
			}

			return false;
		}
	}
}
