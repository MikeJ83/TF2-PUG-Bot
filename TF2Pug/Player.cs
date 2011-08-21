using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2Pug
{
	/// <summary>
	/// Represents a single player within the PUG system.
	/// </summary>
	public class Player
	{
		#region Public Properties

		/// <summary>
		/// Player's ID
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Players current name (alias)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Players skill level (higher is better)
		/// </summary>
		public int Skill { get; set; }

		/// <summary>
		/// Number of games played.
		/// </summary>
		public uint GamesPlayed { get; set; }

		/// <summary>
		/// Percentage of games played as a medic.
		/// </summary>
		public double MedicPercentage { get; set; }

		#endregion

		#region Constructor

		public Player( string id, string name )
		{
			this.Id = id;
			this.Name = name;

			// TODO: Add db integration code to lookup stats

			Skill = 100;
			GamesPlayed = 0;
			MedicPercentage = 0;
		}

		#endregion

		/// <summary>
		/// Sends a message to the player.
		/// </summary>
		/// <param name="message">Message to send.</param>
		public virtual void SendMessage( string message )
		{
			throw new NotImplementedException();
		}

		public override bool Equals( object obj )
		{
			if (null == obj || null == (obj as Player))
				return false;

			if (this.Id.Equals( ((Player)obj).Id ))
				return true;

			return false;
		}
		
	}

	public class PlayerSkillLevelComparer : IComparer<Player>
	{
		public int Compare( Player x, Player y )
		{
			if (x != null && y != null)
				return y.Skill.CompareTo( x.Skill );
			return 0;
		}
	}

	
}
