using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MastersOfTempest
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticlesSucker : MonoBehaviour
    {
        private ParticleSystem ps;
        private ParticleSystem.Particle[] modifiedParticles;
        public Color ParticlesColor;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            if (ps == null)
            {
                throw new InvalidOperationException($"{nameof(ps)} is not specified!");
            }
            var main = ps.main;
            main.startColor = ParticlesColor;
        }

        private CoroutineCancellationToken currentToken;
        public void StartChannel(Transform target, CoroutineCancellationToken cancellationToken)
        {
            if (!currentToken?.CancellationRequested ?? false)
            {
                throw new InvalidOperationException("Cannot start new channeling before ending previous!");
            }
            currentToken = cancellationToken;
            StartCoroutine(ChannelCoroutine(target, currentToken));
        }

        private IEnumerator ChannelCoroutine(Transform target, CoroutineCancellationToken cancellationToken)
        {
            Vector3 vecToTarget;
            float distToTarget;
            int length;
            const float speed = 2f;
            ps.Play();
            if (modifiedParticles == null)
            {
                modifiedParticles = new ParticleSystem.Particle[1000];
            }
            while (!cancellationToken.CancellationRequested)
            {
                length = ps.GetParticles(modifiedParticles);
                vecToTarget = (target.position - transform.position).normalized * speed;
                distToTarget = (target.position - transform.position).sqrMagnitude;
                for (var i = 0; i < length; i++)
                {
                    // if (modifiedParticles[i].position == target.position)
                    if ((modifiedParticles[i].position - transform.position).sqrMagnitude > distToTarget)
                    {
                        modifiedParticles[i].remainingLifetime = 0f;
                        continue;
                    }
                    modifiedParticles[i].velocity = vecToTarget;
                }
                ps.SetParticles(modifiedParticles, length);
                yield return null;
            }
            length = ps.GetParticles(modifiedParticles);
            for (int i = 0; i < length; ++i)
            {
                modifiedParticles[i].remainingLifetime = 0f;
            }
            ps.SetParticles(modifiedParticles, length);
            ps.Stop();
            currentToken = null;
        }
    }
}
