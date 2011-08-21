using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

namespace TF2Pug
{
	/// <summary>
	/// Represents a TF2 server instance.
	/// </summary>
	public class Server
	{

		static Regex StatusCommand = new Regex( @"hostname: .*\nversion : .*\nudp/ip  :  .*\nmap     : .*\n", RegexOptions.Compiled );
		static Regex StatusCommandPlayers = new Regex( @"\nplayers : [0-9]+", RegexOptions.Compiled );

		#region Public Properties

		public Guid UniqueId { get; private set; }

		public string FriendlyName { get; private set; }

		/// <summary>
		/// Default SRCDS port number.
		/// </summary>
		public const ushort DefaultPort = 27015;

		/// <summary>
		/// Server address (can be a hostname or IP address).
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// Server connect password.
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// Server port number.
		/// </summary>
		public ushort Port { get; private set; }

		/// <summary>
		/// Server RCON password.
		/// </summary>
		public string RconPassword { get; private set; }

		/// <summary>
		/// Number of players currently connected to the server.
		/// </summary>
		public int PlayerCount { get; private set; }

		#endregion

		#region Private Properties

		SourceRcon.SourceRcon Rcon { get; set; }

		#endregion

		#region Constructors

		public Server( Guid uniqueId, string friendlyName, string address, string password, string rconPassword )
			: this( uniqueId, friendlyName, address, password, DefaultPort, rconPassword )
		{
		}

		public Server( Guid uniqueId, string friendlyName, string address, string password, ushort port, string rconPassword )
		{
			this.UniqueId = uniqueId;
			this.FriendlyName = friendlyName;
			this.Address = address;
			this.Password = password;
			this.Port = port;
			this.RconPassword = rconPassword;
			PlayerCount = -1;
		}

		#endregion

		public void StartPug( Guid pugId, Map map )
		{
			if (!Rcon.Connected)
			{
				Rcon.Connect( new IPEndPoint( Dns.GetHostAddresses( Address )[0], (int)Port ), RconPassword );
				System.Threading.Thread.Sleep( 1000 );
			}

			Rcon = new SourceRcon.SourceRcon();
			Rcon.ConnectionSuccess += new SourceRcon.BoolInfo( Rcon_ConnectionSuccess );
			Rcon.ServerOutput += new SourceRcon.StringOutput( Rcon_ServerOutput );
			Rcon.Connect( new IPEndPoint( Dns.GetHostAddresses( Address )[0], (int)Port ), RconPassword );

			System.Threading.Thread.Sleep( 1000 );
			
			Rcon.ServerCommand( String.Format( "sm_pug_id {0}", pugId ) );
			Rcon.ServerCommand( String.Format( "changelevel {0}", map.Name ) );
			//Rcon.Disconnect();
			Rcon = null;
		}

		public void RefreshStatus()
		{
			PlayerCount = -1;

			Rcon = new SourceRcon.SourceRcon();
			Rcon.ConnectionSuccess += new SourceRcon.BoolInfo( Rcon_ConnectionSuccess );
			Rcon.ServerOutput += new SourceRcon.StringOutput( Rcon_ServerOutput );
			Rcon.Connect( new IPEndPoint( Dns.GetHostAddresses( Address )[0], (int)Port ), RconPassword );

			System.Threading.Thread.Sleep( 1000 );
			//Rcon.Disconnect();
			//Rcon = null;

			return;

			if (!Rcon.Connected)
				Rcon.Connect( new IPEndPoint( Dns.GetHostAddresses( Address )[0], (int)Port ), RconPassword );
			else
			{
				Rcon.ServerCommand( @"status" );
			}
		}

		#region Event Handlers

		void Rcon_ServerOutput( string output )
		{
			string y = null;
			if (StatusCommand.IsMatch( output ))
			{
				string rawPlayerCount = StatusCommandPlayers.Match( output ).Value;
				if (!String.IsNullOrEmpty( rawPlayerCount ))
					PlayerCount = Int32.Parse( rawPlayerCount.Substring( 10 ).TrimEnd(), System.Globalization.CultureInfo.InvariantCulture );
			}
			else
				y = "no";
			string s = output;
		}

		void Rcon_ConnectionSuccess( bool info )
		{
			Rcon.ServerCommand( @"status" );
		}

		#endregion

	}
}
