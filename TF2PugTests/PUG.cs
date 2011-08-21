using System;
using NUnit.Framework;
using TF2Pug;

namespace TF2PugTests
{
	[TestFixture]
	public class PugTests
	{

		Player player1, player2, player3, player4, player5, player6, player7, player8, player9, player10, player11, player12;
		Map map;
		Server server;

		public PugTests()
		{
			player1 = new Player("Player01", "Player 01") { Skill = 200 };
			player2 = new Player("Player02", "Player 02") { Skill = 100 };
			player3 = new Player("Player03", "Player 03") { Skill = 200 };
			player4 = new Player("Player04", "Player 04") { Skill = 100 };
			player5 = new Player("Player05", "Player 05") { Skill = 100 };
			player6 = new Player("Player06", "Player 06") { Skill = 100 };
			player7 = new Player("Player07", "Player 07") { Skill = 100 };
			player8 = new Player("Player08", "Player 08") { Skill = 100 };
			player9 = new Player("Player09", "Player 09") { Skill = 100 };
			player10 = new Player( "Player10", "Player 10" ) { Skill = 100 };
			player11 = new Player( "Player11", "Player 11" ) { Skill = 100 };
			player12 = new Player( "Player12", "Player 12" ) { Skill = 100 };

			map = new Map( Guid.Empty, null, null );

			server = new Server( Guid.Empty, null, "dallas1.tf2pug.us", "123", "123" );
		}

		[Test]
		public void AddPlayerTest()
		{
			Pug pug = new Pug();

			pug.AddPlayer( player1, PlayerClass.Medic );
			Assert.Contains( player1, pug.Players[PlayerClass.Medic] );
		}

		[Test]
		public void AddTooManyPlayersTest()
		{
			Pug pug = new Pug();

			pug.AddPlayer( player1, PlayerClass.Medic );
			pug.AddPlayer( player2, PlayerClass.Medic );
			pug.AddPlayer( player3, PlayerClass.Medic );

			Assert.That( pug.Players[PlayerClass.Medic].Count, Is.EqualTo( 2 ) );
		}

		[Test]
		public void RemovePlayerTest()
		{
			Pug pug = new Pug();
			
			pug.AddPlayer( player1, PlayerClass.Medic );
			pug.RemovePlayer( player1 );

			Assert.IsEmpty( pug.Players[PlayerClass.Medic] );
		}

		[Test]
		public void ReAddPlayerTest()
		{
			Pug pug = new Pug();

			pug.AddPlayer( player1, PlayerClass.Medic );
			pug.AddPlayer( player1, PlayerClass.Demo );

			Assert.IsEmpty( pug.Players[PlayerClass.Medic] );
		}

		[Test]
		[ExpectedException( typeof( PugNotFullException ) )]
		public void PugStartWithNotEnoughPlayersTest()
		{
			Pug pug = new Pug();
			pug.AddPlayer( player1, PlayerClass.Medic );
			pug.Start( map, server );
		}

		[Test]
		public void StartTest()
		{
			Pug pug = new Pug();
			pug.AddPlayer( player1, PlayerClass.Medic );
			pug.AddPlayer( player2, PlayerClass.Medic );
			pug.AddPlayer( player3, PlayerClass.Demo );
			pug.AddPlayer( player4, PlayerClass.Demo );
			pug.AddPlayer( player5, PlayerClass.Scout );
			pug.AddPlayer( player6, PlayerClass.Scout );
			pug.AddPlayer( player7, PlayerClass.Scout );
			pug.AddPlayer( player8, PlayerClass.Scout );
			pug.AddPlayer( player9, PlayerClass.Soldier );
			pug.AddPlayer( player10, PlayerClass.Soldier );
			pug.AddPlayer( player11, PlayerClass.Soldier );
			pug.AddPlayer( player12, PlayerClass.Soldier );

			PugStartedHandler anonDelegate = delegate( object sender, PugStartedEventsArgs data )
			{
				Assert.IsFalse( (player1 == data.RedTeam.Players[PlayerClass.Medic][0]) && (player3 == data.RedTeam.Players[PlayerClass.Demo][0]), @"Teams are not balanced." );
				Assert.IsFalse( (player1 == data.BluTeam.Players[PlayerClass.Medic][0]) && (player3 == data.BluTeam.Players[PlayerClass.Demo][0]), @"Teams are not balanced." );
			};

			pug.OnPugStarted += anonDelegate;

			pug.Start( map, server );

			pug.OnPugStarted -= anonDelegate;
		}
	}
}
