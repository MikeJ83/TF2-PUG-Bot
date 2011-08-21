using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace TF2Pug
{
	/// <summary>
	/// This class is responsible for running a single PUG.
	/// </summary>
	public class Pug
	{
		#region Public Properties

		/// <summary>
		/// List of players added to the current PUG, arranged by class. TODO: Convert this to a one-dimensional array.
		/// </summary>
		public Dictionary<PlayerClass, List<Player>> Players { get; internal set; }

		/// <summary>
		/// Map that will be played. TODO: This can probably be removed.
		/// </summary>
		public Map Map { get; set; }

		/// <summary>
		/// Server that the PUG will be played on. TODO: This can probably be removed.
		/// </summary>
		public Server Server { get; set; }

		/// <summary>
		/// Unique ID for the PUG.
		/// </summary>
		public Guid UniqueId { get; private set; }

		#endregion

		/// <summary>
		/// Instantiate a new PUG.
		/// </summary>
		public Pug()
		{
			this.UniqueId = Guid.NewGuid();

			Players = new Dictionary<PlayerClass, List<Player>>( 4 );

			Players[PlayerClass.Demo] = new List<Player>( 2 );
			Players[PlayerClass.Scout] = new List<Player>( 4 );
			Players[PlayerClass.Soldier] = new List<Player>( 4 );
			Players[PlayerClass.Medic] = new List<Player>( 2 );
		}

		/// <summary>
		/// Add a player to the PUG.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="desiredClass"></param>
		public void AddPlayer( Player player, PlayerClass desiredClass )
		{
			if (desiredClass < PlayerClass.Medic && player.GamesPlayed > 10 && player.MedicPercentage < 0.06)
			{
				player.SendMessage( @"Your medic percentage is too low. You must maintain a medic percentage higher than %6 to play here." );
				return;
			}

			if (Players[desiredClass].Count < Players[desiredClass].Capacity)
			{
				// Check to see if the player is already added to the PUG, and just wants to change classes.
				foreach (PlayerClass currentClass in Players.Keys)
				{
					foreach (Player currentPlayer in Players[currentClass])
					{
						if (currentPlayer.Id == player.Id)
						{
							Players[currentClass].Remove( currentPlayer );
							break;
						}
					}
				}

				Players[desiredClass].Add( player );

				if (null != OnPlayerAdded)
					OnPlayerAdded( this, new PlayerAddedOrRemovedEventArgs( player ) );

				if (this.IsFull() && null != OnPugFull)
				{
					OnPugFull( this, null );
				}

				return;
			}

			player.SendMessage( "This class is full. Try adding as a different class." );
		}

		/// <summary>
		/// Checks to see if the PUG is full.
		/// </summary>
		/// <returns>True if the PUG is full; otherwise, false.</returns>
		public bool IsFull()
		{
			return Players[PlayerClass.Medic].Count == Players[PlayerClass.Medic].Capacity
					&& Players[PlayerClass.Demo].Count == Players[PlayerClass.Demo].Capacity
					&& Players[PlayerClass.Soldier].Count == Players[PlayerClass.Soldier].Capacity
					&& Players[PlayerClass.Scout].Count == Players[PlayerClass.Scout].Capacity;
		}

		#region Public Methods

		/// <summary>
		/// Remove a player from the PUG.
		/// </summary>
		/// <param name="player"></param>
		public void RemovePlayer( Player player )
		{
			foreach (List<Player> currentClass in Players.Values)
			{
				if (currentClass.Remove( player ))
				{
					if (null != OnPlayerRemoved)
						OnPlayerRemoved( this, new PlayerAddedOrRemovedEventArgs( player ) );

					break;
				}
			}
		}

		/// <summary>
		/// Removes a player by their name (a hack required due to a shortcoming or misunderstanding of the IRC library).
		/// </summary>
		/// <param name="Name"></param>
		public void RemovePlayer( string name )
		{
			foreach (PlayerClass currentClass in Players.Keys)
			{
				foreach (Player currentPlayer in Players[currentClass])
				{
					if (currentPlayer.Name == name)
					{
						Players[currentClass].Remove( currentPlayer );
						if (null != OnPlayerRemoved)
							OnPlayerRemoved( this, new PlayerAddedOrRemovedEventArgs( null ) );
						return;
					}
				}
			}
		}

		/// <summary>
		/// Renames a player (a hack required due to a shortcoming or misunderstanding of the IRC library).
		/// </summary>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		public void ChangeName( string oldName, string newName )
		{
			foreach (PlayerClass currentClass in Players.Keys)
			{
				foreach (Player currentPlayer in Players[currentClass])
				{
					if (currentPlayer.Name == oldName)
					{
						currentPlayer.Name = newName;
						if (null != OnPlayerRemoved)
							OnPlayerRemoved( this, new PlayerAddedOrRemovedEventArgs( currentPlayer ) );
					}
				}
			}
		}

		/// <summary>
		/// Assigns teams and sets up the server for the PUG.
		/// </summary>
		/// <param name="map">Map that will be played.</param>
		/// <param name="server">Server that the PUG will be played on.</param>
		public void Start(Map map, Server server)
		{
			this.Map = map;
			this.Server = server;

			foreach (List<Player> currentClass in Players.Values)
			{
				if (currentClass.Count < currentClass.Capacity)
					throw new PugNotFullException();
			}

			Team bluTeam = new Team( TeamSide.Blu );
			Team redTeam = new Team( TeamSide.Red );
			
			Players[PlayerClass.Scout].Sort( new PlayerSkillLevelComparer() );
			Players[PlayerClass.Soldier].Sort( new PlayerSkillLevelComparer() );
			Players[PlayerClass.Demo].Sort( new PlayerSkillLevelComparer() );
			Players[PlayerClass.Medic].Sort( new PlayerSkillLevelComparer() );

			foreach (Player currentPlayer in Players[PlayerClass.Scout])
				AddPlayerToTeam( redTeam, bluTeam, currentPlayer, PlayerClass.Scout );

			foreach (Player currentPlayer in Players[PlayerClass.Soldier])
				AddPlayerToTeam( redTeam, bluTeam, currentPlayer, PlayerClass.Soldier );

			// Add the demo with the higher skill rating to the team that needs it
			if (bluTeam.Skill <= redTeam.Skill)
			{
				bluTeam.AddPlayer( Players[PlayerClass.Demo][0], PlayerClass.Demo );
				redTeam.AddPlayer( Players[PlayerClass.Demo][1], PlayerClass.Demo );
			}
			else
			{
				bluTeam.AddPlayer( Players[PlayerClass.Demo][1], PlayerClass.Demo );
				redTeam.AddPlayer( Players[PlayerClass.Demo][0], PlayerClass.Demo );
			}

			// Add the medic with the higher skill rating to the team that needs it
			if (bluTeam.Skill <= redTeam.Skill)
			{
				bluTeam.AddPlayer( Players[PlayerClass.Medic][0], PlayerClass.Medic );
				redTeam.AddPlayer( Players[PlayerClass.Medic][1], PlayerClass.Medic );
			}
			else
			{
				bluTeam.AddPlayer( Players[PlayerClass.Medic][1], PlayerClass.Medic );
				redTeam.AddPlayer( Players[PlayerClass.Medic][0], PlayerClass.Medic );
			}

			Server.StartPug( this.UniqueId, Map );

			// Save the PUG to the database
			using (SqlConnection sqlConnection = new SqlConnection( ConfigurationManager.ConnectionStrings["pug"].ConnectionString ))
			{
				sqlConnection.Open();

				using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
				{
					sqlCommand.CommandText = @"INSERT INTO PUGs (UniqueId, MapId, BluScore, RedScore, Started) VALUES (@UniqueId, @MapId, 0, 0, @Started);";
					sqlCommand.Parameters.Add( new SqlParameter( @"UniqueId", SqlDbType.UniqueIdentifier ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"MapId", SqlDbType.UniqueIdentifier ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"Started", SqlDbType.DateTime ) );
					sqlCommand.Parameters[0].Value = this.UniqueId;
					sqlCommand.Parameters[1].Value = this.Map.UniqueId;
					sqlCommand.Parameters[2].Value = DateTime.Now;

					sqlCommand.ExecuteScalar();
				}

				using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
				{
					sqlCommand.CommandText = @"sp_AddPlayerToPug";
					sqlCommand.CommandType = CommandType.StoredProcedure;

					sqlCommand.Parameters.Add( new SqlParameter( @"PugId", SqlDbType.UniqueIdentifier ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"PlayerId", SqlDbType.NVarChar ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"PlayerName", SqlDbType.NVarChar ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"PlayerClass", SqlDbType.TinyInt ) );

					sqlCommand.Parameters[0].Value = this.UniqueId;

					foreach (PlayerClass currentClass in Players.Keys)
					{
						foreach (Player currentPlayer in Players[currentClass])
						{
							sqlCommand.Parameters[1].Value = currentPlayer.Id;
							sqlCommand.Parameters[2].Value = currentPlayer.Name;
							sqlCommand.Parameters[3].Value = currentClass;

							sqlCommand.ExecuteScalar();
						}
					}
				}
			}

			if (null != OnPugStarted)
				OnPugStarted( this, new PugStartedEventsArgs( redTeam, bluTeam ) );
		}

		/// <summary>
		/// Adds a player to a team, and attempts to make the teams even. Currently only used for the scout and solider classes.
		/// </summary>
		/// <param name="redTeam"></param>
		/// <param name="bluTeam"></param>
		/// <param name="player"></param>
		/// <param name="desiredClass"></param>
		protected void AddPlayerToTeam( Team redTeam, Team bluTeam, Player player, PlayerClass desiredClass )
		{

			if ((bluTeam.Players[desiredClass].Count < bluTeam.Players[desiredClass].Capacity)
				&& (bluTeam.Skill <= redTeam.Skill) || (redTeam.Players[desiredClass].Count == redTeam.Players[desiredClass].Capacity) )
				bluTeam.AddPlayer( player, desiredClass );
			else
				redTeam.AddPlayer( player, desiredClass );
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Save the final score for a PUG to the database.
		/// </summary>
		/// <param name="pugId">Unique ID of the PUG.</param>
		/// <param name="bluScore">Blu team's final score.</param>
		/// <param name="redScore">Red team's final score.</param>
		public static void ReportScore( Guid pugId, byte bluScore, byte redScore )
		{
			using (SqlConnection sqlConnection = new SqlConnection( ConfigurationManager.ConnectionStrings["pug"].ConnectionString ))
			{
				sqlConnection.Open();

				using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
				{
					sqlCommand.CommandText = @"sp_ReportScore";
					sqlCommand.CommandType = CommandType.StoredProcedure;

					sqlCommand.Parameters.Add( new SqlParameter( @"PugId", SqlDbType.UniqueIdentifier ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"BluScore", SqlDbType.TinyInt ) );
					sqlCommand.Parameters.Add( new SqlParameter( @"RedScore", SqlDbType.TinyInt ) );
					sqlCommand.Parameters[0].Value = pugId;
					sqlCommand.Parameters[1].Value = bluScore;
					sqlCommand.Parameters[2].Value = redScore;

					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Triggered when a player has been added to the PUG.
		/// </summary>
		public event PlayerAddedOrRemovedHandler OnPlayerAdded;

		/// <summary>
		/// Triggered when a player has been removed from the PUG.
		/// </summary>
		public event PlayerAddedOrRemovedHandler OnPlayerRemoved;

		/// <summary>
		/// Triggered when the PUG starts.
		/// </summary>
		public event PugStartedHandler OnPugStarted;

		/// <summary>
		/// Triggered when the PUG is full.
		/// </summary>
		public event PugFullHandler OnPugFull;

		#endregion
	}

	public delegate void PlayerAddedOrRemovedHandler( object sender, PlayerAddedOrRemovedEventArgs player );
	public delegate void PugStartedHandler( object sender, PugStartedEventsArgs e );
	public delegate void PugFullHandler (object sender, EventArgs e);

	public class PlayerAddedOrRemovedEventArgs : EventArgs
	{
		public Player Player { get; private set; }

		public PlayerAddedOrRemovedEventArgs( Player player )
		{
			this.Player = player;
		}
	}

	public class PugStartedEventsArgs : EventArgs
	{
		public Team RedTeam { get; internal set; }

		public Team BluTeam { get; internal set; }

		public PugStartedEventsArgs( Team red, Team blu )
		{
			this.RedTeam = red;
			this.BluTeam = blu;
		}
	}
}
