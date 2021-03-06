// Copyright (c) 2017 Emilian Cioca
#pragma once
#include "Jewel3D/Resource/Encoder.h"

class MeshEncoder : public Jwl::Encoder
{
public:
	MeshEncoder();

	Jwl::ConfigTable GetDefault() const override;

	bool Validate(const Jwl::ConfigTable& metadata, unsigned loadedVersion) const override;

	bool Convert(std::string_view source, std::string_view destination, const Jwl::ConfigTable& metadata) const override;

	bool Upgrade(Jwl::ConfigTable& metadata, unsigned loadedVersion) const override;
};
