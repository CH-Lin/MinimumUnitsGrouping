#region License Information (GPL v3)

/**
 *  This file is part of Minimum Unit Grouping Project.
 *  Copyright (C) 2020 Che-Hung Lin
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion License Information (GPL v3)

using System.Collections.Generic;

namespace MinimumUnit.Grouping
{
    enum DistanceFuncTpye
    {
        Fixed,
        FuzzyInference
    }

    interface IDistanceFunc
    {
        int Calculate(List<MinimumUnit> minimumUnitList, int idx);
    }

    class DistanceFunctions
    {
        private static Dictionary<DistanceFuncTpye, IDistanceFunc> Funcs = new Dictionary<DistanceFuncTpye, IDistanceFunc>()
        {
            {FixNumberDistanceFunc.type, new FixNumberDistanceFunc()},
            {FuzzyInferenceDistanceFunc.type, new FuzzyInferenceDistanceFunc()}
        };

        public static IDistanceFunc GetDistanceFunc(DistanceFuncTpye type)
        {
            try
            {
                return Funcs[type];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }

    class FixNumberDistanceFunc : IDistanceFunc
    {
        public static readonly DistanceFuncTpye type = DistanceFuncTpye.Fixed;
        int IDistanceFunc.Calculate(List<MinimumUnit> minimumUnitList, int idx)
        {
            return 15;
        }
    }

    class FuzzyInferenceDistanceFunc : IDistanceFunc
    {
        public static readonly DistanceFuncTpye type = DistanceFuncTpye.FuzzyInference;
        int IDistanceFunc.Calculate(List<MinimumUnit> minimumUnitList, int idx)
        {
            return 0;
        }

        #region Fuzzy inference
        // Fuzzy membership functions





        #endregion
    }
}
