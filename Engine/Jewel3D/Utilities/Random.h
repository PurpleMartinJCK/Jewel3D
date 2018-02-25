// Copyright (c) 2017 Emilian Cioca
#pragma once
#include "Jewel3D/Reflection/Reflection.h"

namespace Jwl
{
	struct vec3;

	void SeedRandomNumberGenerator();
	void SeedRandomNumberGenerator(unsigned seed);

	float RandomRange(float min, float max);
	int RandomRange(int min, int max);

	// Returns a random unit-length vector.
	vec3 RandomDirection();
	// Returns a random color with [0, 1] RGB values.
	vec3 RandomColor();

	struct Range
	{
		Range() = default;
		Range(float min, float max);

		// Potential range is 'value' +- half the 'deviation'.
		static Range Deviation(float value, float deviation);

		float Random() const;
		void Set(float min, float max);

		bool Contains(float value) const;

		float min = 0.0f;
		float max = 1.0f;
	};
}

REFLECT(Jwl::Range)<>,
	MEMBERS <
		REF_MEMBER(min)<>,
		REF_MEMBER(max)<>
	>
REF_END;
