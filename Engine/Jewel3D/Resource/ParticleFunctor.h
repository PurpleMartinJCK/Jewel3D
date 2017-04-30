// Copyright (c) 2017 Emilian Cioca
#pragma once
#include "ParticleBuffer.h"
#include "Shareable.h"
#include "Jewel3D/Utilities/Random.h"

#include <vector>

namespace Jwl
{
	class ParticleEmitter;

	//- Base functor class for changing particle behaviour or spawning particles.
	class ParticleFunctor
	{
	public:
		virtual ~ParticleFunctor() = default;
		//- Called to, optionally, initialize newly spawned particles.
		//- New particles will be in the specified index range.
		virtual void Init(ParticleBuffer& particles, ParticleEmitter& emitter, unsigned startIndex, unsigned count) {};
		//- Main update call for the functor.
		virtual void Update(ParticleBuffer& particles, ParticleEmitter& emitter, float deltaTime) = 0;
		//- Specifies the required data buffers for this functor to update.
		virtual ParticleBuffers GetRequirements() const { return ParticleBuffers::None; };
	};

	//- A list of ParticleFunctors acting on a ParticleEmitter.
	class FunctorList
	{
		friend ParticleEmitter;
	public:
		FunctorList() = default;
		FunctorList(const FunctorList&);
		FunctorList& operator=(const FunctorList&);

		template<class T>
		void Add(std::shared_ptr<T> ptr)
		{
			static_assert(std::is_base_of<ParticleFunctor, T>::value, "Template argument must inherit from ParticleFunctor.");
			static_assert(std::is_base_of<Shareable<T>, T>::value, "A ParticleFunctor must inherit from Shareable<>.");

			functors.push_back(ptr);
			dirty = true;
		}

		//- Removes all functors.
		void Clear();

		const auto& GetAll() const { return functors; }

	private:
		std::vector<std::shared_ptr<ParticleFunctor>> functors;

		bool dirty = true;
	};

	/* Default Jewel3D Particle Functors */
	//- Rotates particles.
	class RotationFunc : public ParticleFunctor, public Shareable<RotationFunc>
	{
		friend ShareableAlloc;
		RotationFunc() = default;
		RotationFunc(float rotationSpeed, RandomRange initialRotation = RandomRange(180.0f, 180.0f));

	public:
		virtual void Init(ParticleBuffer& particles, ParticleEmitter& emitter, unsigned startIndex, unsigned count) override;
		virtual void Update(ParticleBuffer& particles, ParticleEmitter& emitter, float deltaTime) override;

		virtual ParticleBuffers GetRequirements() const final override;

		float rotationSpeed = 5.0f;
		RandomRange initialRotation{ 180.0f, 180.0f };
	};
}
