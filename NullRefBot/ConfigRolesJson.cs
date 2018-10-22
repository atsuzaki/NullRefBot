using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NullRefBot
{
	public class ConfigRolesJson
	{
		[JsonProperty("roles")] public RoleInfo[] Roles;

		public class RoleInfo
		{
			[JsonProperty("name")]
			public string Name;

			[JsonProperty("id")]
			public ulong Id;

			[JsonProperty("prereq_roles")]
			public ulong[] PreRequiredRoles;
		}
	}
}