using System.Collections;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BrittleHolds {
    public class BrittleHold : MonoBehaviour {
        private static Cfg config {
            get => Plugin.instance.config;
        }

        private int maxHp = config.maxHp.Value;
        private int hp = config.maxHp.Value;

        public AudioSource source;

        private float minVolume = 0.12f;
        private float maxVolume = 0.15f;

        private float minDurationWarn = 1.2f;
        private float maxDurationWarn = 1.4f;

        private float minDurationBreak = 2.2f;
        private float maxDurationBreak = 2.4f;

        private IEnumerator crumbleCoroutine;

        public bool destroyed {
            get => hp <= 0;
        }

        private IEnumerator PlayCrumbleSound(float duration) {
            yield return new WaitForSeconds(0.5f);

            float timer = duration;

            while (timer > 0f) {
                float normalized = timer / duration;

                source.volume = Mathf.Lerp(0, source.volume, normalized);
                timer -= Time.deltaTime;

                yield return null;
            }

            crumbleCoroutine = null;
        }

        private IEnumerator Disable() {
            yield return new WaitForSeconds(10f);
            gameObject.SetActive(false);
        }

        public void Grab() {
            // Reduce HP
            if (hp > 0) {
                hp--;
            }

            if (crumbleCoroutine != null) {
                StopCoroutine(crumbleCoroutine);
                crumbleCoroutine = null;
            }

            if (destroyed == false) {
                if (hp <= maxHp / 2) {
                    source.volume = Random.Range(minVolume, maxVolume);
                    crumbleCoroutine = PlayCrumbleSound(
                        Random.Range(minDurationWarn, maxDurationWarn)
                    );
                    StartCoroutine(crumbleCoroutine);
                    source.Play();
                }

                return;
            }

            source.volume = Random.Range(minVolume, maxVolume);
            crumbleCoroutine = PlayCrumbleSound(
                Random.Range(minDurationBreak, maxDurationBreak)
            );
            StartCoroutine(crumbleCoroutine);
            source.Play();

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) {
                return;
            }

            gameObject.tag = "Untagged";
            rb.isKinematic = false;

            StartCoroutine(Disable());
        }
    }
}
