using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            float distToTarget = (target.position - ps.transform.position).magnitude;

            var main = ps.main;
            main.startColor = ParticlesColor;
            float speed = distToTarget / main.startLifetime.constant;
            int length;
            bool atleastOnePart = false;
            ps.Play();
            if (modifiedParticles == null)
            {
                modifiedParticles = new ParticleSystem.Particle[1000];
            }
            while (!cancellationToken.CancellationRequested || atleastOnePart)
            {
                atleastOnePart = false;
                length = ps.GetParticles(modifiedParticles);
                distToTarget = (target.position - ps.transform.position).magnitude;
                speed = distToTarget / main.startLifetime.constant;
                vecToTarget = ps.transform.InverseTransformPoint(target.position).normalized * speed;

                for (var i = 0; i < length; i++)
                {
                    modifiedParticles[i].velocity = vecToTarget;
                    if (modifiedParticles[i].remainingLifetime > 0f)
                        atleastOnePart = true;
                }
                if(cancellationToken.CancellationRequested)
                    ps.Stop();
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
