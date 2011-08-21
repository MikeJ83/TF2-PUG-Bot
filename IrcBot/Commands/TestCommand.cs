using System;
using System.Text.RegularExpressions;
using TF2Pug;


namespace IrcBot.Commands
{
	public class TestCommand : AdminCommand
	{
		public TestCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^test", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				Player player1, player2, player3, player4, player5, player6, player7, player8, player9, player10, player11;
				player1 = new Player( "Player01", "Player 01" ) { Skill = 200 };
				player2 = new Player( "Player02", "Player 02" ) { Skill = 50 };
				player3 = new Player( "Player03", "Player 03" ) { Skill = 200 };
				player4 = new Player( "Player04", "Player 04" ) { Skill = 75 };
				player5 = new Player( "Player05", "Player 05" ) { Skill = 115 };
				player6 = new Player( "Player06", "Player 06" ) { Skill = 98 };
				player7 = new Player( "Player07", "Player 07" ) { Skill = 215 };
				player8 = new Player( "Player08", "Player 08" ) { Skill = 75 };
				player9 = new Player( "Player09", "Player 09" ) { Skill = 125 };
				player10 = new Player( "Player10", "Player 10" ) { Skill = 185 };
				player11 = new Player( "Player11", "Player 11" ) { Skill = 25 };

				CurrentPug.AddPlayer( player1, PlayerClass.Medic );
				CurrentPug.AddPlayer( player2, PlayerClass.Medic );
				CurrentPug.AddPlayer( player3, PlayerClass.Demo );
				CurrentPug.AddPlayer( player4, PlayerClass.Demo );
				CurrentPug.AddPlayer( player5, PlayerClass.Scout );
				CurrentPug.AddPlayer( player6, PlayerClass.Scout );
				CurrentPug.AddPlayer( player7, PlayerClass.Scout );
				CurrentPug.AddPlayer( player8, PlayerClass.Scout );
				CurrentPug.AddPlayer( player9, PlayerClass.Soldier );
				CurrentPug.AddPlayer( player10, PlayerClass.Soldier );
				CurrentPug.AddPlayer( player11, PlayerClass.Soldier );
				return true;
			}

			return false;
		}
	}
}
