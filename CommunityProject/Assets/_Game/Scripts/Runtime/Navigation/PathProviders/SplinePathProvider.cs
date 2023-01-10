using System.Collections.Generic;
using System.Linq;
using BoundfoxStudios.CommunityProject.Extensions;
using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Navigation.PathProviders
{
	/// <summary>
	///   The <see cref="SplinePathProvider" /> will create a path through a <see cref="ISplineContainer" />.
	/// </summary>
	public class SplinePathProvider
	{
		public ISpline CreatePath(ISplineContainer container, ISplineLinkDecisionMaker splineLinkDecisionMaker,
			int startSplineIndex = 0, int startKnotIndex = 0)
		{
			var pathSlices = new List<SplineSlice<Spline>>();
			var endKnot = container.Splines[startSplineIndex][^1];
			var splineIndex = startSplineIndex;
			var knotIndex = startKnotIndex;

			// Fail safe in case ISplineContainer contains splines and links that eventually will not lead to the end.
			var maxIterations = container.Splines.Sum(spline => spline.Count);
			var iteration = 0;

			BezierKnot lastKnotInSlice;

			do
			{
				var traverseResult = TryTraverseToNextKnotLink(container, splineIndex, knotIndex);
				pathSlices.Add(traverseResult.SplineSlice);
				lastKnotInSlice = traverseResult.SplineSlice[^1];

				// Traverse to next knot on the current spline.
				// Will be overwritten, when the algorithm finds another spline to traverse.
				knotIndex = traverseResult.SplineSlice.Range.End + 1;

				if (TrySelectSplineForFurtherTraversal(container, splineLinkDecisionMaker, traverseResult.LinkedKnots,
						out var nextSpline, out var nextKnot))
				{
					splineIndex = nextSpline;
					knotIndex = nextKnot;
				}

				iteration++;
			} while (!endKnot.Equals(lastKnotInSlice) && iteration < maxIterations);

			if (iteration >= maxIterations)
			{
				throw new(
					"Did not find a valid path, this happens most likely when the splines contain links but are not linked in a way, the path provider can traverse to the final knot in the main spline.");
			}

			return new Spline(new SplinePath(pathSlices));
		}

		private bool TrySelectSplineForFurtherTraversal(
			ISplineContainer container,
			ISplineLinkDecisionMaker splineLinkDecisionMaker,
			IReadOnlyList<SplineKnotIndex> linkedKnots,
			out int splineIndex,
			out int knotIndex)
		{
			splineIndex = -1;
			knotIndex = -1;

			// If the knot contains no links, we can not select any other spline.
			if (linkedKnots is null)
			{
				return false;
			}

			// If we do have knots, we must check if they have at least one more knot that is traversable.
			// Otherwise we may hit the end of another spline (e.g. if that one merges back to the main spline).
			var candidates = linkedKnots
				.Where(linkedKnot => container.Splines[linkedKnot.Spline].IsValidKnotIndex(linkedKnot.Knot + 1))
				.ToArray();

			if (candidates.Length == 0)
			{
				return false;
			}

			var candidate = splineLinkDecisionMaker.Decide(candidates);

			splineIndex = candidate.Spline;

			// We add 1 here, otherwise we'd try to traverse the same knot again and would get stuck.
			knotIndex = candidate.Knot + 1;
			return true;
		}

		/// <summary>
		///   This method will traverse a spline starting from <see cref="startKnotIndex" />.
		///   It will traverse until it reaches a knot link.
		///   If no knot link is found, it will traverse to the end of the spline.
		/// </summary>
		/// <returns>
		///   Tuple with a <see cref="SplineSlice{T}" /> that is the part of the traversed spline.
		///   It will include the knot at <see cref="startKnotIndex" /> and either the link knot or the end knot of the spline.
		///   The tuple possibly includes a <see cref="IReadOnlyList{T}" /> containing a list of <see cref="SplineKnotIndex" />.
		/// </returns>
		private (SplineSlice<Spline> SplineSlice, IReadOnlyList<SplineKnotIndex> LinkedKnots) TryTraverseToNextKnotLink(
			ISplineContainer container, int splineIndex, int startKnotIndex)
		{
			var spline = container.Splines[splineIndex];
			var links = container.KnotLinkCollection;

			var knotIndex = startKnotIndex;
			IReadOnlyList<SplineKnotIndex> linkedKnots = null;

			while (knotIndex < spline.Count)
			{
				if (links.TryGetKnotLinks(new(splineIndex, knotIndex), out linkedKnots))
				{
					knotIndex++;
					break;
				}

				knotIndex++;
			}

			return (
				new(spline, new(startKnotIndex, knotIndex - startKnotIndex)),
				linkedKnots
			);
		}
	}
}
