using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2Pug
{
	public class Team
	{
		public TeamSide Side { get; internal set; }

		public int Skill { get; set; }

		public Dictionary<PlayerClass, List<Player>> Players { get; internal set; }

		public Team( TeamSide side )
		{
			this.Side = side;
			Skill = 0;

			Players = new Dictionary<PlayerClass, List<Player>>( 4 );

			Players[PlayerClass.Demo] = new List<Player>( 1 );
			Players[PlayerClass.Scout] = new List<Player>( 2 );
			Players[PlayerClass.Soldier] = new List<Player>( 2 );
			Players[PlayerClass.Medic] = new List<Player>( 1 );
		}

		public void AddPlayer( Player player, PlayerClass desiredClass )
		{
			Players[desiredClass].Add( player );
			this.Skill += player.Skill;
		}
	}

	public enum TeamSide
	{
		Blu,
		Red
	}
}
