using System;
using Meebey.SmartIrc4net;

namespace IrcBot
{
	public class IrcPlayer : TF2Pug.Player
	{
		IrcClient ircClient;

		/// <summary>
		/// Privilege level of the user, current values are: 0 = normal, 1 = voice, 2 = op
		/// </summary>
		public int UserLevel { get; set; }

		public IrcPlayer( string id, string name, IrcClient client, int userLevel )
			: base( id, name )
		{
			UserLevel = userLevel;
			ircClient = client;
		}

		public override void SendMessage( string message )
		{
			ircClient.SendMessage(SendType.Notice, this.Name, message );
		}

		public void Kick( string message )
		{
			ircClient.RfcKick( IrcBot.Channel, Name, message, Priority.Low );
		}
	}
}
