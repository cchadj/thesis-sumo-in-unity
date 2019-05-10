/* 
 * Smallest enclosing circle - Library (C#)
 * 
 * Copyright (c) 2018 Project Nayuki
 * https://www.nayuki.io/page/smallest-enclosing-circle
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using Random = System.Random;


public sealed class SmallestEnclosingCircle
{
	private static readonly Random Rand = new Random();
	private static readonly Point[] Points = new Point[4]
		{ new Point(), new Point(), new Point(), new Point()};
	private static readonly Point[] shuffledPoints = new Point[4]
		{ new Point(), new Point(), new Point(), new Point()};
	/* 
	 * Returns the smallest circle that encloses all the given points. Runs in expected O(n) time, randomized.
	 * Note: If 0 points are given, a circle of radius -1 is returned. If 1 point is given, a circle of radius 0 is returned.
	 * Assumes that points are in the x,z plane
	 */
	public static Circle MakeUnityCircle([NotNull] Vector3[] points)
	{
		if (points == null) throw new ArgumentNullException(nameof(points));
		var ps = points.Select(x => new Point(x.x, x.z)).ToList();
		return MakeCircle(ps);
	}
	
	/*
	 *   Slightly optimised for exactly four points. Almost non alloc because of new circle.
	 */
	public static Circle MakeUnityCircleFourPointsNonAlloc([NotNull] Vector3[] points)
	{
		Assert.AreEqual(points.Length, 4, "There must be exactly 4 points given");
		
		for (int i = 0; i < 4; i++)
		{
			Points[i].x = points[i].x;
			Points[i].y = points[i].z;
		}
		return MakeCircle(Points);
	}
	
	/* 
	 * Returns the smallest circle that encloses all the given points. Runs in expected O(n) time, randomized.
	 * Note: If 0 points are given, a circle of radius -1 is returned. If 1 point is given, a circle of radius 0 is returned.
	 */
	// Initially: No boundary points known
	public static Circle MakeCircle(IList<Point> points) {
		// Clone list to preserve the caller's data, do Durstenfeld shuffle
		List<Point> shuffled = new List<Point>(points);
		
		for (int i = shuffled.Count - 1; i > 0; i--) {
			int j = Rand.Next(i + 1);
			Point temp = shuffled[i];
			shuffled[i] = shuffled[j];
			shuffled[j] = temp;
		}
		
		// Progressively add points to circle or recompute circle
		Circle c = Circle.INVALID;
		for (int i = 0; i < shuffled.Count; i++) {
			Point p = shuffled[i];
			if (c.r < 0 || !c.Contains(p))
				c = MakeCircleOnePoint(shuffled.GetRange(0, i + 1), p);
		}
		return c;
	}
	
	
	
	// One boundary point known
	private static Circle MakeCircleOnePoint(List<Point> points, Point p) {
		Circle c = new Circle(p, 0);
		for (int i = 0; i < points.Count; i++) {
			Point q = points[i];
			if (!c.Contains(q)) {
				if (c.r == 0)
					c = MakeDiameter(p, q);
				else
					c = MakeCircleTwoPoints(points.GetRange(0, i + 1), p, q);
			}
		}
		return c;
	}
	
	
	// Two boundary points known
	private static Circle MakeCircleTwoPoints(List<Point> points, Point p, Point q) {
		Circle circ = MakeDiameter(p, q);
		Circle left  = Circle.INVALID;
		Circle right = Circle.INVALID;
		
		// For each point not in the two-point circle
		Point pq = q.Subtract(p);
		foreach (Point r in points) {
			if (circ.Contains(r))
				continue;
			
			// Form a circumcircle and classify it on left or right side
			float cross = pq.Cross(r.Subtract(p));
			Circle c = MakeCircumcircle(p, q, r);
			if (c.r < 0)
				continue;
			else if (cross > 0 && (left.r < 0 || pq.Cross(c.c.Subtract(p)) > pq.Cross(left.c.Subtract(p))))
				left = c;
			else if (cross < 0 && (right.r < 0 || pq.Cross(c.c.Subtract(p)) < pq.Cross(right.c.Subtract(p))))
				right = c;
		}
		
		// Select which circle to return
		if (left.r < 0 && right.r < 0)
			return circ;
		else if (left.r < 0)
			return right;
		else if (right.r < 0)
			return left;
		else
			return left.r <= right.r ? left : right;
	}
	
	
	public static Circle MakeDiameter(Point a, Point b) {
		Point c = new Point((a.x + b.x) / 2, (a.y + b.y) / 2);
		return new Circle(c, Math.Max(c.Distance(a), c.Distance(b)));
	}
	
	
	public static Circle MakeCircumcircle(Point a, Point b, Point c) {
		// Mathematical algorithm from Wikipedia: Circumscribed circle
		float ox = (Math.Min(Math.Min(a.x, b.x), c.x) + Math.Max(Math.Min(a.x, b.x), c.x)) / 2;
		float oy = (Math.Min(Math.Min(a.y, b.y), c.y) + Math.Max(Math.Min(a.y, b.y), c.y)) / 2;
		float ax = a.x - ox,  ay = a.y - oy;
		float bx = b.x - ox,  by = b.y - oy;
		float cx = c.x - ox,  cy = c.y - oy;
		float d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
		if (d == 0)
			return Circle.INVALID;
		float x = ((ax*ax + ay*ay) * (by - cy) + (bx*bx + by*by) * (cy - ay) + (cx*cx + cy*cy) * (ay - by)) / d;
		float y = ((ax*ax + ay*ay) * (cx - bx) + (bx*bx + by*by) * (ax - cx) + (cx*cx + cy*cy) * (bx - ax)) / d;
		Point p = new Point(ox + x, oy + y);
		float r = Math.Max(Math.Max(p.Distance(a), p.Distance(b)), p.Distance(c));
		return new Circle(p, r);
	}
	
}



public struct Circle {
	
	public static readonly Circle INVALID = new Circle(new Point(0, 0), -1);
	
	private const float MULTIPLICATIVE_EPSILON = 1f+ 1e-14f;
	
	
	public Point c;   // Center
	public float r;  // Radius
	
	
	public Circle(Point c, float r) {
		this.c = c;
		this.r = r;
	}
	
	
	public bool Contains(Point p) {
		return c.Distance(p) <= r * MULTIPLICATIVE_EPSILON;
	}
	
	
	public bool Contains(ICollection<Point> ps) {
		foreach (Point p in ps) {
			if (!Contains(p))
				return false;
		}
		return true;
	}
	
}



public struct Point {
	
	public float x;
	public float y;
	
	
	public Point(float x, float y) {
		this.x = x;
		this.y = y;
	}
	
	
	public Point Subtract(Point p) {
		return new Point(x - p.x, y - p.y);
	}
	
	
	public float Distance(Point p) {
		float dx = x - p.x;
		float dy = y - p.y;
		return Mathf.Sqrt(dx * dx + dy * dy);
	}
	
	
	// Signed area / determinant thing
	public float Cross(Point p) {
		return x * p.y - y * p.x;
	}
	
}