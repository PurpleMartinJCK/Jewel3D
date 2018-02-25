// Copyright (c) 2017 Emilian Cioca
#pragma once
#include "Resource.h"
#include "Jewel3D/Reflection/Reflection.h"

namespace Jwl
{
	// An audio clip.
	// To be played, the clip must be set on an Entity's SoundSource component.
	// A SoundListener must also be present in the scene.
	class Sound : public Resource<Sound>
	{
	public:
		~Sound();

		// Loads .wav (must be 8-bit unsigned PCM).
		bool Load(std::string filePath);
		void Unload();

		unsigned GetBufferHandle() const;

	private:
		unsigned hBuffer = 0;
	};
}

REFLECT_SHAREABLE(Jwl::Sound)
REFLECT(Jwl::Sound) < Resource >
REF_END;
