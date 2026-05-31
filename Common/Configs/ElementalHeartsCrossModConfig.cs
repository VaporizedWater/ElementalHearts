using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Per-source-mod toggles for cross-mod hearts. Disabling a mod stops its hearts from
/// loading at all — they can't be crafted, dropped, or found. Entries require a reload
/// because the heart load gate reads them straight from disk before configs are
/// normally available.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsCrossModConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsCrossModConfig Instance => ModContent.GetInstance<ElementalHeartsCrossModConfig>();

	[Header("SupportedMods")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableCalamityHearts;

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableThoriumHearts;

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableConsolariaHearts;

	/// <summary>Internal-mod-name → config-field-name. Single source of truth for the mapping.</summary>
	private static readonly Dictionary<string, string> FieldBySourceMod = new()
	{
		["CalamityMod"] = nameof(EnableCalamityHearts),
		["ThoriumMod"]  = nameof(EnableThoriumHearts),
		["Consolaria"]  = nameof(EnableConsolariaHearts),
	};

	// The load gate fires once per cross-mod heart (~100 calls), but the config file is the
	// same for all of them within a single mod load. Parse it exactly once and reuse the
	// result; a config change is [ReloadRequired], which rebuilds the assembly and resets
	// these statics, so no manual invalidation is needed.
	private static bool _configParsed;
	private static JObject? _configJson;

	private static JObject? LoadConfigOnce()
	{
		if (_configParsed)
			return _configJson;

		_configParsed = true;
		try
		{
			string path = Path.Combine(Main.SavePath, "ModConfigs",
				$"{nameof(ElementalHearts)}_{nameof(ElementalHeartsCrossModConfig)}.json");

			if (File.Exists(path))
				_configJson = JObject.Parse(File.ReadAllText(path));
		}
		catch
		{
			_configJson = null;
		}

		return _configJson;
	}

	/// <summary>
	/// True if hearts from <paramref name="sourceMod"/> should be loaded. Reads the
	/// config file directly from disk (once, cached) because the heart load gate fires
	/// before the config system is ready. Defaults to true on any error or missing entry
	/// so a fresh install loads everything.
	/// </summary>
	public static bool ShouldLoadHeartsFor(string sourceMod)
	{
		if (!FieldBySourceMod.TryGetValue(sourceMod, out string fieldName))
			return true;

		JObject? json = LoadConfigOnce();
		if (json == null)
			return true;

		JToken token = json[fieldName];
		return token == null || token.ToObject<bool>();
	}
}
