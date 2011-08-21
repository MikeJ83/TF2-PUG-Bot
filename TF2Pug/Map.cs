using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2Pug
{
	public class Map
	{
		public Guid UniqueId { get; set; }
		public string Name { get; set; }

		public string FriendlyName { get; set; }

		public Map( Guid uniqueId, string name, string friendlyName )
		{
			this.UniqueId = uniqueId;
			this.Name = name;
			this.FriendlyName = friendlyName;
		}
	}
}
