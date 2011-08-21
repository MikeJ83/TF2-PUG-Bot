using System;
using System.Configuration;

namespace TF2Pug
{
	public class PugConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty( "servers" )]
		[ConfigurationCollection( typeof( ServersCollection ) )]
		public ServersCollection Servers
		{
			get
			{
				ServersCollection serversCollection = (ServersCollection)base["servers"];
				return serversCollection;
			}
		}

		[ConfigurationProperty( "playerThreshold" )]
		public int PlayerThreshold
		{
			get
			{
				return (int)this["playerThreshold"];
			}
			set
			{
				this["playerThreshold"] = value;
			}
		}

		[ConfigurationProperty( "mumbleInfo" )]
		public string MumbleInfo
		{
			get
			{
				return (string)this["mumbleInfo"];
			}
			set
			{
				this["mumbleInfo"] = value;
			}
		}

		[ConfigurationProperty( "gameSurgeAuth" )]
		public string GameSurgeAuth
		{
			get
			{
				return (string)this["gameSurgeAuth"];
			}
			set
			{
				this["gameSurgeAuth"] = value;
			}
		}

		[ConfigurationProperty( "gameSurgeAuthPassword" )]
		public string GameSurgeAuthPassword
		{
			get
			{
				return (string)this["gameSurgeAuthPassword"];
			}
			set
			{
				this["gameSurgeAuthPassword"] = value;
			}
		}
	}

	public class ServersCollection : ConfigurationElementCollection
	{
		public ServersCollection()
		{
			
		}

		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ServerElement();
		}

		protected override Object GetElementKey( ConfigurationElement element )
		{
			return ((ServerElement)element).FriendlyName;
		}

		public ServerElement this[int index]
		{
			get
			{
				return (ServerElement)BaseGet( index );
			}
			set
			{
				if (BaseGet( index ) != null)
				{
					BaseRemoveAt( index );
				}
				BaseAdd( index, value );
			}
		}

		new public ServerElement this[string FriendlyName]
		{
			get
			{
				return (ServerElement)BaseGet( FriendlyName );
			}
		}

		public int IndexOf( ServerElement server )
		{
			return BaseIndexOf( server );
		}

		public void Add( ServerElement server )
		{
			BaseAdd( server );
		}
		protected override void BaseAdd( ConfigurationElement element )
		{
			BaseAdd( element, false );
		}

		public void Remove( ServerElement server )
		{
			if (BaseIndexOf( server ) >= 0)
				BaseRemove( server.FriendlyName );
		}

		public void RemoveAt( int index )
		{
			BaseRemoveAt( index );
		}

		public void Remove( string name )
		{
			BaseRemove( name );
		}

		public void Clear()
		{
			BaseClear();
		}
	}

	public class ServerElement : ConfigurationElement
	{
		public ServerElement( string address, string friendlyName, string password, ushort port, string rconPassword )
		{
			Address = address;
			FriendlyName = friendlyName;
			Password = password;
			Port = port;
			RconPassword = rconPassword;
		}

		public ServerElement()
		{
			Address = null;
			FriendlyName = null;
			Password = null;
			Port = 27015;
			RconPassword = null;
		}

		[ConfigurationProperty( "address", IsRequired = true )]
		public string Address
		{
			get
			{
				return (string)this["address"];
			}
			set
			{
				this["address"] = value;
			}
		}

		[ConfigurationProperty( "friendlyName", IsRequired = true, IsKey = true )]
		public string FriendlyName
		{
			get
			{
				return (string)this["friendlyName"];
			}
			set
			{
				this["friendlyName"] = value;
			}
		}

		[ConfigurationProperty( "port", DefaultValue = (ushort)27015 )]
		public ushort Port
		{
			get
			{
				return (ushort)this["port"];
			}
			set
			{
				this["port"] = value;
			}
		}

		[ConfigurationProperty( "password", IsRequired = true )]
		public string Password
		{
			get
			{
				return (string)this["password"];
			}
			set
			{
				this["password"] = value;
			}
		}

		[ConfigurationProperty( "rconPassword", IsRequired = true )]
		public string RconPassword
		{
			get
			{
				return (string)this["rconPassword"];
			}
			set
			{
				this["rconPassword"] = value;
			}
		}
	}
}
