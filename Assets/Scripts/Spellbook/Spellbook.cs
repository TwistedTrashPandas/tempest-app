using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest
{
    public class Spellbook : MonoBehaviour
    {
        public SkinnedMeshRenderer leftPage;
        public SkinnedMeshRenderer rightPage;
        public SkinnedMeshRenderer frontPage;
        public SkinnedMeshRenderer backPage;

        public Texture2D[] pageTextures;

        private Animator animator;
        private bool inTransition;
        private int page;

        void Awake()
        {
            animator = GetComponent<Animator>();
            inTransition = false;
            page = 0;

            SetTextureOfPage(leftPage, 0);
            SetTextureOfPage(rightPage, 1);
        }

        public void OpenOrClose()
        {
            animator.SetTrigger("OpenOrClose");
        }

        public void OpenOrClose(float delay)
        {
            StartCoroutine(WaitToOpen(delay));
        }

        private IEnumerator WaitToOpen(float delay)
        {
            yield return new WaitForSeconds(delay);
            OpenOrClose();
        }

        public void NextPage()
        {
            if (!inTransition && page < pageTextures.Length - 2)
            {
                animator.SetTrigger("NextPage");
                inTransition = true;
                page += 2;
            }
        }

        public void PreviousPage()
        {
            if (!inTransition && page > 1)
            {
                animator.SetTrigger("PreviousPage");
                inTransition = true;
                page -= 2;
            }
        }

        public void NextPageBegin()
        {
            SetTextureOfPage(backPage, page - 1);
            SetTextureOfPage(frontPage, page);
            SetTextureOfPage(rightPage, page + 1);
        }

        public void NextPageEnd()
        {
            SetTextureOfPage(leftPage, page);
            inTransition = false;
        }

        public void PreviousPageBegin()
        {
            SetTextureOfPage(frontPage, page + 2);
            SetTextureOfPage(backPage, page + 1);
            SetTextureOfPage(leftPage, page);
        }

        public void PreviousPageEnd()
        {
            SetTextureOfPage(rightPage, page + 1);
            inTransition = false;
        }

        private void SetTextureOfPage(SkinnedMeshRenderer page, int index)
        {
            page.material.mainTexture = pageTextures[index];
        }
    }
}
