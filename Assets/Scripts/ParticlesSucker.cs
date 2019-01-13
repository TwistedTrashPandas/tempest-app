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
            const float speed = 2f;
            vecToTarget = (target.position - transform.position).normalized * speed;
            ps.Play();
            if(modifiedParticles == null)
            {
                modifiedParticles = new ParticleSystem.Particle[1000];
            }
            while (!cancellationToken.CancellationRequested)
            {
                int length = ps.GetParticles(modifiedParticles);

                for (var i = 0; i < length; i++)
                {
                    if (modifiedParticles[i].position == target.position)
                    {
                        //TODO: make sure it works
                        modifiedParticles[i].remainingLifetime = 0f;
                        continue;
                    }
                    modifiedParticles[i].velocity = vecToTarget;
                }
                ps.SetParticles(modifiedParticles, length);
                yield return null;
            }
            ps.Stop();
            currentToken = null;
        }
    }
}
