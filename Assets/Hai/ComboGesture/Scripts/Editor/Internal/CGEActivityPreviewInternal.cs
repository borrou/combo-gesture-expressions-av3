﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeActivityPreviewInternal
    {

        private readonly Action _onClipRenderedFn;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeBlendTreeEffector _blendTreeEffector;
        private readonly Dictionary<AnimationClip, Texture2D> _animationClipToTextureDict;
        private readonly Dictionary<AnimationClip, Texture2D> _animationClipToTextureDictGray;
        private readonly int _pictureWidth;
        private readonly int _pictureHeight;
        private readonly Action _onQueueTaskComplete;
        private readonly AnimationClip[] _editorArbitraryAnimations;

        public CgeActivityPreviewInternal(Action onClipRenderedFn, CgeEditorEffector editorEffector, CgeBlendTreeEffector blendTreeEffector, Dictionary<AnimationClip, Texture2D> animationClipToTextureDict, Dictionary<AnimationClip, Texture2D> animationClipToTextureDictGray, int pictureWidth, int pictureHeight, Action onQueueTaskComplete)
        {
            _onClipRenderedFn = onClipRenderedFn;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
            _animationClipToTextureDict = animationClipToTextureDict;
            _animationClipToTextureDictGray = animationClipToTextureDictGray;
            _pictureWidth = pictureWidth;
            _pictureHeight = pictureHeight;
            _onQueueTaskComplete = onQueueTaskComplete;
            _editorArbitraryAnimations = _editorEffector.GetActivity()?.editorArbitraryAnimations ?? new AnimationClip[]{};
        }

        public enum ProcessMode
        {
            RecalculateEverything, CalculateMissing
        }

        public void Process(ProcessMode processMode, AnimationClip prioritize)
        {
            if (AnimationMode.InAnimationMode())
            {
                return;
            }

            var clipDictionary = GatherAnimations(processMode);
            var animationPreviews = ToPrioritizedList(clipDictionary, prioritize);

            new CgePreviewProcessor(_editorEffector.PreviewSetup(), animationPreviews, OnClipRendered, _onQueueTaskComplete).Capture();
        }

        private void OnClipRendered(AnimationPreview animationPreview)
        {
            _animationClipToTextureDict[animationPreview.Clip] = animationPreview.RenderTexture;
            _animationClipToTextureDictGray[animationPreview.Clip] = GrayscaleCopyOf(animationPreview.RenderTexture);
            _onClipRenderedFn.Invoke();
        }

        private static Texture2D GrayscaleCopyOf(Texture2D originalTexture)
        {
            var texture = UnityEngine.Object.Instantiate(originalTexture);
            var mipCount = Mathf.Min(3, texture.mipmapCount);

            for (var mip = 0; mip < mipCount; ++mip)
            {
                var cols = texture.GetPixels(mip);
                for (var i = 0; i < cols.Length; ++i)
                {
                    var value = (cols[i].r + cols[i].g + cols[i].b) / 3f;
                    cols[i] = new Color(value, value, value);
                }
                texture.SetPixels(cols, mip);
            }
            texture.Apply(false);

            return texture;
        }

        private static List<AnimationPreview> ToPrioritizedList(Dictionary<AnimationClip, Texture2D> clipDictionary, AnimationClip prioritize)
        {
            if (prioritize != null && clipDictionary.ContainsKey(prioritize))
            {
                var animationPreviews = clipDictionary.Where(pair => pair.Key != prioritize)
                    .Select(pair => new AnimationPreview(pair.Key, pair.Value))
                    .ToList();
                animationPreviews.Insert(0, new AnimationPreview(prioritize, clipDictionary[prioritize]));

                return animationPreviews;
            }

            return clipDictionary
                .Select(pair => new AnimationPreview(pair.Key, pair.Value))
                .ToList();
        }


        private Dictionary<AnimationClip, Texture2D> GatherAnimations(ProcessMode processMode)
        {
            var enumerable = _editorArbitraryAnimations
                .Union(_editorEffector.AllDistinctAnimations())
                .Union(_blendTreeEffector.AllAnimationsOfSelected())
                .Distinct()
                .Where(clip => clip != null);

            if (processMode == ProcessMode.CalculateMissing)
            {
                enumerable = enumerable.Where(clip => !_animationClipToTextureDict.ContainsKey(clip));
            }

            return new HashSet<AnimationClip>(enumerable.ToList())
                    .ToDictionary(clip => clip, clip => CgePreviewProcessor.NewPreviewTexture2D(_pictureWidth, _pictureHeight));
        }
    }
}
