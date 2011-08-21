using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Meebey.SmartIrc4net;
using TF2Pug;
using IrcBot.Commands;

namespace IrcBot
{
	/// <summary>
	/// IRC interface to the TF2Pug engine.
	/// </summary>
	public class IrcBot
	{
		#region Public properties

		public IrcClient IrcClient { get; private set; }

		/// <summary>
		/// IRC channel to use.
		/// </summary>
		public static string Channel
		{
			get
			{
				return "#tf2.pug";
			}
		}

		string b_topic;
		public string Topic {
			get
			{
				return b_topic;
			}
			set
			{
				b_topic = value;
				UpdateTopic();
			}
		}

		/// <summary>
		/// Mumble server address, port number, and password.
		/// </summary>
		public string MumbleInfo { get; private set; }

		public Pug CurrentPug { get; private set; }

		/// <summary>
		/// TF2 servers that are available for PUGs.
		/// </summary>
		public Queue<Server> Servers { get; private set; }

		/// <summary>
		/// Rotation of maps to play.
		/// </summary>
		public Queue<Map> Maps { get; private set; }

		#endregion

		#region Private properties

		DateTime TopicLastSet { get; set;}
		bool isWaitingToStart { get; set; }
		List<Commands.PlayerCommand> CommandList { get; set; }

		int playerThreshold;

		string gameSurgeAuth { get; set; }

		string gameSurgeAuthPassword { get; set; }

		#endregion

		#region Constructor

		public IrcBot()
		{
			b_topic = null;
			TopicLastSet = DateTime.MinValue;
			isWaitingToStart = false;

			ReadConfiguration();

			this.IrcClient = new IrcClient();
			IrcClient.ActiveChannelSyncing = true;
			IrcClient.SendDelay = 250;

			IrcClient.OnRawMessage += new IrcEventHandler( irc_OnRawMessage );
			IrcClient.OnChannelMessage += new IrcEventHandler( irc_OnChannelMessage );
			IrcClient.OnChannelNotice += new IrcEventHandler( irc_OnChannelNotice );
			IrcClient.OnChannelAction += new ActionEventHandler( irc_OnChannelAction );
			IrcClient.OnPart += new PartEventHandler( irc_OnPart );
			IrcClient.OnNickChange += new NickChangeEventHandler( irc_OnNickChange );
			IrcClient.OnQuit += new QuitEventHandler( irc_OnQuit );
			IrcClient.OnConnected += new EventHandler( irc_OnConnected );
			IrcClient.OnDisconnected += new EventHandler( irc_OnDisconnected );

			IrcClient.Connect( @"prothid.ca.us.gamesurge.net", 6667 );
			IrcClient.Listen();
		}

		#endregion

		#region IRC Events
		void irc_OnChannelAction( object sender, ActionEventArgs e )
		{
			Console.WriteLine( String.Format( "Channel Action: {0} <{1}> {2}", e.Data.Channel, e.Data.Nick, e.ActionMessage ) );
		}

		void irc_OnChannelNotice( object sender, IrcEventArgs e )
		{
			Console.WriteLine( String.Format( "{0} <{1}> {2}", e.Data.Channel, e.Data.Nick, e.Data.Message ) );
		}

		void irc_OnChannelMessage( object sender, IrcEventArgs e )
		{
			Console.WriteLine( String.Format( "{0} <{1}> {2}", e.Data.Channel, e.Data.Nick, e.Data.Message ) );

			if (PlayerCommand.ControlChar == e.Data.Message.ToCharArray()[0])
			{
				// reject users who are not auth'd with GameSurge
				if (!e.Data.Ident.StartsWith( "~" ))
				{
					IrcClient.RfcPrivmsg( e.Data.Nick, @"You must be authenticated with Gamesurge to play PUGs in this channel: http://www.gamesurge.net/newuser/" );
				}

				string commandText = e.Data.Message.Substring( 1 );
				int userLevel = 0;

				ChannelUser currentUser = IrcClient.GetChannelUser( e.Data.Channel, e.Data.Nick );

				if (currentUser.IsVoice)
					userLevel = 1;
				else if (currentUser.IsOp)
					userLevel = 2;

				IrcPlayer currentPlayer = new IrcPlayer( e.Data.Ident, e.Data.Nick, (IrcClient)sender, userLevel );


				foreach (PlayerCommand currentCommand in CommandList)
				{
					if (currentCommand.ProcessCommand( commandText, currentPlayer ))
						break;
				}
			}
			byte[] message = System.Text.Encoding.ASCII.GetBytes( e.Data.Message );
			foreach (byte b in message)
			{
				Console.WriteLine( b.ToString() );
			}
		}

		void irc_OnRawMessage( object sender, IrcEventArgs e )
		{
			Console.WriteLine( "RAW:" + e.Data.Type.ToString() + " " + e.Data.Message );
		}

		void irc_OnConnected( object sender, EventArgs e )
		{
			IrcClient.Login( new string[] { @"TF2-PUG-Bot", @"TF2PugBot", @"TF2Pugs" }, @"TF2 PUG Bot", 0, @"TF2PUGBot" );
			IrcClient.WriteLine( String.Format( "AUTHSERV AUTH {0} {1}", gameSurgeAuth, gameSurgeAuthPassword ) );
			IrcClient.RfcJoin( Channel );

			CurrentPug = new Pug();
			UpdateTopic();
			CurrentPug.OnPlayerAdded += new PlayerAddedOrRemovedHandler( CurrentPug_OnPlayerAdded );
			CurrentPug.OnPlayerRemoved += new PlayerAddedOrRemovedHandler( CurrentPug_OnPlayerRemoved );
			CurrentPug.OnPugFull += new PugFullHandler( CurrentPug_OnPugFull );
			CurrentPug.OnPugStarted += new PugStartedHandler( CurrentPug_OnPugStarted );

			CommandList = new List<Commands.PlayerCommand>();

			CommandList.Add( new Commands.AddCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.RemoveCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.SkillCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.HelpCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.MumbleCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.TestCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.TopicCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.QuitCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.StatusCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.ServerCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.NextServerCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.MapCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.NextMapCommand( CurrentPug, this ) );
		}

		void irc_OnDisconnected( object sender, EventArgs e )
		{
			Environment.Exit( 0 );
		}

		void irc_OnQuit( object sender, QuitEventArgs e )
		{
			CurrentPug.RemovePlayer( e.Who );
		}

		void irc_OnNickChange( object sender, NickChangeEventArgs e )
		{
			CurrentPug.ChangeName( e.OldNickname, e.NewNickname );
		}

		void irc_OnPart( object sender, PartEventArgs e )
		{
			CurrentPug.RemovePlayer( e.Who );
		}

		#endregion

		#region PUG Events
		void CurrentPug_OnPugStarted( object sender, PugStartedEventsArgs e )
		{
			IrcClient.SendMessage( SendType.Message, Channel, String.Format( "\x3\x32 BLU Team: {0} medic, {1} demo, {2} soldier, {3} soldier, {4} scout, {5} scout", new string[] { e.BluTeam.Players[PlayerClass.Medic][0].Name, e.BluTeam.Players[PlayerClass.Demo][0].Name, e.BluTeam.Players[PlayerClass.Soldier][0].Name, e.BluTeam.Players[PlayerClass.Soldier][1].Name, e.BluTeam.Players[PlayerClass.Scout][0].Name, e.BluTeam.Players[PlayerClass.Scout][1].Name } ) );
			IrcClient.SendMessage( SendType.Message, Channel, String.Format( "\x3\x34 RED Team: {0} medic, {1} demo, {2} soldier, {3} soldier, {4} scout, {5} scout", new string[] { e.RedTeam.Players[PlayerClass.Medic][0].Name, e.RedTeam.Players[PlayerClass.Demo][0].Name, e.RedTeam.Players[PlayerClass.Soldier][0].Name, e.RedTeam.Players[PlayerClass.Soldier][1].Name, e.RedTeam.Players[PlayerClass.Scout][0].Name, e.RedTeam.Players[PlayerClass.Scout][1].Name } ) );
			IrcClient.SendMessage( SendType.Message, Channel, String.Format( "PUG will be played on {0} on the map {1}", ((Pug)sender).Server.FriendlyName, ((Pug)sender).Map.FriendlyName ) );
			CurrentPug = null;
			CurrentPug = new Pug();
			UpdateTopic();
			CurrentPug.OnPlayerAdded += new PlayerAddedOrRemovedHandler( CurrentPug_OnPlayerAdded );
			CurrentPug.OnPlayerRemoved += new PlayerAddedOrRemovedHandler( CurrentPug_OnPlayerRemoved );
			CurrentPug.OnPugFull += new PugFullHandler( CurrentPug_OnPugFull );
			CurrentPug.OnPugStarted += new PugStartedHandler( CurrentPug_OnPugStarted );

			// Eventually do something to clean this up:
			CommandList = new List<Commands.PlayerCommand>();

			CommandList.Add( new Commands.AddCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.RemoveCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.SkillCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.HelpCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.MumbleCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.TestCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.TopicCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.QuitCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.StatusCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.ServerCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.NextServerCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.MapCommand( CurrentPug, this ) );
			CommandList.Add( new Commands.NextMapCommand( CurrentPug, this ) );
		}

		void CurrentPug_OnPugFull( object sender, EventArgs e )
		{
			if (isWaitingToStart)
				return;
			new Thread( new ThreadStart( StartPug ) ).Start();
		}

		void CurrentPug_OnPlayerRemoved( object sender, PlayerAddedOrRemovedEventArgs player )
		{
			UpdateTopic();
		}

		void CurrentPug_OnPlayerAdded( object sender, PlayerAddedOrRemovedEventArgs player )
		{
			UpdateTopic();
		}

		#endregion

		#region Private Methods

		void DelayedTopicUpdate()
		{
			Thread.Sleep( 5000 );
			if (DateTime.Now.Subtract( TopicLastSet ).TotalSeconds < 5)
			{
				Console.WriteLine( @"Topic already up to date" );
				return;
			}
			UpdateTopic();
			Console.WriteLine( @"Delayed update starting..." );
		}

		void UpdateTopic()
		{
			if (DateTime.Now.Subtract( TopicLastSet ).TotalSeconds < 3)
			{
				Console.WriteLine( @"Delaying topic update " + DateTime.Now.Subtract( TopicLastSet ).TotalSeconds.ToString() );
				new Thread( new ThreadStart( DelayedTopicUpdate ) ).Start();
				return;
			}

			Console.WriteLine( @"Setting topic" );
			TopicLastSet = DateTime.Now;

			string[] classes = {
								   CurrentPug.Players[PlayerClass.Medic].Count > 0 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Medic][0].Name) : "\u00033medic",
								   CurrentPug.Players[PlayerClass.Medic].Count > 1 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Medic][1].Name) : "\u00033medic",

								   CurrentPug.Players[PlayerClass.Demo].Count > 0 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Demo][0].Name) : "\u00033demo",
								   CurrentPug.Players[PlayerClass.Demo].Count > 1 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Demo][1].Name) : "\u00033demo",

								   CurrentPug.Players[PlayerClass.Soldier].Count > 0 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Soldier][0].Name) : "\u00033soldier",
								   CurrentPug.Players[PlayerClass.Soldier].Count > 1 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Soldier][1].Name) : "\u00033soldier",
								   CurrentPug.Players[PlayerClass.Soldier].Count > 2 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Soldier][2].Name) : "\u00033soldier",
								   CurrentPug.Players[PlayerClass.Soldier].Count > 3 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Soldier][3].Name) : "\u00033soldier",

								   CurrentPug.Players[PlayerClass.Scout].Count > 0 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Scout][0].Name) : "\u00033scout",
								   CurrentPug.Players[PlayerClass.Scout].Count > 1 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Scout][1].Name) : "\u00033scout",
								   CurrentPug.Players[PlayerClass.Scout].Count > 2 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Scout][2].Name) : "\u00033scout",
								   CurrentPug.Players[PlayerClass.Scout].Count > 3 ? String.Format("\u00034{0}", CurrentPug.Players[PlayerClass.Scout][3].Name) : "\u00033scout"
							   };
			if (!String.IsNullOrEmpty(Topic))
				IrcClient.RfcTopic( Channel, Topic + String.Format( " \x3\x34[\xf {0} - {1} - {2} - {3} - {4} - {5} - {6} - {7} - {8} - {9} - {10} - {11} \x3\x34]\xf", classes ) );
			else
				IrcClient.RfcTopic( Channel, String.Format( "\u00034,1\u0002[ {0} \u000316- {1} - {2} \u000316- {3} \u000316- {4} \u000316- {5} \u000316- {6} \u000316- {7} \u000316- {8} \u000316- {9} \u000316- {10} \u000316- {11} \x3\x34]\xf", classes ) );

		}

		void StartPug()
		{
			IrcClient.SendMessage( SendType.Notice, Channel, "The PUG is full and will start in about 30 seconds. Remove now if you don't want to play." );
			isWaitingToStart = true;
			Thread.Sleep( 30000 );
			if (CurrentPug.IsFull())
			{
				Server server = null;
				while (null == server)
				{
					server = Servers.Dequeue();
					server.RefreshStatus();
					Thread.Sleep( 1000 );
					if (server.PlayerCount == -1)
					{
						IrcClient.SendMessage( SendType.Message, Channel, String.Format( @"{0} is not responding. Trying the next server in five seconds...", server.FriendlyName ) );
						Servers.Enqueue( server );
						server = null;
						Thread.Sleep( 4000 );
					}
					else if (server.PlayerCount > playerThreshold)
					{
						IrcClient.SendMessage( SendType.Message, Channel, String.Format( @"{0} is in use. Trying the next server in five seconds...", server.FriendlyName ) );
						Servers.Enqueue( server );
						server = null;
						Thread.Sleep( 4000 );
					}

					// Check to see if someone removed while we were looking for a server
					if (!CurrentPug.IsFull())
					{
						isWaitingToStart = false;
						return;
					}
				}

				Map mapToPlay = Maps.Dequeue();
				CurrentPug.Start( mapToPlay, server );
				Maps.Enqueue( mapToPlay );
				Servers.Enqueue( server );
			}
			isWaitingToStart = false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Loads configuration settings from the database and config file
		/// </summary>
		public void ReadConfiguration()
		{
			PugConfigurationSection pugConfig = (PugConfigurationSection)ConfigurationManager.GetSection( "pugSettings" );
			playerThreshold = pugConfig.PlayerThreshold;
			MumbleInfo = pugConfig.MumbleInfo;
			gameSurgeAuth = pugConfig.GameSurgeAuth;
			gameSurgeAuthPassword = pugConfig.GameSurgeAuthPassword;

			using (SqlConnection sqlConnection = new SqlConnection( ConfigurationManager.ConnectionStrings["pug"].ConnectionString ))
			{
				sqlConnection.Open();

				// Read the list of servers from the database.
				using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
				{
					sqlCommand.CommandText = @"SELECT UniqueId, [Address], Port, [Password], RconPassword, FriendlyName FROM [Servers];";
					using (SqlDataReader sqlReader = sqlCommand.ExecuteReader( CommandBehavior.SingleResult ))
					{
						Servers = new Queue<Server>();

						while (sqlReader.Read())
						{
							if (DBNull.Value == sqlReader[2])
								Servers.Enqueue( new Server( (Guid)sqlReader[0], (string)sqlReader[5], (string)sqlReader[1], (string)sqlReader[3], (string)sqlReader[4] ) );
							else
								Servers.Enqueue( new Server( (Guid)sqlReader[0], (string)sqlReader[5], (string)sqlReader[1], (string)sqlReader[3], (ushort)sqlReader[2], (string)sqlReader[4] ) );
						}
					}
				}

				// Read the list of maps from the database.
				using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
				{
					sqlCommand.CommandText = @"SELECT UniqueId, Name, FriendlyName FROM Maps;";
					using (SqlDataReader sqlReader = sqlCommand.ExecuteReader( CommandBehavior.SingleResult ))
					{
						Maps = new Queue<Map>();
						while (sqlReader.Read())
						{
							Maps.Enqueue( new Map( (Guid)sqlReader[0], (string)sqlReader[1], (string)sqlReader[2] ) );
						}
					}
				}
			}
		}

		#endregion
	}
}
