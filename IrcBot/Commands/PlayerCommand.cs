using System;
using System.Text.RegularExpressions;
using TF2Pug;

namespace IrcBot.Commands
{
	/// <summary>
	/// Base class for all IRC commands
	/// </summary>
	public abstract class PlayerCommand
	{
		/// <summary>
		/// Control character used by players to interface with the bot. If a users sends a string to the channel that starts with this character the text should be treated as a command.
		/// </summary>
		public static char ControlChar { get { return '.'; } }

		protected Pug CurrentPug { get; private set; }

		#region Private properties

		/// <summary>
		/// Minimum user level required to execute this command.
		/// </summary>
		protected virtual int UserLevel { get { return 0; } }

		protected IrcBot IrcBot { get; private set; }

		IrcPlayer CurrentPlayer { get; set; }

		protected virtual Regex Command { get { return null; } }

		#endregion

		public PlayerCommand( Pug currentPug, IrcBot bot )
		{
			this.CurrentPug = currentPug;
			this.IrcBot = bot;
		}


		/// <summary>
		/// Attempts to process the given command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="player"></param>
		/// <returns>True if the command was applicable to this class; otherwise, false.</returns>
		public virtual bool ProcessCommand( string command, IrcPlayer player )
		{
			if (!Command.IsMatch(command))
				return false;

			if (player.UserLevel < UserLevel)
			{
				player.Kick( @"You lack the necessary access to execute that command." );
				return false;
			}


			return true;
		}
	}
}
