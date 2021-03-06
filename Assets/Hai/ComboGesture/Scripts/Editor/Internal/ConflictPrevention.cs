﻿using System;
using Hai.ComboGesture.Scripts.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class ConflictPrevention
    {
        public bool ShouldGenerateExhaustiveAnimations { get; }
        public bool ShouldWriteDefaults { get; }

        private static readonly ConflictPrevention CopyAnimationsWithWriteDefaults = new ConflictPrevention(false, true);
        private static readonly ConflictPrevention GenerateExhaustiveAnimationsWithWriteDefaults = new ConflictPrevention(true, true);
        private static readonly ConflictPrevention GenerateExhaustiveAnimationsWithoutWriteDefaults = new ConflictPrevention(true, false);

        // As the purpose of this class is to describe an enumeration,
        // the boolean parameter coding convention is purposefully broken here.
        private ConflictPrevention(bool shouldGenerateExhaustiveAnimations, bool shouldWriteDefaults)
        {
            ShouldGenerateExhaustiveAnimations = shouldGenerateExhaustiveAnimations;
            ShouldWriteDefaults = shouldWriteDefaults;
        }

        public static ConflictPrevention Of(ConflictPreventionMode mode)
        {
            switch (mode)
            {
                case ConflictPreventionMode.UseRecommendedConfiguration:
                    return GenerateExhaustiveAnimationsWithWriteDefaults;
                case ConflictPreventionMode.OnlyWriteDefaults:
                    return CopyAnimationsWithWriteDefaults;
                case ConflictPreventionMode.GenerateAnimationsWithWriteDefaults:
                    return GenerateExhaustiveAnimationsWithWriteDefaults;
                case ConflictPreventionMode.GenerateAnimationsWithoutWriteDefaults:
                    return GenerateExhaustiveAnimationsWithoutWriteDefaults;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
