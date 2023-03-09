using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoreMath : MonoBehaviour
{
    private void Start()
    {
        Span.TestProgram();
    }
}

public class Span : IEquatable<Span>, IComparable<Span>
{
    public float Center;
    public float Radius;


    private float RightEdge() { return Center + Radius; }
    private float LeftEdge() { return Center - Radius; }


    public Span(float center = 0f, float radius = 1f)
    {
        Center = center;
        Radius = radius;
    }


    public bool IfInside(float distance)
    {
        return Center - Radius < distance && Center + Radius > distance;
    }


    private static bool MergeSpans(Span left, Span right, out Span newSpan)
    {
        newSpan = new();

        if (Mathf.Abs(right.Center - left.Center) > right.Radius + left.Radius)
            return false; // No need to merge spans

        var rightEdge = Mathf.Max(left.RightEdge(), right.RightEdge());
        var leftEdge = Mathf.Min(left.LeftEdge(), right.LeftEdge());

        newSpan.Center = (leftEdge + rightEdge) / 2f;
        newSpan.Radius = rightEdge - newSpan.Center;
        return true;
    }


    public static Span[] MergeSpans(in Span[] spans)
    {
        // 0 or 1 element
        if (spans.Length == 0 || spans.Length == 1) return spans;

        // more elements
        List<Span> spansList = new();

        // Iterate over all spans
        foreach (var span in spans)
        {
            // Find first Span on the right
            int index = spansList.BinarySearch(span);

            if (index >= 0) // Found the same index
            {
                Debug.Log("Found the same span");
                continue; // No need to do anything
            }

            index = ~index; // Index of first element to the right

            if (index == spansList.Count) // End of list
            {
                // Append at the end
                spansList.Add(span);
            }
            else
            {
                // Merge with span on the right
                if (MergeSpans(span, spansList[index], out var newSpan))
                {
                    spansList[index] = newSpan;
                }
                else
                {
                    // Insert to the left of found element
                    spansList.Insert(index, span);
                }
            }
            {
                // Merge with span on the left
                if (index != 0 && MergeSpans(spansList[index - 1], spansList[index], out var newSpan))
                {
                    spansList.RemoveAt(index);
                    spansList[index - 1] = newSpan;
                }
            }
        }
        // Merging:
        // If merging didn't happen => do nothing
        // If merging happened => remove those two Spans and add a resulting one

        return spansList.ToArray();
    }


    public bool Equals(Span other)
    {
        if (other is null)
            return false;

        return this.Center == other.Center && this.Radius == other.Radius;
    }


    // CompareTo is a space ship operator (<=>)
    public int CompareTo(Span other)
    {
        if (other == null) return 1;

        if (this.Center.CompareTo(other.Center) != 0)
            return this.Center.CompareTo(other.Center);
        return this.Radius.CompareTo(other.Radius);
    }


    private static bool Test(Span[] input, Span[] expected)
    {
        return expected.SequenceEqual(MergeSpans(input));
    }


    public static void TestProgram()
    {
        Span[] input = new Span[] { }, expected = new Span[] { };
        if (Test(input, expected) == false) Debug.Log("Test 1 failed");

        input = new Span[] { new() }; expected = new Span[] { new() };
        if (Test(input, expected) == false) Debug.Log("Test 2 failed");

        input = new Span[] { new(0), new(3) }; expected = new Span[] { new(0), new(3) };
        if (Test(input, expected) == false) Debug.Log("Test 3 failed");

        input = new Span[] { new(0), new(2) }; expected = new Span[] { new(1, 2) };
        if (Test(input, expected) == false) Debug.Log("Test 4 failed");

        input = new Span[] { new(0), new(3) }; expected = new Span[] { new(0), new(3) };
        if (Test(input, expected) == false) Debug.Log("Test 5 failed");

        input = new Span[] { new(0), new(3), new(1.5f) }; expected = new Span[] { new(1.5f, 2.5f) };
        if (Test(input, expected) == false) Debug.Log("Test 6 failed");

        input = new Span[] { new(0), new(4), new(2) }; expected = new Span[] { new(2, 3) };
        if (Test(input, expected) == false) Debug.Log("Test 7 failed");

        input = new Span[] { new(0), new(2) }; expected = new Span[] { new(0), new(2) };
        if (Test(input, expected) == true) Debug.Log("Test 8 should fail");

        Debug.Log("Test end");
    }
}
