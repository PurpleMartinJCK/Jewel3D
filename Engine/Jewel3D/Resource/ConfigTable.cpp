// Copyright (c) 2017 Emilian Cioca
#include "Jewel3D/Precompiled.h"
#include "ConfigTable.h"
#include "Jewel3D/Application/FileSystem.h"
#include "Jewel3D/Utilities/String.h"

namespace Jwl
{
	bool ConfigTable::Load(std::string_view file)
	{
		FileReader input;
		if (!input.OpenAsBuffer(file))
		{
			return false;
		}

		while (!input.IsEOF())
		{
			std::string line = input.GetLine();

			if (line.size() == 0 || line[0] == ':')
				continue;

			// Line must have an '=' to be processed.
			size_t pos = line.find("=");

			// Check that the position is valid and not the last character.
			if (pos == std::string::npos || pos == line.size() - 1)
				continue;

			std::string leftSide = line.substr(0, pos);
			std::string rightSide = line.substr(pos + 1);

			settings[leftSide] = rightSide;
		}

		return true;
	}

	bool ConfigTable::Save(std::string_view file) const
	{
		std::ofstream output(file.data());
		if (!output)
		{
			return false;
		}

		for (auto& [name, value] : settings)
		{
			output << name << "=" << value << '\n';
		}

		output.close();
		return true;
	}

	bool ConfigTable::operator==(const ConfigTable& other) const
	{
		return settings == other.settings;
	}

	bool ConfigTable::operator!=(const ConfigTable& other) const
	{
		return settings != other.settings;
	}

	bool ConfigTable::HasSetting(std::string_view setting) const
	{
		return settings.count(setting);
	}

	unsigned ConfigTable::GetSize() const
	{
		return settings.size();
	}

	std::string ConfigTable::GetString(std::string_view setting) const
	{
		std::string result;
		auto itr = settings.find(setting);

		if (itr != settings.end())
		{
			result = itr->second;
		}

		return result;
	}

	float ConfigTable::GetFloat(std::string_view setting) const
	{
		auto itr = settings.find(setting);
		if (itr != settings.end())
		{
			return std::stof(itr->second);
		}
		else
		{
			return 0.0f;
		}
	}

	int ConfigTable::GetInt(std::string_view setting) const
	{
		auto itr = settings.find(setting);
		if (itr != settings.end())
		{
			return std::stoi(itr->second);
		}
		else
		{
			return 0;
		}
	}

	bool ConfigTable::GetBool(std::string_view setting) const
	{
		auto itr = settings.find(setting);
		if (itr != settings.end())
		{
			const auto& value = itr->second;

			return
				value == "1" ||
				CompareLowercase(value, "on") || 
				CompareLowercase(value, "true");
		}
		else
		{
			return false;
		}
	}

	void ConfigTable::SetValue(const std::string& setting, std::string_view value)
	{
		settings[setting] = value;
	}

	void ConfigTable::SetValue(const std::string& setting, const char* value)
	{
		settings[setting] = value;
	}

	void ConfigTable::SetValue(const std::string& setting, float value)
	{
		settings[setting] = std::to_string(value);
	}

	void ConfigTable::SetValue(const std::string& setting, int value)
	{
		settings[setting] = std::to_string(value);
	}

	void ConfigTable::SetValue(const std::string& setting, bool value)
	{
		settings[setting] = value ? "on" : "off";
	}

	void ConfigTable::SetDefaultValue(const std::string& setting, std::string_view value)
	{
		if (!HasSetting(setting))
		{
			settings[setting] = value;
		}
	}

	void ConfigTable::SetDefaultValue(const std::string& setting, const char* value)
	{
		if (!HasSetting(setting))
		{
			settings[setting] = value;
		}
	}

	void ConfigTable::SetDefaultValue(const std::string& setting, float value)
	{
		if (!HasSetting(setting))
		{
			settings[setting] = std::to_string(value);
		}
	}

	void ConfigTable::SetDefaultValue(const std::string& setting, int value)
	{
		if (!HasSetting(setting))
		{
			settings[setting] = std::to_string(value);
		}
	}

	void ConfigTable::SetDefaultValue(const std::string& setting, bool value)
	{
		if (!HasSetting(setting))
		{
			settings[setting] = value ? "on" : "off";
		}
	}
}
