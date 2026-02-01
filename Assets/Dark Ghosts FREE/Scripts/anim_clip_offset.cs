using UnityEngine;

namespace namespace_animclip_offset
{
    public class anim_clip_offset : MonoBehaviour
    {
        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            // Ensure the animator has evaluated at least one frame
            animator.Update(0f);
            Play_Animationclip_offset();
        }

        void Play_Animationclip_offset()
        {
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips == null || clips.Length == 0 || clips[0].clip == null)
                return;

            AnimationClip animClip = clips[0].clip;

            float normalizedTime = Random.value; // 0..1
            int stateHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            animator.Play(stateHash, 0, normalizedTime);
        }
    }
}
