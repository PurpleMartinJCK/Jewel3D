#pragma once
#include "Jewel3D/Resource/Encoder.h"

class Encoder : public Jwl::Encoder
{
public:
	Encoder();

	Jwl::ConfigTable GetDefault() const override;

	bool Validate(const Jwl::ConfigTable& metadata, unsigned loadedVersion) const override;

	bool Convert(std::string_view source, std::string_view destination, const Jwl::ConfigTable& metadata) const override;
};
