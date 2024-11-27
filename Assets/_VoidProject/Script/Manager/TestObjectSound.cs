using UnityEngine;

namespace VoidProject
{
    public class TestObjectSound : MonoBehaviour
    {
        [SerializeField] private Transform[] randomObject;
        private int ghostAppearClipIndex;   // 유령 등장 사운드의 SoundManager 인덱스
        private int ghostChaseClipIndex;    // 유령 추적 사운드의 SoundManager 인덱스
        private int ghostDisappearClipIndex; // 유령 소멸 사운드의 SoundManager 인덱스
        private int ambientClipIndex;       // 유령 주변 앰비언트 사운드의 SoundManager 인덱스

        private bool isChasing = false;    // 추적 상태 플래그

        void Start()
        {
            // 유령 주변 앰비언트 사운드 루프 재생
            if (ambientClipIndex >= 0)
            {
                SoundManager.Instance.PlayClipByIndex(ambientClipIndex, 1f);
            }

            InvokeRepeating(nameof(RandomPlaySound), 2f, 10f);
        }

        private void RandomPlaySound()
        {
            PlayGhostAppearSoundAtPosition(randomObject[Random.Range(0, randomObject.Length)].position);
        }

        public void PlayGhostAppearSoundAtPosition(Vector3 position)
        {
            if (ghostAppearClipIndex >= 0)
            {
                SoundManager.Instance.PlayClipAtPoint(
                    (int)SoundClipString.MonsterSound3,
                    position,
                    1f
                );
            }
        }

        // 유령이 나타날 때
        public void PlayGhostAppearSound()
        {
            if (ghostAppearClipIndex >= 0)
            {
                SoundManager.Instance.PlayClipByIndex(ghostAppearClipIndex, 1f);
            }
        }

        // 유령이 플레이어를 추적할 때
        public void PlayChaseSound()
        {
            if (ghostChaseClipIndex >= 0 && !isChasing)
            {
                isChasing = true;
                SoundManager.Instance.PlayClipByIndex(ghostChaseClipIndex, 1f);
            }
        }

        // 유령이 추적을 멈출 때
        public void StopChaseSound()
        {
            if (isChasing)
            {
                isChasing = false;
                SoundManager.Instance.StopClipByIndex(ghostChaseClipIndex); // 추적 사운드 정지 (매니저를 통한 제어)
            }
        }

        // 유령이 사라질 때
        public void PlayGhostDisappearSound()
        {
            if (ghostDisappearClipIndex >= 0)
            {
                SoundManager.Instance.PlayClipByIndex(ghostDisappearClipIndex, 1f);
            }
        }
    }
}