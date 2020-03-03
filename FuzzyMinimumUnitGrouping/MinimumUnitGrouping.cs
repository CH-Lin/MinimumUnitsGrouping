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

using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Emgu.CV.Structure;

namespace MinimumUnit.Grouping
{
    /// <summary>
    /// Number only include digital numbers, e.g., 10, 100, 999.
    /// </summary>
    enum UnitType
    {
        CJK,
        Latin,
        Number,
        Punctuation,
        CharWithJoin,
        TableLine
    }

    /// <summary>
    /// Quick look up the type of punctuations
    /// </summary>
    class PunctuationLookup
    {
        private static readonly HashSet<string> Stop = new HashSet<string> { ":", ".", "。" }; // kind of punctuations that can stop grouping
        private static readonly HashSet<string> Special = new HashSet<string> { ",", ":", ".", "/", "／" }; //special type
        private static readonly HashSet<string> Split = new HashSet<string> { "、", ";", "!", "?", "。", "！", "？" };
        private static readonly HashSet<string> Connection = new HashSet<string> { "-", "'", "・", "@" };
        private static readonly HashSet<string> LeftBracket = new HashSet<string> { "(", "⦅", "{", "[", "<", "（", "「", "『", "［", "【", "＜", "｟", "〚", "｛", "《", "⟪", "【", "〖", "〈" };
        private static readonly HashSet<string> RightBracket = new HashSet<string> { ")", "⦆", "}", "]", ">", "）", "」", "』", "］", "】", "＞", "｠", "〛", "｝", "》", "⟫", "】", "〗", "〉" };
        private static readonly string verticalBar = "|";

        public static bool IsVerticalBat(string text)
        {
            return text.Equals(verticalBar);
        }

        public static bool IsPunctuations(string text)
        {
            return IsSpecial(text) || IsJoin(text) || IsSplit(text) || IsLeftBracket(text) || IsRightBracket(text);
        }

        public static bool IsStop(string text)
        {
            return Stop.Contains(text);
        }

        public static bool IsSpecial(string text)
        {
            return Special.Contains(text);
        }
        public static bool IsJoin(string text)
        {
            return Connection.Contains(text);
        }

        public static bool IsSplit(string text)
        {
            return Split.Contains(text);
        }

        public static bool IsLeftBracket(string text)
        {
            return LeftBracket.Contains(text);
        }

        public static bool IsRightBracket(string text)
        {
            return RightBracket.Contains(text);
        }
    }

    #region MinimumUnit
    /// <summary>
    /// MinimumUnit class for segmentation grouping
    /// </summary>
    class MinimumUnit
    {
        public Point UpLeft { get; set; }
        public Point UpRight { get; set; }
        public Point DownRight { get; set; }
        public Point DownLeft { get; set; }
        public string Text { get; set; }
        public bool Break { get; set; }
        public float Confidence { get; set; }
        public UnitType Type { get; set; }

        private MinimumUnit PrependedPunctuation = null;

        private MinimumUnit AppendedPunctuation = null;

        public string GetText()
        {
            string PrependedText = (PrependedPunctuation != null) ? PrependedPunctuation.Text : "";
            string ApppendedText = (AppendedPunctuation != null) ? AppendedPunctuation.Text : "";
            string LatinText = (Type == UnitType.Latin) ? " " : "";
            return PrependedText + Text + ApppendedText + LatinText;
        }

        public int GetWidth()
        {
            return GetRealUpRight().X - GetRealUpLeft().X;
        }

        public int GetHeight()
        {
            return GetRealUpRight().Y - GetRealDownRight().Y;
        }

        #region methord to get four real points
        public Point GetRealUpLeft()
        {
            return (PrependedPunctuation != null) ? PrependedPunctuation.UpLeft : UpLeft;
        }

        public Point GetRealUpRight()
        {
            return (AppendedPunctuation != null) ? AppendedPunctuation.UpRight : UpRight;
        }

        public Point GetRealDownRight()
        {
            return (AppendedPunctuation != null) ? AppendedPunctuation.DownRight : DownRight;
        }

        public Point GetRealDownLeft()
        {
            return (PrependedPunctuation != null) ? PrependedPunctuation.DownLeft : DownLeft;
        }
        #endregion

        #region handle punctuations
        public bool IsPunctuations()
        {
            return PunctuationLookup.IsPunctuations(Text);
        }

        public bool IsLeftBracket()
        {
            return PunctuationLookup.IsLeftBracket(Text);
        }

        public bool IsRightBracket()
        {
            return PunctuationLookup.IsRightBracket(Text);
        }

        public bool IsSpecial()
        {
            return PunctuationLookup.IsSpecial(Text);
        }

        public bool IsStop()
        {
            return PunctuationLookup.IsStop(Text);
        }

        public bool IsJoin()
        {
            return PunctuationLookup.IsJoin(Text);
        }

        public bool IsSplit()
        {
            return PunctuationLookup.IsSplit(Text);
        }

        /// <summary>
        /// Insert content, specified by the parameter, to the beginning
        /// </summary>
        /// <param name="unit"></param>
        public void Prepend(MinimumUnit unit)
        {
            PrependedPunctuation = unit;
        }

        /// <summary>
        /// Insert content, specified by the parameter, to the beginning
        /// </summary>
        /// <param name="text"></param>
        /// <param name="UpLeft"></param>
        /// <param name="DownLeft"></param>
        public void Prepend(String text, Point UpLeft, Point DownLeft)
        {
            PrependedPunctuation = new MinimumUnit
            {
                Text = text,
                UpLeft = UpLeft,
                DownLeft = DownLeft
            };
        }

        /// <summary>
        /// Insert content, specified by the parameter, to the end
        /// </summary>
        /// <param name="unit"></param>
        public void Append(MinimumUnit unit)
        {
            AppendedPunctuation = unit;
        }

        /// <summary>
        /// Insert content, specified by the parameter, to the end
        /// </summary>
        /// <param name="text"></param>
        /// <param name="UpRight"></param>
        /// <param name="DownRight"></param>
        public void Append(String text, Point UpRight, Point DownRight)
        {
            AppendedPunctuation = new MinimumUnit
            {
                Text = text,
                UpRight = UpRight,
                DownRight = DownRight
            };
        }

        /// <summary>
        /// Remove prepend punctuation
        /// </summary>
        public void DiscardPrepend()
        {
            this.PrependedPunctuation = null;
        }

        /// <summary>
        /// Remove append punctuation
        /// </summary>
        public void DiscardAppend()
        {
            this.AppendedPunctuation = null;
        }

        /// <summary>
        /// Get text of prepend punctuation
        /// </summary>
        /// <returns></returns>
        public string GetPrependText()
        {
            return (this.PrependedPunctuation != null) ? this.PrependedPunctuation.Text : "";
        }
        #endregion
    }
    #endregion

    class LineSearchHelper
    {
        private readonly LineSegment2DF[] Lines = null;

        public LineSearchHelper(LineSegment2DF[] lines)
        {
            this.Lines = lines;
            // TO-DO, add necessary catch for lines
        }

        public bool ExistLineOnTheRegion(Rectangle rectangle)
        {
            // TO-DO, implement O(1) line search here
            return false;
        }
    }

    class GroupedRegion
    {
        public string Text { get; set; }
        public int Confidence { get; set; }
        public Rectangle Bounds { get; set; }

        public GroupedRegion(string Text, int Confidence, Rectangle Bounds)
        {
            this.Text = Text;
            this.Confidence = Confidence;
            this.Bounds = Bounds;
        }
    }

    class GroupedResult
    {
        public List<GroupedRegion> GroupedRegions { get; set; }
        public List<GroupedRegion> LowConfidenceRegions { get; set; }
    }

    /// <summary>
    /// MinimumUnitGrouping Algorithm
    /// </summary>
    class MinimumUnitGrouping
    {
        private static MinimumUnitGrouping Instance = null;

        /// <summary>
        /// Get instance of MinimumUnitGrouping.
        /// This function can make sure only one instance in memory and avoid anyone use constructor to create nunecessary instance because constructor is expensive.
        /// </summary>
        /// <param name="ConfidenceScoreThreshold"></param>
        /// <returns></returns>
        public static MinimumUnitGrouping GetInstance(double ConfidenceScoreThreshold)
        {
            if (Instance == null)
            {
                Instance = new MinimumUnitGrouping(ConfidenceScoreThreshold);
            }
            else
            {
                Instance.ConfidenceScoreThreshold = ConfidenceScoreThreshold;
            }
            return Instance;
        }

        public double ConfidenceScoreThreshold { get; set; } = 0.8;

        private MinimumUnitGrouping(double ConfidenceScoreThreshold)
        {
            this.ConfidenceScoreThreshold = ConfidenceScoreThreshold;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumUnitList"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public GroupedResult Group(List<MinimumUnit> minimumUnitList, LineSegment2DF[] lines, DistanceFuncTpye Type = DistanceFuncTpye.Fixed)
        {
            if (minimumUnitList == null)
            {
                return null;
            }

            LineSearchHelper lineSearchHelper = new LineSearchHelper(lines);

            var combined = new List<GroupedRegion>();
            var lowConfidence = new List<GroupedRegion>();
            var allRegions = new List<GroupedRegion>();
            var units = new List<MinimumUnit>();
            var lastXDistance = 0;

            // TO-DO, sort minimum units based on y and x here

            // TO-DO, use table line to enhance grouping
            foreach (var item in minimumUnitList.Select((value, index) => new { value, index }))
            {
                var current = item.value;

                if (units.Count != 0)
                {
                    // previous minimum unit
                    var previous = units.LastOrDefault();
                    MinimumUnit next = null;
                    var xBackDistance = 0;

                    if (previous.IsStop())
                    {
                        allRegions.Add(CombineUnits(units));
                        units.Clear();
                        lastXDistance = 0;
                    }
                    else
                    {
                        if (!current.Equals(minimumUnitList.LastOrDefault()))
                        {
                            // next minimum unit
                            next = minimumUnitList[item.index + 1];
                            xBackDistance = next.GetRealUpLeft().X - current.GetRealUpRight().X;
                        }

                        // check syntax
                        if (current.IsPunctuations())
                        {
                            current.Type = UnitType.Punctuation;
                            bool oneLine = IsSameLine(previous, current);
                            if (oneLine)
                            {
                                if (!current.IsRightBracket())
                                {
                                    if (!ExistLineBetweenUnits(lineSearchHelper, previous, current))
                                    {
                                        if (current.IsStop())
                                        {
                                            // TO-DO, review
                                            current.UpRight = previous.UpRight;
                                            current.DownRight = previous.DownRight;
                                            previous.Break = true;
                                            current.Break = true;
                                            previous.Append(current);
                                            allRegions.Add(CombineUnits(units));
                                            units.Clear();
                                            lastXDistance = 0;
                                        }
                                        else if (current.IsJoin())
                                        {
                                            previous.Append(current);
                                            previous.Type = UnitType.CharWithJoin;
                                        }
                                        else if (current.IsSplit())
                                        {
                                            previous.Append(current);
                                            previous.Break = true;
                                        }
                                        else if (IsSameLine(next, current) && !ExistLineBetweenUnits(lineSearchHelper, current, next))
                                        {
                                            next.Prepend(current);
                                        }
                                    }
                                    else if (IsSameLine(next, current) && !ExistLineBetweenUnits(lineSearchHelper, current, next))
                                    {
                                        next.Prepend(current);
                                    }
                                }
                                else if (!ExistLineBetweenUnits(lineSearchHelper, previous, current))
                                {
                                    previous.Append(current);
                                }
                            }
                            else if (IsSameLine(next, current) && !ExistLineBetweenUnits(lineSearchHelper, current, next))
                            {
                                next.Prepend(current);
                            }
                            continue;
                        }

                        // check distance
                        var xFrontDistance = current.GetRealUpLeft().X - previous.GetRealUpRight().X;
                        var angle = GetAngle(previous.GetRealUpRight(), current.GetRealUpLeft());
                        var isOneWord = CanCombine(xFrontDistance, xBackDistance, lastXDistance, angle,
                                                  DistanceFunctions.GetDistanceFunc(Type).Calculate(minimumUnitList, item.index));
                        lastXDistance = xFrontDistance;

                        // additional checking by table line support
                        if (isOneWord && lines != null)
                        {
                            isOneWord = !ExistLineBetweenUnits(lineSearchHelper, previous, current);
                        }

                        if (!isOneWord)
                        {
                            // create a word and add it to word regions.
                            allRegions.Add(CombineUnits(units));
                            units.Clear();
                            lastXDistance = 0;

                            // line break so separate
                            if (current.Break)
                            {
                                MoveToResultLists(combined, lowConfidence, allRegions, ConfidenceScoreThreshold * 100);
                                allRegions.Clear();
                            }
                        }
                    }
                }
                units.Add(current);
            }

            allRegions.Add(CombineUnits(units));
            MoveToResultLists(combined, lowConfidence, allRegions, ConfidenceScoreThreshold * 100);

            return new GroupedResult
            {
                GroupedRegions = combined,
                LowConfidenceRegions = lowConfidence
            };
        }

        private bool ExistLineBetweenUnits(LineSearchHelper lineSearchHelper, MinimumUnit unit1, MinimumUnit unit2)
        {
            MinimumUnit left, right;
            if (unit1.GetRealUpRight().X < unit2.GetRealUpRight().X)
            {
                left = unit1;
                right = unit2;
            }
            else
            {
                left = unit2;
                right = unit1;
            }
            int width = Math.Abs(right.GetRealUpRight().X - left.GetRealUpLeft().X);
            int height = Math.Abs(Math.Max(right.GetRealDownLeft().Y, left.GetRealDownLeft().Y) - Math.Min(right.GetRealUpLeft().Y, left.GetRealUpLeft().Y));
            Rectangle rect = new Rectangle(left.GetRealUpLeft().X, left.GetRealUpLeft().Y, width, height);

            if (lineSearchHelper.ExistLineOnTheRegion(rect))
            {
                return true;
            }
            return false;
        }

        private bool IsSameLine(MinimumUnit source, MinimumUnit target)
        {
            if (source == null || target == null)
            {
                return false;
            }
            var tuple = GetMinMaxY(source);
            var tuple2 = GetMinMaxY(target);
            var centerPoint1 = GetCenterPoint(source);
            var centerPoint2 = GetCenterPoint(target);
            var centerYDifference = Math.Abs(centerPoint1.Y - centerPoint2.Y);
            var minYDifference = Math.Abs(tuple.Item1 - tuple2.Item1);
            var maxYDifference = Math.Abs(tuple.Item2 - tuple2.Item2);
            if (centerYDifference <= 5 && minYDifference <= 5 && maxYDifference <= 5) return true;
            return false;
        }

        private Tuple<int, int> GetMinMaxY(MinimumUnit unit)
        {
#if DEBUG
            // crash it in debug build to make sure all the caller never pass null and find the caller pass null into this function
            System.Diagnostics.Debug.Assert(unit != null);
#endif
            int lowY = Math.Min(unit.GetRealUpRight().Y, unit.GetRealUpLeft().Y);
            int highY = Math.Max(unit.GetRealDownRight().Y, unit.GetRealDownLeft().Y);
            return new Tuple<int, int>(lowY, highY);
        }

        private Point GetCenterPoint(MinimumUnit unit)
        {
#if DEBUG
            // crash it in debug build to make sure all the caller never pass null and find the caller pass null into this function
            System.Diagnostics.Debug.Assert(unit != null);
#endif
            int centerX = unit.GetRealUpLeft().X + unit.GetRealUpRight().X + unit.GetRealDownRight().X + unit.GetRealDownLeft().X;
            int centerY = unit.GetRealUpLeft().Y + unit.GetRealUpRight().Y + unit.GetRealDownRight().Y + unit.GetRealDownLeft().Y;
            return new Point(centerX / 4, centerY / 4);
        }

        private double GetAngle(Point p1, Point p2)
        {
            var xDiff = p2.X - p1.X;
            var yDiff = p2.Y - p1.Y;
            var angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            return angle >= 180 ? angle - 180 : angle;
        }

        private bool CanCombine(int frontDistance, int backDistance, int lastXDistance, double angle, int threshold)
        {
            if (angle < 30.0)
            {
                frontDistance = Math.Abs(frontDistance);
                backDistance = Math.Abs(backDistance);
                if (backDistance != 0)
                {
                    if (Math.Abs(frontDistance - backDistance) < threshold ||
                        Math.Abs(frontDistance - lastXDistance) < threshold) { return true; }
                }
                else
                {
                    if (Math.Abs(frontDistance - lastXDistance) < threshold) { return true; }
                }
            }
            return false;
        }

        // TO-DO, quickest and simplest bounding box calculation, no need to check all vertex because it is rectangle
        private GroupedRegion CombineUnits(List<MinimumUnit> symbols)
        {
            // text
            var text = string.Join("", symbols.Select(s => s.GetText()));

            // TO-DO, speed up bounding box calculation
            int minX = 0, maxX = 0, minY = Int32.MaxValue, maxY = Int32.MinValue;
            if (symbols.Count > 0)
            {
                minX = symbols[0].GetRealUpLeft().X;
                maxX = symbols.Last().GetRealUpRight().X;
                foreach (var symbol in symbols)
                {
                    if (symbol.GetRealUpLeft().Y < minY)
                    {
                        minY = symbol.GetRealUpLeft().Y;
                    }
                    if (symbol.GetRealDownLeft().Y > maxY)
                    {
                        maxY = symbol.GetRealDownLeft().Y;
                    }
                }
            }
            var width = maxX - minX;
            var height = maxY - minY;
            if (!string.IsNullOrEmpty(text)) // seems not necessary, need to be review
            {
                width = width <= 0 ? 1 : width;
                height = height <= 0 ? 1 : height;
            }
            var bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            // confidence
            var average = symbols.Sum(r => r.Confidence) / (double)symbols.Count();
            var confidence = (int)Math.Round(average * 100);
            return new GroupedRegion(text, confidence, bounds);
        }

        /// <summary>
        /// This function move data from source list to two lists based on confidence score threshold
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="lowConfidenceList"></param>
        /// <param name="sourceList"></param>
        /// <param name="confidenceScoreThreshold"></param>
        private void MoveToResultLists(List<GroupedRegion> resultList, List<GroupedRegion> lowConfidenceList,
                            List<GroupedRegion> sourceList, double confidenceScoreThreshold)
        {
            foreach (var region in sourceList)
            {
                if (region.Confidence <= confidenceScoreThreshold)
                {
                    lowConfidenceList.Add(region);
                }
                else
                {
                    resultList.Add(region);
                }
            }
        }
    }
}
