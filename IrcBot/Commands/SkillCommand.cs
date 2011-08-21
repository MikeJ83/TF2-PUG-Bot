using System;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TF2Pug;

namespace IrcBot.Commands
{
	public class SkillCommand : PlayerCommand
	{
		public SkillCommand( Pug currentPug, IrcBot bot )
			: base( currentPug, bot )
		{
			b_command = new Regex( @"^skill", RegexOptions.Compiled | RegexOptions.IgnoreCase );
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
				string playerSkill = null;

				foreach (PlayerClass currentClass in CurrentPug.Players.Keys)
				{
					foreach (Player currentPlayer in CurrentPug.Players[currentClass])
					{
						playerSkill += String.Format( ", {0} ({1})", currentPlayer.Name, currentPlayer.Skill );
					}
				}
				if (!String.IsNullOrEmpty( playerSkill ))
				{
					IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, playerSkill.Substring( 2 ) );
				}
				else
				{
					IrcBot.IrcClient.SendMessage( SendType.Message, IrcBot.Channel, @"No players are added" );
				}
				return true;
			}

			return false;
		}
	}
}
